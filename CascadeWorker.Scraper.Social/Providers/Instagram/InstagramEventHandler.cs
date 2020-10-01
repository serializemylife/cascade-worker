using CascadeWorker.Scraper.Accounts;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts.Media;
using CascadeWorker.Scraper.Validation;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Bugsnag;
using CascadeWorker.Logging;
using CascadeWorker.Scraper.Notifications;
using CascadeWorker.Scraper.Settings;
using CascadeWorker.Shared;
using CascadeWorker.Shared.Extentions;
using CascadeWorker.Scraper.Queue;
using Newtonsoft.Json;

namespace CascadeWorker.Scraper.Social.Providers.Instagram
{
    public class InstagramEventHandler : AbstractSocialEventHandler, ISocialEventHandler
    {
        private bool _loginNeeded = true;

        private readonly IWebDriver _webDriver;
        private readonly ScraperAccountDao _accountDao;
        private readonly InstagramProfileDao _profileDao;
        private readonly ScraperSettings _scraperSettings;
        private readonly IScraperValidator _scraperValidator;
        private readonly ILogger _logger;
        private readonly NotificationSender _notificationSender;

        private ScraperAccount _currentAccount;
        private string _currentItem;
        private int _itemsProcessedSinceLogin;

        public InstagramEventHandler(
            IWebDriver webDriver, 
            ScraperAccountDao accountRepository, 
            InstagramProfileDao profileDao, 
            ScraperSettings scraperSettings, 
            IScraperValidator scraperValidator, 
            ILogger logger,
            NotificationSender notificationSender)
        {
            _webDriver = webDriver;
            _accountDao = accountRepository;
            _profileDao = profileDao;
            _scraperSettings = scraperSettings;
            _scraperValidator = scraperValidator;
            _logger = logger;
            _notificationSender = notificationSender;
        }

        public override string GetLoginPageUrl()
        {
            return "https://instagram.com/accounts/login";
        }

        public override string GetLogoutPageUrl()
        {
            return "https://instagram.com/accounts/logout";
        }

        public override void NavigateToProfile()
        {
            _webDriver.Navigate().GoToUrl(_currentItem);
        }

        public override bool TryWaitForProfileToLoad()
        {
            return _webDriver.WaitUntilVisible(By.XPath(InstagramDomSelectors.ProfileFollowers), 3) != null;
        }

        public override bool IsLoginNeeded()
        {
            return _loginNeeded;
        }

        public override void Login()
        {
            if (!_loginNeeded)
            {
                return;
            }

            if (_webDriver.PageSource.Contains("js logged-in"))
            {
                Logout();
            }

            _webDriver.Navigate().GoToUrl(GetLoginPageUrl());

            if (!TryGetAccount(out _currentAccount))
            {
                WaitUntilAccountAvailable();
                Login();
                return;
            }

            if (IsProfileVisitsThrottled())
            {
                // For some reason, Instagram started displaying the "wait a few minutes before you try again" message on login :(
                
                _logger.Warning("The login page has been throttled, switching accounts.");
                
                Logout();
                Login();
                
                return;
            }

            var usernameField = _webDriver.WaitUntilVisible(By.Name("username"), 5);
            var passwordField = _webDriver.WaitUntilVisible(By.Name("password"), 0);

            if (usernameField == null || passwordField == null)
            {
                throw new Exception("Failed to find the required login form fields.");
            }

            usernameField.SendKeys(_currentAccount.Username);
            passwordField.SendKeys(_currentAccount.Password);
            passwordField.SendKeys(Convert.ToString(Convert.ToChar((object)57351)));

            if (_webDriver.WaitUntilVisible(By.Id("slfErrorAlert"), 3) != null)
            {
                OnFailedLogin();
                return;
            }

            if (_webDriver.PageSource.Contains("challengeType") || _webDriver.PageSource.Contains("RecaptchaChallengeForm") || _webDriver.PageSource.Contains("Help Us Confirm It's You"))
            {
                OnFailedLogin();
                return;
            }

            _itemsProcessedSinceLogin = 0;
            _loginNeeded = false;
        }

        private void OnFailedLogin()
        {
            _accountDao.MarkAccountAsDisabled(_currentAccount.Id);
            
            Logout();
            Login();
        }

        private void WaitUntilAccountAvailable()
        {
            _logger.Warning($"Worker {StaticState.WorkerId} is waiting for an available account.");

            var waitingSince = DateTime.Now;
            var waitTicks = 0;
            
            while (!TryGetAccount(out var _, false))
            {
                // If we've been waiting for an account for longer than 10 minutes, lets send a notification as a warning.
                
                Thread.Sleep(TimeSpan.FromSeconds(30));
                waitTicks++;
            }
            
            if (waitTicks >= 20)
            {
                _notificationSender.SendNotification($"Worker {StaticState.WorkerId} has been waiting for an account since {waitingSince.ToShortTimeString()}.", "829YaL");
            }
            
            _logger.Success("We found an available account, lets do this.");
        }

        public override ISocialProfile CreateProfile()
        {
            var bio = GetBio();
            var name = GetName();

            var profile =  new InstagramProfile(_profileDao, _scraperSettings, _scraperValidator)
            {
                Url = GetUrl(),
                Username = GetUsername(),
                Name = name,
                Picture = GetProfilePicture(),
                Bio = bio,
                IsPrivate = GetIsPrivate(),
                FollowerCount = GetFollowerCount(),
                FollowingCount = GetFollowingCount(),
                SnapchatFound = 
                    _scraperValidator.StringContainsSnapchatUsername(bio.ToLower()) || 
                    _scraperValidator.StringContainsSnapchatUsername(name.ToLower())
            };

            if (_scraperValidator.TryExtractAgeFromString(bio, out var ageGuessed))
            {
                profile.AgeGuessed = ageGuessed;
            }

            if (_scraperValidator.StringContainsFemaleName(name.CleanQueueItem()) || _scraperValidator.StringContainsFemaleName(profile.Username.CleanQueueItem()))
            {
                profile.GenderGuessed = 'F';
            }
            else
            {
                profile.GenderGuessed = 'M';
            }

            if (!profile.SnapchatFound)
            {
                return profile;
            }
            
            if (_scraperValidator.TryExtractSnapchatUsernameFromString(bio.ToLower(), out var snapchatUsername) ||
                _scraperValidator.TryExtractSnapchatUsernameFromString(name.ToLower(), out snapchatUsername))
            {
                profile.SnapchatUsername = snapchatUsername;
            }

            return profile;
        }

        private bool TryGetAccount(out ScraperAccount profile, bool markAsFetched = true)
        {
            return _accountDao.TryFindAccount(
                out profile,
                1,
                int.Parse(_scraperSettings["minutes_to_wait_since_account_throttled"]),
                int.Parse(_scraperSettings["minutes_to_wait_since_account_fetched"]),
                markAsFetched);
        }

        public override void Logout()
        {
            _loginNeeded = true;
            _webDriver.Navigate().GoToUrl(GetLogoutPageUrl());
        }

        private long GetProfileId()
        {
            var jse = (IJavaScriptExecutor) _webDriver;
            var profileIdFromSharedData = jse.ExecuteScript("return window._sharedData.entry_data.ProfilePage[0].logging_page_id;")?.ToString();

            if (profileIdFromSharedData != null && long.TryParse(profileIdFromSharedData.Split("_").Last(), out var profileId))
            {
                return profileId;
            }
            
            var username = new Uri(_currentItem).AbsolutePath.Replace("/", "");
                
            var possibleProfileId = _webDriver.PageSource.GetInbetween(
                "window.__additionalDataLoaded('/" + username + "/',{\"logging_page_id\":\"", "\",");

            if (string.IsNullOrEmpty(possibleProfileId) || !long.TryParse(possibleProfileId.Split("_").Last(), out profileId))
            {
                throw new Exception("Failed to find the unique ID for the profile.");
            }

            return profileId;
        }
        
        public override List<InstagramConnection> GetConnections()
        {
            var connections = new List<InstagramConnection>();
            var endCursor = "";

            while (true)
            {
                if (connections.Count >= int.Parse(_scraperSettings["maximum_connections_to_scrape_for_profile"]))
                {
                    break; // We've got enough
                }

                var profileId = GetProfileId();

                if (profileId < 1)
                {
                    throw new Exception("Failed to find the unique ID for the profile.");
                }
                
                var jse = (IJavaScriptExecutor) _webDriver;
                var csrfToken = jse.ExecuteScript("return window._sharedData.config.csrf_token;")?.ToString();
                
                HttpResponseMessage httpResponse;
                
                try
                {
                    httpResponse = InstagramConnectionCollector.GetConnectionsFromApi(profileId, csrfToken, _webDriver.Manage().Cookies.AllCookies, endCursor);
                }
                catch (AggregateException)
                {
                    SwitchAccount(true);
                    continue;
                }

                if (httpResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _logger.Warning($"Connections have been throttled for {_currentAccount.Username} ({_currentAccount.Id}).");
                    
                    SwitchAccount(true);
                    continue;
                }

                var responseText = httpResponse.Content.ReadAsStringAsync().Result;

                if (string.IsNullOrEmpty(responseText))
                {
                    _logger.Warning("Instagram API returned an empty string for connection nodes.");
                    return new List<InstagramConnection>();
                }

                if (responseText.Contains("https://www.instagram.com/challenge/?next="))
                {
                    _logger.Warning($"Connections have been challenged for {_currentAccount.Username} ({_currentAccount.Id}).");
                    
                    _accountDao.MarkAccountAsDisabled(_currentAccount.Id);
                    
                    Logout();
                    Login();
                    
                    NavigateToProfile();
                    continue;
                }
                
                // TODO: Validate if it contains valid json.

                var responseJsonObject = JObject.Parse(responseText);
                
                var connectionNodes = JsonConvert.DeserializeObject<JArray>(responseJsonObject["data"]["user"]["edge_followed_by"]["edges"].ToString());

                foreach (var connection in connectionNodes)
                {
                    connections.Add(new InstagramConnection
                    {
                        Item = "https://instagram.com/" + connection["node"]["username"],
                        IsPrivate = connection["node"]["is_private"].ToString() == "True",
                    });
                }

                if (responseJsonObject["data"]["user"]["edge_followed_by"]["page_info"]["has_next_page"].ToString().ToLower() != "true")
                {
                    break;
                }

                endCursor = responseJsonObject["data"]["user"]["edge_followed_by"]["page_info"]["end_cursor"].ToString();

                Thread.Sleep(1000);
            }

            return connections;
        }
        
        public override string GetUrl()
        {
            return _webDriver.Url;
        }

        public override string GetUsername()
        {
            return _webDriver.WaitUntilVisible(By.XPath(InstagramDomSelectors.ProfileUsername), 0)?.GetAttribute("innerHTML") ?? "";
        }

        public override string GetName()
        {
            return _webDriver.WaitUntilVisible(By.XPath(InstagramDomSelectors.ProfileName), 0)?.GetAttribute("innerHTML") ?? GetUsername();
        }

        public override string GetProfilePicture()
        {
            return _webDriver.WaitUntilVisible(By.XPath(InstagramDomSelectors.ProfilePicture), 0).GetAttribute("src");
        }

        public override string GetBio()
        {
            var bioElement = _webDriver.WaitUntilVisible(By.XPath(InstagramDomSelectors.ProfileBio), 0);
            return bioElement == null ? "" : bioElement.GetAttribute("innerText").Replace(Environment.NewLine, " ");
        }
        public override bool GetIsPrivate()
        {
            return _webDriver.PageSource.Contains("This Account is Private");
        }

        public override int GetFollowerCount()
        {
            return GetFollowBoxCount(InstagramDomSelectors.ProfileFollowers);
        }

        public override int GetFollowingCount()
        {
            return GetFollowBoxCount(InstagramDomSelectors.ProfileFollowing);
        }

        private int GetFollowBoxCount(string xpathSelector)
        {
            var followingElement = _webDriver.WaitUntilVisible(By.XPath(xpathSelector), 0);
            var titleAttributeValue = followingElement.GetAttribute("title");

            if (titleAttributeValue.Length > 0)
            {
                return int.Parse(titleAttributeValue.Replace(",", ""));
            }

            return int.Parse(followingElement.GetAttribute("innerHTML").Replace(",", ""));
        }

        public override List<InstagramPost> GetPosts(string owner)
        {
            return new List<InstagramPost>
            {
                new InstagramPost
                {
                    Media = GetPostMedia(owner)
                }
            };
        }

        public override List<InstagramPostMedia> GetPostMedia(string owner)
        {
            var media = new List<InstagramPostMedia>();

            foreach (var domElement in _webDriver.FindElements(By.XPath(InstagramDomSelectors.ProfilePostPicture)).Take(20))
            {
                if (string.IsNullOrEmpty(domElement.GetProperty("src")))
                {
                    continue;
                }

                media.Add(new InstagramPostMedia
                {
                    CdnUrl = domElement.GetProperty("src"),
                    MetaData = domElement.GetAttribute("alt")
                });
            }

            return media;
        }

        public override void SetCurrentItem(string item)
        {
            _currentItem = item;
            _itemsProcessedSinceLogin++;

            if (_itemsProcessedSinceLogin > int.Parse(_scraperSettings["maximum_scrapes_before_forced_login"]))
            {
                _loginNeeded = true;
            }
        }

        public override List<InstagramConnection> GetFilteredConnections(List<InstagramConnection> connections)
        {
            var filteredConnections = new List<InstagramConnection>();

            foreach (var connection in connections)
            {
                var connectionUrl = connection.Item;
                
                if (!_scraperValidator.StringContainsMaleFirstName(connectionUrl.CleanQueueItem()) && 
                    !_scraperValidator.StringContainsMaleUsername(connectionUrl.CleanQueueItem()) && 
                    !_scraperValidator.StringContainsAnythingBad(connectionUrl) &&
                    !_scraperValidator.StringContainsAnythingBad(connectionUrl.CleanQueueItem()))
                {
                    filteredConnections.Add(connection);
                }
            }

            return filteredConnections;
        }

        public override string GetPageSource()
        {
            return _webDriver.PageSource;
        }

        public override List<ScraperQueueItem> ConvertConnectionsToQueueItems(List<InstagramConnection> connections)
        {
            var queueItems = new List<ScraperQueueItem>();

            foreach (var connection in connections)
            {
                var isConfirmed = true; // todo

                queueItems.Add(new ScraperQueueItem
                {
                    Id = 0,
                    Item = Utilities.GetConsistentSocialUrl(connection.Item),
                    IsPrivate = connection.IsPrivate,
                    Confirmed = isConfirmed
                });
            }

            return queueItems;
        }

        public override bool IsProfileVisitsThrottled()
        {
            return _webDriver.PageSource.Contains("wait a few minutes before you try again");
        }

        public override bool IsProfileNotFound()
        {
            return _webDriver.PageSource.Contains("The link you followed may be broken");
        }

        public override void SwitchAccount(bool markCurrentAsThrottled)
        {
            if (markCurrentAsThrottled)
            {
                _accountDao.MarkAccountAsThrottled(_currentAccount.Id);
            }

            Logout();
            Login();

            NavigateToProfile();
        }
    }
}
