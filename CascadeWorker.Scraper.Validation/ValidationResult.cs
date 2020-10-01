namespace CascadeWorker.Scraper.Validation
{
    public enum ValidationResult
    {
        Success = 0,
        UsernameContainsMaleName = 1,
        BioContainsBadPhrase = 2,
        BioContainsPromotion = 3,
        StringIsForeign = 4,
        MinimumAgeNotMet = 5,
        NameContainsMaleName = 6,
        NameTooLong = 7,
        NameTooShort = 8,
        SnapchatNotFound = 9,
        SnapchatUsernameContainsMaleName = 10,
        TooFewConnections = 11,
        TooManyConnections = 12,
        UsernameContainsDeletedProfilePhrase = 13,
        UsernameContainsMakeUpPhrase = 14,
        UsernameTooLong = 15,
        UsernameTooShort = 16,
    }
}
