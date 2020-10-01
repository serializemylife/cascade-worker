using System;
using System.Threading.Tasks;
using CascadeWorker.Database;

namespace CascadeWorker.Scraper
{
    public class ScraperWorkerDao
    {
        private readonly IDatabaseProvider _databaseProvider;

        public ScraperWorkerDao(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public int GetWorkerIdFromIp(string ipAddress)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("SELECT `id` FROM `scraper_workers` WHERE `ip_address` = @ipAddress");
            dbConnection.AddParameter("ipAddress", ipAddress);
                
            var workerRow = dbConnection.ExecuteRow();

            return workerRow == null ? 0 : Convert.ToInt32(workerRow["id"]);
        }

        public bool IsWorkerRunning(int workerId)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("SELECT `is_running` FROM `scraper_workers` WHERE `id` = @workerId LIMIT 1;");
            dbConnection.AddParameter("workerId", workerId);

            return dbConnection.ExecuteScalar().ToString() == "1";
        }

        public async Task UpdateWorkerLastSeen(int workerId)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("UPDATE `scraper_workers` SET `last_seen` = @lastSeen WHERE `id` = @workerId");
            dbConnection.AddParameter("lastSeen", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.AddParameter("workerId", workerId);
            await dbConnection.ExecuteQueryAsync();
        }
    }
}
