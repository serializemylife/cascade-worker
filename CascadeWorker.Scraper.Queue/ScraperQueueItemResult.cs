namespace CascadeWorker.Scraper.Queue
{
    public class ScraperQueueItemResult
    {
        public long ItemId { get; set; }
        public string Content { get; set; }
        public string PageSource { get; set; }
        public bool Success { get; set; }
    }
}
