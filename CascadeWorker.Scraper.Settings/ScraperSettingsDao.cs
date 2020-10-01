using System.Collections.Generic;
using CascadeWorker.Database;
using System.Data;

namespace CascadeWorker.Scraper.Settings
{
    public class ScraperSettingsDao
    {
        private readonly IDatabaseProvider _databaseProvider;

        public ScraperSettingsDao(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public Dictionary<string, string> GetSettingsCollection()
        {
            var output = new Dictionary<string, string>();
            
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("SELECT * FROM `scraper_settings`");

            foreach (DataRow row in dbConnection.ExecuteTable().Rows)
            {
                output.Add((string) row["name"], (string) row["value"]);
            }

            return output;
        }
    }
}
