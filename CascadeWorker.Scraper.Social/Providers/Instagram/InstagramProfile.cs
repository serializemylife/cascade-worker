using CascadeWorker.Scraper.Social.Providers.Instagram.Posts;
using CascadeWorker.Scraper.Validation;
using System.Collections.Generic;
using CascadeWorker.Scraper.Settings;
using CascadeWorker.Shared.Extentions;
using ValidationResult = CascadeWorker.Scraper.Validation.ValidationResult;

namespace CascadeWorker.Scraper.Social.Providers.Instagram
{
    public class InstagramProfile : ISocialProfile
    {
        private readonly InstagramProfileDao _profileDao;
        private readonly ScraperSettings _scraperSettings;
        private readonly IScraperValidator _scraperValidator;

        public InstagramProfile(InstagramProfileDao profileDao, ScraperSettings scraperSettings, IScraperValidator scraperValidator)
        {
            _profileDao = profileDao;
            _scraperSettings = scraperSettings;
            _scraperValidator = scraperValidator;
        }

        public int Id { get; set; }
        public string Url { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Bio { get; set; }
        public int AgeGuessed { get; set; }
        public char GenderGuessed { get; set; }
        public bool IsPrivate { get; set; }
        public int FollowerCount { get; set; } // ignored for fb provider
        public int FollowingCount { get; set; }
        public List<InstagramConnection> Connections { get; set; } = new List<InstagramConnection>();
        public List<InstagramPost> Posts { get; set; } = new List<InstagramPost>();
        public string SnapchatUsername { get; set; }
        public bool SnapchatFound { get; set; }

        public void Save()
        {
            _profileDao.SaveProfile(this);
        }

        public bool ShouldScrape(out ValidationResult validationResult)
        {
            if (_scraperSettings["ignore_profiles_with_male_username"] == "1" && _scraperValidator.StringContainsMaleUsername(Username.CleanQueueItem()))
            {
                validationResult = ValidationResult.UsernameContainsMaleName;
                return false;
            }

            if (_scraperSettings["ignore_profiles_with_male_name"] == "1" && _scraperValidator.StringContainsMaleFirstName(Name.CleanQueueItem()))
            {
                validationResult = ValidationResult.NameContainsMaleName;
                return false;
            }

            if (_scraperSettings["ignore_profiles_with_foreign_bio"] == "1" && _scraperValidator.IsStringForeign(Bio))
            {
                validationResult = ValidationResult.StringIsForeign;
                return false;
            }

            if (_scraperSettings["ignore_profiles_with_foreign_name"] == "1" && _scraperValidator.IsStringForeign(Name))
            {
                validationResult = ValidationResult.StringIsForeign;
                return false;
            }

            if (_scraperSettings["ignore_profiles_with_male_snap_username"] == "1" && SnapchatFound && !string.IsNullOrEmpty(SnapchatUsername) && _scraperValidator.StringContainsMaleUsername(SnapchatUsername))
            {
                validationResult = ValidationResult.SnapchatUsernameContainsMaleName;
                return false;
            }

            if (AgeGuessed >= 12 && AgeGuessed < int.Parse(_scraperSettings["minimum_age_to_scrape_profile"]))
            {
                validationResult = ValidationResult.MinimumAgeNotMet;
                return false;
            }

            if (_scraperSettings["ignore_profiles_with_promotion_bio"] == "1" && _scraperValidator.StringContainsPromotion(Bio))
            {
                validationResult = ValidationResult.BioContainsPromotion;
                return false;
            }

            if (_scraperValidator.StringContainsDeletedProfilePhrase(Username)) // TODO: Only check depending on a config value?
            {
                validationResult = ValidationResult.UsernameContainsDeletedProfilePhrase;
                return false;
            }

            if (_scraperValidator.StringContainsMakeUpPhrase(Username)) // TODO: Only check depending on a config value?
            {
                validationResult = ValidationResult.UsernameContainsMakeUpPhrase;
                return false;
            }

            if (Username.Length < 7)
            {
                validationResult = ValidationResult.UsernameTooShort;
                return false;
            }

            if (Username.Length > 25)
            {
                validationResult = ValidationResult.UsernameTooLong;
                return false;
            }

            if (Name.Length < 4)
            {
                validationResult = ValidationResult.NameTooShort;
                return false;
            }

            if (Name.Length > 30)
            {
                validationResult = ValidationResult.NameTooLong;
                return false;
            }

            if (_scraperValidator.StringContainsGenericBadPhrase(Bio))
            {
                validationResult = ValidationResult.BioContainsBadPhrase;
                return false;
            }

            validationResult = ValidationResult.Success;
            return true;
        }

        public bool ShouldSave(out ValidationResult validationResult)
        {
            if (FollowerCount < int.Parse(_scraperSettings["minimum_connections_to_save_profile"]))
            {
                validationResult = ValidationResult.TooFewConnections;
                return false;
            }

            if (FollowerCount > int.Parse(_scraperSettings["maximum_connections_to_save_profile"]))
            {
                validationResult = ValidationResult.TooManyConnections;
                return false;
            }

            if (!SnapchatFound)
            {
                validationResult = ValidationResult.SnapchatNotFound;
                return false;
            }

            validationResult = ValidationResult.Success;
            return true;
        }

        public bool ShouldCollectConnections()
        {
            return FollowerCount > int.Parse(_scraperSettings["minimum_connections_to_collect_connections"]) &&
                   FollowerCount < int.Parse(_scraperSettings["maximum_connections_to_collect_connections"]) &&
                   !IsPrivate &&
                   SnapchatFound;
        }
    }
}
