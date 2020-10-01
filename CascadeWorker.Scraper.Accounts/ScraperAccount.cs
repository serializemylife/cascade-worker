namespace CascadeWorker.Scraper.Accounts
{
    public class ScraperAccount
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public int TypeId { get; set; }

        public ScraperAccount(int id, string username, string password, int typeId)
        {
            Id = id;
            Username = username;
            Password = password;
            TypeId = typeId;
        }
    }
}
