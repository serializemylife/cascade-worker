namespace CascadeWorker.Scraper.Validation
{
    public interface IScraperValidator
    {
        bool StringContainsMaleFirstName(string str);
        bool StringContainsFemaleName(string str);
        bool StringContainsMaleUsername(string str);
        bool IsStringForeign(string str);
        bool StringContainsSnapchatUsername(string str);
        bool TryExtractSnapchatUsernameFromString(string str, out string snapchatUsername);
        bool TryExtractAgeFromString(string str, out int age);
        bool StringContainsGenericBadPhrase(string str);
        bool StringContainsPromotion(string str);
        bool StringContainsMakeUpPhrase(string str);
        bool StringContainsDeletedProfilePhrase(string str);
        bool StringContainsAnythingBad(string str);
    }
}
