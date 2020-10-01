namespace CascadeWorker.Scraper.Social.Providers.Instagram
{
    public static class InstagramDomSelectors
    {
        
        public const string ProfileUsername = "//*[@class='_7UhW9       fKFbl yUEEX   KV-D4              fDxYl     ']";
        public const string ProfileName = "//h1[@class='rhpdm']";
        public const string ProfilePicture = "//img[1]";
        public const string ProfileBio = "//div[@class='-vDIg']//span";
        public const string ProfileFollowers = "//li[@class='Y8-fY '][2]/*/span";
        public const string ProfileFollowing = "//li[@class='Y8-fY '][3]/*/span";
        public const string ProfilePostPicture = "//img[@class='FFVAD']";
    }
}