using CascadeWorker.Scraper.Queue;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts.Media;
using System.Collections.Generic;
using CascadeWorker.Scraper.Social.Providers.Instagram;

namespace CascadeWorker.Scraper.Social
{
    public interface ISocialEventHandler
    {
        string GetLoginPageUrl();
        string GetLogoutPageUrl();
        bool IsLoginNeeded();
        void Login();
        void Logout();
        ISocialProfile CreateProfile();
        List<InstagramConnection> GetConnections();
        void NavigateToProfile();
        bool TryWaitForProfileToLoad();
        string GetUrl();
        string GetUsername();
        string GetName();
        string GetProfilePicture();
        string GetBio();
        bool GetIsPrivate();
        int GetFollowerCount();
        int GetFollowingCount();
        List<InstagramPost> GetPosts(string owner);
        List<InstagramPostMedia> GetPostMedia(string owner);
        void SetCurrentItem(string currentItem);
        List<InstagramConnection> GetFilteredConnections(List<InstagramConnection> connections);
        string GetPageSource();
        List<ScraperQueueItem> ConvertConnectionsToQueueItems(List<InstagramConnection> connections);
        bool IsProfileVisitsThrottled();
        void SwitchAccount(bool markCurrentAsThrottled);
        bool IsProfileNotFound();
    }
}
