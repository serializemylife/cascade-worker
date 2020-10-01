using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace CascadeWorker.Scraper.Settings
{
    public class ScraperSettings
    {
        private readonly Dictionary<string, string> _scraperSettings;
        
        public ScraperSettings(ScraperSettingsDao scraperSettingsDao)
        {
            _scraperSettings = scraperSettingsDao.GetSettingsCollection();
        }

        public string this[string key] => _scraperSettings[key];
    }
}
