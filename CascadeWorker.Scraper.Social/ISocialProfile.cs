using CascadeWorker.Scraper.Social.Providers.Instagram.Posts;
using CascadeWorker.Scraper.Validation;
using System.Collections.Generic;
using CascadeWorker.Scraper.Social.Providers.Instagram;

namespace CascadeWorker.Scraper.Social
{
    public interface ISocialProfile
    {
        int Id { get; set; }
        string Url { get; set; }
        string Username { get; set; }
        string Name { get; set; }
        string Picture { get; set; }
        string Bio { get; set; }
        int AgeGuessed { get; set; }
        char GenderGuessed { get; set; }
        bool IsPrivate { get; set; }
        int FollowerCount { get; set; }
        int FollowingCount { get; set; }
        List<InstagramConnection> Connections { get; set; }
        List<InstagramPost> Posts { get; set; }
        void Save();
        bool ShouldScrape(out ValidationResult validationResult);
        bool ShouldSave(out ValidationResult validationResult);
        bool ShouldCollectConnections();
    }
}
