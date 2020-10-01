using CascadeWorker.Database;
using CascadeWorker.Scraper.Social.Providers.Instagram.Posts.Media;
using CascadeWorker.Shared;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CascadeWorker.Scraper.Social.Providers.Instagram
{
    public class InstagramProfileDao
    {
        private readonly IDatabaseProvider _databaseProvider;

        public InstagramProfileDao(IDatabaseProvider databaseProvider)
        {
            _databaseProvider = databaseProvider;
        }

        public void SaveProfile(InstagramProfile profile)
        {
            profile.Id = CreateProfile(profile.Url, 1, StaticState.WorkerId);

            if (profile.Id == 0)
            {
                return;
            }
            
            CreateProfileData(
                    profile.Id, 
                    profile.Name, 
                    profile.Username,
                    profile.Picture, 
                    profile.Bio, 
                    profile.AgeGuessed, 
                    profile.IsPrivate, 
                    profile.FollowerCount, 
                    profile.FollowingCount, 
                    profile.SnapchatUsername,
                    profile.SnapchatFound);

            var postId = CreateProfilePost(profile.Id, "");

            if (profile.Posts.Any(x => x.Media.Count > 0))
            {
                CreateProfilePostMediaBulk(postId, profile.Posts.SelectMany(c => c.Media).ToList());
            }

            if (profile.Connections.Count > 0)
            {
                CreateProfileConnectionEntries(profile.Id, profile.Connections);
            }
        }

        private int CreateProfile(string url, int typeId, int workerId)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT INTO scraper_profiles (url, type_id, worker_id, created_at, updated_at) VALUES (@url, @typeId, @workerId, @createdAt, @createdAt)");
            dbConnection.AddParameter("url", url);
            dbConnection.AddParameter("typeId", typeId);
            dbConnection.AddParameter("workerId", workerId);
            dbConnection.AddParameter("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.ExecuteQuery();

            return dbConnection.GetLastId();
        }

        private void CreateProfileData(int profileId, string name, string username, string picture, string bio, int ageGuessed, bool isPrivate, int followerCount, int followingCount, string snapchatUsername, bool snapchatFound)
        {
            var hasSetSnapUsername = snapchatFound && !string.IsNullOrEmpty(snapchatUsername);

            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT INTO scraper_profile_data (profile_id, name, username, picture, bio, age_guessed, is_private, follower_count, following_count, snapchat_username, snapchat_updated_at, snapchat_status, created_at, updated_at) VALUES (@profileId, @name, @username, @picture, @bio, @ageGuessed, @isPrivate, @followerCount, @followingCount, @snapchatUsername, @snapchatUpdatedAt, @snapchatStatus, @createdAt, @createdAt)");
            dbConnection.AddParameter("profileId", profileId);
            dbConnection.AddParameter("name", name);
            dbConnection.AddParameter("username", username);
            dbConnection.AddParameter("picture", picture);
            dbConnection.AddParameter("bio", bio);
            dbConnection.AddParameter("ageGuessed", ageGuessed);
            dbConnection.AddParameter("isPrivate", isPrivate ? "1" : "0");
            dbConnection.AddParameter("followerCount", followerCount);
            dbConnection.AddParameter("followingCount", followingCount);
            dbConnection.AddParameter("snapchatUsername", snapchatUsername);
            dbConnection.AddParameter("snapchatUpdatedAt", hasSetSnapUsername ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") : null);
            dbConnection.AddParameter("snapchatStatus", hasSetSnapUsername ? "pending" : "waiting");
            dbConnection.AddParameter("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            dbConnection.ExecuteQuery();
        }

        private int CreateProfilePost(int profileId, string caption)
        {
            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT INTO `scraper_profile_posts` (profile_id, caption, created_at, updated_at) VALUES (@profileId, @caption, @createdAt, @createdAt)");
            dbConnection.AddParameter("profileId", profileId);
            dbConnection.AddParameter("caption", caption);
            dbConnection.AddParameter("createdAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            dbConnection.ExecuteQuery();

            return dbConnection.GetLastId();
        }

        private void CreateProfilePostMediaBulk(int postId, List<InstagramPostMedia> mediaItems) // TODO: its trying to use parameters 
        {
            var queries = new List<string>();
            var createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var lastMediaItem = mediaItems.Last();

            foreach (var mediaItem in mediaItems)
            {
                queries.Add($" ({postId}, '{MySqlHelper.EscapeString(mediaItem.CdnUrl)}', '{MySqlHelper.EscapeString(mediaItem.MetaData)}', '{MySqlHelper.EscapeString(createdAt)}', '{MySqlHelper.EscapeString(createdAt)}')" + (mediaItem.Equals(lastMediaItem) ? ";" : ","));
            }

            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT INTO `scraper_profile_post_media` (post_id, cdn_url, meta_data, created_at, updated_at) VALUES " + string.Join("", queries));
            dbConnection.ExecuteQuery();
        }

        private void CreateProfileConnectionEntries(int profileId, List<InstagramConnection> connections)
        {
            var queries = new List<string>();
            var createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var lastConnection = connections.Last();

            foreach (var connection in connections)
            {
                queries.Add($" ({profileId}, '{connection.Item}', '{MySqlHelper.EscapeString(createdAt)}', '{MySqlHelper.EscapeString(createdAt)}')" + (connection.Equals(lastConnection) ? ";" : ",")); // , or ;
            }

            using var dbConnection = _databaseProvider.GetConnection();
            dbConnection.SetQuery("INSERT INTO `scraper_profile_followers` (profile_id, target_url, created_at, updated_at) VALUES " + string.Join(Environment.NewLine, queries));
            dbConnection.ExecuteQuery();
        }
    }
}
