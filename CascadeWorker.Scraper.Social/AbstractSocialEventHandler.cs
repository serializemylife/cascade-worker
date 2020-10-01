using System.Collections.Generic;
using CascadeWorker.Scraper.Queue;
using CascadeWorker.Scraper.Social.Providers.Instagram;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts.Media;

namespace CascadeWorker.Scraper.Social
{
    public abstract class AbstractSocialEventHandler : ISocialEventHandler
    {
        public abstract string GetLoginPageUrl();
        public abstract string GetLogoutPageUrl();
        public abstract bool IsLoginNeeded();
        public abstract void Login();
        public abstract void Logout();
        public abstract ISocialProfile CreateProfile();
        public abstract List<InstagramConnection> GetConnections();
        public abstract void NavigateToProfile();
        public abstract bool TryWaitForProfileToLoad();
        public abstract string GetUrl();
        public abstract string GetUsername();
        public abstract string GetName();
        public abstract string GetProfilePicture();
        public abstract string GetBio();
        public abstract bool GetIsPrivate();
        public abstract int GetFollowerCount();
        public abstract int GetFollowingCount();
        public abstract List<InstagramPost> GetPosts(string owner);
        public abstract List<InstagramPostMedia> GetPostMedia(string owner);
        public abstract void SetCurrentItem(string currentItem);
        public abstract List<InstagramConnection> GetFilteredConnections(List<InstagramConnection> connections);
        public abstract string GetPageSource();
        public abstract List<ScraperQueueItem> ConvertConnectionsToQueueItems(List<InstagramConnection> connections);
        public abstract bool IsProfileVisitsThrottled();
        public abstract void SwitchAccount(bool markCurrentAsThrottled);
        public abstract bool IsProfileNotFound();
    }
}
