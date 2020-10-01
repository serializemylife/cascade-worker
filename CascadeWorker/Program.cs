using System;
using Microsoft.Extensions.DependencyInjection;
using CascadeWorker.Scraper;
using CascadeWorker.Shared;
using Bugsnag;
using System.Threading;
using CascadeWorker.Logging;
using CascadeWorker.Scraper.Notifications;

namespace CascadeWorker
{
    internal static class Program
    {
        private static readonly DependencyProvider DependencyProvider = new DependencyProvider();

        private static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var logger = DependencyProvider.BuildServiceProvider().GetService<LogFactory>().GetLogger();
            
            var provider = DependencyProvider.BuildServiceProvider();
            var ipAddress = Utilities.GetIpAddress();

            var workerDao = provider.GetService<ScraperWorkerDao>();
            
            StaticState.WorkerId = workerDao.GetWorkerIdFromIp(ipAddress);
    
            if (StaticState.WorkerId == 0)
            {
                logger.Error($"Your IP address ({ipAddress}) is not assigned to any worker.");
                return;
            }
            
            provider.GetService<ScraperWorker>().Start();

            while (true)
            {
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        public static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            var provider = DependencyProvider.BuildServiceProvider();
            
            provider.GetService<Client>().Notify((Exception)e.ExceptionObject);
            provider.GetService<NotificationSender>().SendNotification($"Worker {StaticState.WorkerId} has crashed.", "829YaL");
            
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            Environment.Exit(1);
        }
    }
}