namespace CascadeWorker.Scraper.Queue
{
    public class ScraperQueueItem
    {
        public long Id { get; set; }
        public string Item { get; set; }
        public int TypeId { get; set; }
        public bool IsPrivate { get; set; }
        public bool Confirmed { get; set; }
    }
}
