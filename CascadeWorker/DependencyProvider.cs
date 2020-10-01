using CascadeWorker.Config;
using Microsoft.Extensions.DependencyInjection;
using CascadeWorker.Database;
using CascadeWorker.Logging;
using CascadeWorker.Scraper.Queue;
using CascadeWorker.Scraper;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection;
using CascadeWorker.Scraper.Accounts;
using System.Collections.Generic;
using CascadeWorker.Scraper.Social;
using CascadeWorker.Scraper.Social.Providers.Instagram;
using CascadeWorker.Shared;
using CascadeWorker.Scraper.Settings;
using CascadeWorker.Scraper.Validation;
using CascadeWorker.Scraper.Validation.Validators;
using System.Net;
using System;
using Bugsnag;
using Bugsnag.Payload;
using CascadeWorker.Scraper.Notifications;

namespace CascadeWorker
{
    public class DependencyProvider : ServiceCollection
    {
        /// <summary>
        /// Class constructor
        /// </summary>
        public DependencyProvider()
        {
            Register();
        }

        /// <summary>
        /// Register all of the dependencies to the container.
        /// </summary>
        private void Register()
        {
            IConfigProvider configProvider = new JsonConfigProvider();
            
            configProvider.Load(StaticState.RemoteConfigUrl);

            this.AddSingleton(provider => configProvider);

            this.AddSingleton<LogFactory>();
            this.AddTransient(provider => provider.GetService<LogFactory>().GetLogger());

            this.AddSingleton<IDatabaseProvider, DatabaseProvider>(provider => new DatabaseProvider(
                configProvider.GetValueByKey("database.name"), 
                configProvider.GetValueByKey("database.password"), 
                uint.Parse(configProvider.GetValueByKey("database.port")), 
                configProvider.GetValueByKey("database.host"), 
                configProvider.GetValueByKey("database.username")));

            var chromeOptions = new ChromeOptions();

            chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.images", 2);
            chromeOptions.AddUserProfilePreference("profile.managed_default_content_settings.stylesheets", 2);
            
            chromeOptions.AddArgument("--disable-notifications");
            chromeOptions.AddArgument("--user-agent=Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                chromeOptions.AddArgument("headless");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                chromeOptions.AddArgument("--no-sandbox");
            }

            this.AddSingleton<IWebDriver>(provider => new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), chromeOptions, TimeSpan.FromSeconds(120)));

            this.AddSingleton<ScraperQueueDao>();
            this.AddSingleton<ScraperAccountDao>();

            this.AddSingleton<InstagramProfileDao>();

            var maleNames = Utilities.GetListFromLineOfStrings("https://cdn.cascade.io/lists/blacklisted-male-first-name-phrases.txt?v=10");
            var maleUsernames = Utilities.GetListFromLineOfStrings("https://cdn.cascade.io/lists/blacklisted-male-username-phrases.txt?v=8");
            var femaleNames = Utilities.GetListFromLineOfStrings("https://cdn.cascade.io/lists/female-first-name-phrases.txt?v=7");

            this.AddSingleton<IScraperValidator>(provider => new InstagramValidator(
                maleNames,
                maleUsernames,
                femaleNames
            ));

            this.AddSingleton<ScraperSettingsDao>();
            this.AddSingleton<ScraperSettings>();

            this.AddSingleton(provider => new NotificationSender(
                "*********************", 
                "***********************************************"));

            this.AddSingleton<InstagramEventHandler>();
            
            this.AddSingleton(provider => new Dictionary<string, ISocialEventHandler>
            {
                { "instagram.com", provider.GetService<InstagramEventHandler>() },
            });

            this.AddSingleton(provider =>
            {
                var bugSnagClient = new Client(new Configuration(StaticState.BugSnagApiKey));

                bugSnagClient.BeforeNotify(report =>
                {
                    report.Event.User = new User
                    {
                        Id = "Worker " + StaticState.WorkerId,
                    };
                });

                return bugSnagClient;
            });
            
            this.AddSingleton<ScraperWorker>();
            this.AddSingleton<ScraperWorkerDao>();
        }
    }
}
