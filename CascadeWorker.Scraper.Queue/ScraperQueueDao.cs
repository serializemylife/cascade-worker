using CascadeWorker.Database;
using CascadeWorker.Shared;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CascadeWorker.Scraper.Queue
{
    public class ScraperQueueDao
    {
        private readonly IDatabaseProvider _databaseProvider;

        public ScraperQueueDao(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public bool TryGetItemSafe(out ScraperQueueItem scraperQueueItem)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            
            dbConnection.SetQuery(@"
                    START TRANSACTION;

                    SELECT @id := `id`,`item` 
                    FROM `queue_items` 
                    WHERE `processed_at` IS NULL AND `completed_at` IS NULL ORDER BY `id` ASC 
                    LIMIT 1
                    FOR UPDATE;

                    UPDATE `queue_items` SET `processed_at` = @processedAt, `worker_id` = @workerId WHERE `id` = @id;

                    COMMIT;");
                
            dbConnection.AddParameter("processedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.AddParameter("workerId", StaticState.WorkerId);

            if (!int.TryParse(dbConnection.ExecuteScalar().ToString(), out var queueItemId) || queueItemId == 0)
            {
                scraperQueueItem = null;
                return false;
            }

            dbConnection.SetQuery("SELECT `id`,`item`,`type_id`,`is_private`,`confirmed` FROM `queue_items` WHERE `id` = @itemId LIMIT 1");
            dbConnection.AddParameter("itemId", queueItemId);
                
            var itemRow = dbConnection.ExecuteRow();
            
            scraperQueueItem = new ScraperQueueItem
            {
                Id = Convert.ToInt64(itemRow["id"]),
                Item = (string) itemRow["item"],
                TypeId = Convert.ToInt32(itemRow["type_id"]),
                IsPrivate = Convert.ToInt32(itemRow["is_private"]) == 1,
                Confirmed = Convert.ToInt32(itemRow["confirmed"]) == 1
            };
            
            return true;
        }

        public async Task MarkItemAsComplete(long id)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("UPDATE `queue_items` SET `completed_at` = @completedAt, `updated_at` = @completedAt WHERE `id` = @id");
            dbConnection.AddParameter("completedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.AddParameter("id", id);
            await dbConnection.ExecuteQueryAsync();
        }

        public async Task StoreItems(List<ScraperQueueItem> items, int ownerId)
        {
            var queries = new List<string>();
            var createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var lastItem = items.Last();

            foreach (var item in items)
            {
                queries.Add($" ('{MySqlHelper.EscapeString(item.Item)}', {item.TypeId}, '{(item.IsPrivate ? "1" : "0")}', '{(item.Confirmed ? 1 : 0)}', '{ownerId}', '{MySqlHelper.EscapeString(createdAt)}', '{MySqlHelper.EscapeString(createdAt)}')" + (item.Equals(lastItem) ? ";" : ",")); // , or ;
            }

            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT IGNORE INTO `queue_items` (item, type_id, is_private, confirmed, owner_id, created_at, updated_at) VALUES " + string.Join(Environment.NewLine, queries));
            await dbConnection.ExecuteQueryAsync();
        }

        public async Task StoreItemResult(ScraperQueueItemResult result)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT INTO `queue_item_results` (item_id, content, page_source, success, created_at, updated_at) VALUES (@itemId, @content, @pageSource, @success, @createdAt, @createdAt)");
            dbConnection.AddParameter("itemId", result.ItemId);
            dbConnection.AddParameter("content", result.Content);
            dbConnection.AddParameter("pageSource", result.PageSource);
            dbConnection.AddParameter("success", result.Success ? "1" : "0");
            dbConnection.AddParameter("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            await dbConnection.ExecuteQueryAsync();
        }
    }
}
