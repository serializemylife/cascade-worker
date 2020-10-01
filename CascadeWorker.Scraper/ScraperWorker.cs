using Bugsnag;
using CascadeWorker.Scraper.Queue;
using CascadeWorker.Scraper.Social;
using CascadeWorker.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CascadeWorker.Logging;
using OpenQA.Selenium;

namespace CascadeWorker.Scraper
{
    public class ScraperWorker
    {
        private readonly Dictionary<string, ISocialEventHandler> _eventHandlers;
        private readonly ScraperWorkerDao _scraperWorkerDao;
        private readonly ScraperQueueDao _scraperQueueDao;
        private readonly Client _bugSnagClient;
        private readonly ILogger _logger;

        private bool _isProcessing;

        public ScraperWorker(
            Dictionary<string, ISocialEventHandler> eventHandlers, 
            ScraperWorkerDao scraperWorkerDao, 
            ScraperQueueDao scraperQueueDao,
            Client bugSnagClient,
            ILogger logger)
        {
            _eventHandlers = eventHandlers;
            _scraperWorkerDao = scraperWorkerDao;
            _scraperQueueDao = scraperQueueDao;
            _bugSnagClient = bugSnagClient;
            _logger = logger;
            
            Process();
        }

        public void Start()
        {
            _isProcessing = true;
        }

        public void Stop()
        {
            _isProcessing = false;
        }

        private void Process()
        {
            while (true)
            {
                RenewWorkerStatus(); // Checks if we have paused the worker via an external service.

                _scraperWorkerDao.UpdateWorkerLastSeen(StaticState.WorkerId); // async
                    
                if (!_isProcessing)
                {
                    _logger.Warning($"The worker is currently paused, sleeping for 30 seconds.");

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }
                    
                if (!_scraperQueueDao.TryGetItemSafe(out var item))
                {
                    _logger.Warning($"The queue is currently empty, sleeping for 30 seconds.");

                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }

                var result = ProcessQueueItem(item);

                if (result.Success)
                {
                    _logger.Success($"Finished processing {item.Item}");
                }
                else
                {
                    _logger.Trace($"Finished processing {item.Item} - " + result.Content);
                }

                Console.WriteLine();
                    
                _scraperQueueDao.MarkItemAsComplete(item.Id); // async
                _scraperQueueDao.StoreItemResult(result); // async
            }
        }

        private void RenewWorkerStatus()
        {
            _isProcessing = _scraperWorkerDao.IsWorkerRunning(StaticState.WorkerId);
        }

        private ScraperQueueItemResult ProcessQueueItem(ScraperQueueItem item)
        {
            var eventHandler = ResolveEventHandlerFromItem(item.Item);
            
            try
            {
                _logger.Trace($"Processing {item.Item}");
                
                eventHandler.SetCurrentItem(item.Item);

                if (eventHandler.IsLoginNeeded())
                {
                    eventHandler.Login();
                }

                eventHandler.NavigateToProfile();

                if (eventHandler.IsProfileVisitsThrottled())
                {
                    eventHandler.SwitchAccount(true);
                    return ProcessQueueItem(item);
                }

                if (!eventHandler.TryWaitForProfileToLoad())
                {
                    if (eventHandler.IsProfileNotFound())
                    {
                        return new ScraperQueueItemResult
                        {
                            ItemId = item.Id,
                            Content = "The profile resulted in a 404.",
                            PageSource = eventHandler.GetPageSource(),
                            Success = false
                        };
                    }
                    
                    return new ScraperQueueItemResult
                    {
                        ItemId = item.Id,
                        Content = "Method 'TryWaitForProfileToLoad' returned false.",
                        PageSource = eventHandler.GetPageSource(),
                        Success = false
                    };
                }

                var profile = eventHandler.CreateProfile();

                if (!profile.ShouldScrape(out var validationResult))
                {
                    return new ScraperQueueItemResult
                    {
                        ItemId = item.Id,
                        Content = validationResult.ToString(),
                        PageSource = eventHandler.GetPageSource(),
                        Success = false
                    };
                }

                if (profile.ShouldCollectConnections())
                {
                    profile.Connections = eventHandler.GetConnections();
                }

                var connectionsToStore = eventHandler.GetFilteredConnections(profile.Connections);
                
                if (connectionsToStore.Any())
                {
                    _logger.Trace($"Collected {profile.Connections.Count} / {profile.FollowerCount}, storing {connectionsToStore.Count} of them in the database.");
                    _scraperQueueDao.StoreItems(eventHandler.ConvertConnectionsToQueueItems(connectionsToStore), profile.Id);
                }

                if (profile.ShouldSave(out validationResult))
                {
                    if (!profile.IsPrivate)
                    {
                        profile.Posts = eventHandler.GetPosts(profile.Username);
                    }

                    profile.Save();

                    return new ScraperQueueItemResult
                    {
                        ItemId = item.Id,
                        Content = "success",
                        Success = true
                    };
                }

                return new ScraperQueueItemResult
                {
                    ItemId = item.Id,
                    Content = validationResult.ToString(),
                    PageSource = eventHandler.GetPageSource(),
                    Success = false,
                };
            }
            catch (WebDriverException e)
            {
                BugReport(e, item.Item, eventHandler.GetUrl(), eventHandler.GetPageSource());

                return new ScraperQueueItemResult
                {
                    ItemId = item.Id,
                    Content = e.Message,
                    PageSource = eventHandler.GetPageSource(),
                    Success = false
                };
            }
        }

        private void BugReport(Exception e, string queueItem, string currentUrl, string pageSource)
        {
            _bugSnagClient.Notify(e, report =>
            {
                report.Event.Metadata.Add("queue_item", queueItem);
                report.Event.Metadata.Add("current_url", currentUrl);
                report.Event.Metadata.Add("page_source", pageSource);
                report.Event.Metadata.Add("created_at", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            });
        }

        private ISocialEventHandler ResolveEventHandlerFromItem(string item)
        {
            var host = new Uri(item).Host.Replace("www.", "");

            if (_eventHandlers.ContainsKey(host))
            {
                return _eventHandlers[host];
            }

            throw new Exception($"Failed to resolve event handler for host '{host}'");
        }
    }
}
