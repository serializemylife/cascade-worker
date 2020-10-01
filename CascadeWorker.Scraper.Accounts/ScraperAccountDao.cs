using CascadeWorker.Database;
using System;
using CascadeWorker.Logging;
using CascadeWorker.Shared;

namespace CascadeWorker.Scraper.Accounts
{
    public class ScraperAccountDao
    {
        private readonly IDatabaseProvider _databaseProvider;
        private readonly ILogger _logger;

        public ScraperAccountDao(IDatabaseProvider databaseProvider, ILogger logger)
        {
            _databaseProvider = databaseProvider;
            _logger = logger;
        }

        public bool TryFindAccount(out ScraperAccount account, int typeId, int minutesToWaitSinceThrottle, int minutesToWaitSinceFetched, bool markAsFetched = true)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery($"SELECT `id`,`username`,`password`,`type_id` FROM `scraper_accounts` WHERE (`throttled_at` IS NULL OR `throttled_at` < (NOW() - INTERVAL {minutesToWaitSinceThrottle} MINUTE)) AND `fetched_at` < (NOW() - INTERVAL {minutesToWaitSinceFetched} MINUTE) AND `enabled` = '1' AND `type_id` = '" + typeId + "' AND `worker_id` = @workerId ORDER BY `fetched_at` ASC LIMIT 1");
            dbConnection.AddParameter("workerId", StaticState.WorkerId);
            
            var accountRow = dbConnection.ExecuteRow();

            if (accountRow == null)
            {
                account = null;
                return false;
            }

            if (markAsFetched)
            {
                MarkAccountAsFetched(Convert.ToInt32(accountRow["id"]));
            }

            account = new ScraperAccount(
                Convert.ToInt32(accountRow["id"]),
                (string)accountRow["username"],
                (string)accountRow["password"],
                Convert.ToInt32(accountRow["type_id"]));

            return true;
        }

        private void MarkAccountAsFetched(int accountId)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("UPDATE `scraper_accounts` SET `fetched_at` = @fetchedAt, `updated_at` = @fetchedAt WHERE `id` = @accountId");
            dbConnection.AddParameter("fetchedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.AddParameter("accountId", accountId);
            dbConnection.ExecuteQuery();
        }

        public void MarkAccountAsThrottled(int accountId)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("UPDATE `scraper_accounts` SET `throttled_at` = @throttledAt, `updated_at` = @throttledAt WHERE `id` = @accountId");
            dbConnection.AddParameter("throttledAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.AddParameter("accountId", accountId);
            dbConnection.ExecuteQuery();
        }

        public void MarkAccountAsDisabled(int accountId)
        {
            using (var dbConnection = _databaseProvider.GetConnection())
            {
                dbConnection.SetQuery("UPDATE `scraper_accounts` SET `enabled` = '0', `updated_at` = @updatedAt WHERE `id` = @accountId");
                dbConnection.AddParameter("updatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                dbConnection.AddParameter("accountId", accountId);
                dbConnection.ExecuteQuery();
            }
            
            _logger.Error($"Marking account {accountId} as disabled.");
        }
    }
}
