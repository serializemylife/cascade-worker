using CascadeWorker.Shared;
using CascadeWorker.Shared.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CascadeWorker.Scraper.Validation.Validators
{
    public class InstagramValidator : IScraperValidator
    {
        private readonly List<string> _maleFirstNamePhrases;
        private readonly List<string> _maleUsernamePhrases; 
        private readonly List<string> _femaleFirstNamePhrases; 

        public InstagramValidator(
            List<string> maleFirstNamePhrases, 
            List<string> maleUsernamePhrases, 
            List<string> femaleFirstNamePhrases)
        {
            _maleFirstNamePhrases = maleFirstNamePhrases;
            _maleUsernamePhrases = maleUsernamePhrases;
            _femaleFirstNamePhrases = femaleFirstNamePhrases;
        }

        public bool StringContainsMaleFirstName(string str)
        {
            return _maleFirstNamePhrases.Any(s => str.StartsWith(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool StringContainsFemaleName(string str)
        {
            return _femaleFirstNamePhrases.Any(s => str.Contains(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool StringContainsMaleUsername(string str)
        {
            return _maleUsernamePhrases.Any(s => str.Contains(s, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool IsStringForeign(string str)
        {
            var foreignTriggers = new List<string>
            {
                "african american", "romanian", "italian", "nigerian", "australian", "russian", "aussie", "australian", "australia", "collumbian",
                "french", "brazilian", "arabic", "polish", "swedish",

                "california", "washington", "michigan", "arizona", "massachusetts", "pennsylvania", "wisconsin",  "connecticut", "louisiana", "kentucky"
            };

            return foreignTriggers.Any(str.ContainsIgnoreCase) || HasArabicCharacters(str);
        }

        private bool HasArabicCharacters(string str)
        {
            return new Regex("[\u0600-\u06ff]|[\u0750-\u077f]|[\ufb50-\ufc3f]|[\ufe70-\ufefc]").IsMatch(str);
        }

        public bool StringContainsSnapchatUsername(string str)
        {
            var snapTriggers = new List<string>
            {
                "👻", "𝑺𝒄 ", "𝑺𝒄 :", "𝚜𝚗𝚊𝚙𝚌𝚑𝚊𝚝", "sc’", "sc - ", "sc ", "sc:", "scm ", "scm;", "sc- ", "sc.", "sc-", "sc ~", "sc;", "sc; ", "sc~", "sc/", "snapchat:",
                "snapchat - ", "snapchat-", "snap~", "snap =", "snap:", "add my sc", "add me on sc", "add me sc", "add my snap", "add my snapchat", "add me on snap",
                "scm~", "scm ~", "sc•", "sc •", "🆂🅲 "
            };

            return snapTriggers.Any(str.Contains);
        }

        public bool TryExtractSnapchatUsernameFromString(string str, out string snapchatUsername)
        {
            if (
                TryExtractSnapchatUsernameFromConvention("sc:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc-", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc -", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc ~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm ~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc;", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc👻:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc👻~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc •", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc•", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm-", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm👻:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm👻~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm •", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("scm•", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap-", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap;", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap👻:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap👻~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("👻snap", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap •", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snap•", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("sc/", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat-", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat -", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat;", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat👻:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat👻~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat •", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("snapchat•", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("👻:", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("👻-", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("👻~", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("👻;", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("👻•", str, out snapchatUsername) ||
                TryExtractSnapchatUsernameFromConvention("𝕊𝕟𝕒𝕡-", str, out snapchatUsername))
            {
                snapchatUsername = Regex.Replace(snapchatUsername, @"\p{Cs}", ""); // Lets remove any unicode such as emojis.
            }
            else if (str.Contains("sc "))
            {
                var sequences = str.Split(" ").SkipWhile(x => x != "sc");
                snapchatUsername = sequences.Count() > 0 ? sequences.Skip(1).First() : "";
            }
            else if (str.StartsWith("👻 "))
            {
                snapchatUsername = str.Split(" ").SkipWhile(x => x != "👻").Skip(1).First();
            }
            else if (str.StartsWith("👻"))
            {
                snapchatUsername = str.Split(" ").SkipWhile(x => !x.StartsWith("👻")).First().Substring("👻".Length);
            }
            else if (str.Split(" ").Any(x => x.EndsWith("👻")))
            {
                snapchatUsername = str.Split(" ").SkipWhile(x => !x.EndsWith("👻")).First();
            }

            snapchatUsername = Regex.Replace(snapchatUsername, @"\p{Cs}", ""); // Lets remove any unicode such as emojis.
            snapchatUsername = snapchatUsername.Replace("•", "");

            return !string.IsNullOrEmpty(snapchatUsername) && Utilities.IsValidSnapchatUsername(snapchatUsername);
        }

        private bool TryExtractSnapchatUsernameFromConvention(string convention, string str, out string snapchatUsername)
        {
            if (str.Contains(convention + " "))
            {
                var possibleConventions = str.Split(" ").SkipWhile(x => x != convention);
                
                if (!possibleConventions.Any())
                {
                    var everythingAfterConvention = str.Substring(str.IndexOf(convention) + convention.Length + 1);

                    if (!everythingAfterConvention.Contains(" "))
                    {
                        snapchatUsername = everythingAfterConvention;
                    }
                    else
                    {
                        snapchatUsername = everythingAfterConvention.Split(" ").First();
                    }
                }
                else
                {
                    snapchatUsername = str.Split(" ").SkipWhile(x => x != convention).Skip(1).First();
                }
            }
            else if (str.StartsWith(convention) && !str.Contains(" "))
            {
                snapchatUsername = str.Substring(convention.Length);
            }
            else if (str.Contains(convention))
            {
                if (str.Contains(" ") && str.Split(" ").Last().Contains(convention))
                {
                    snapchatUsername = str.Split(" ").Last().Substring(convention.Length);
                }
                else if (str.Contains(" ") && str.Split(" ").First().Contains(convention))
                {
                    snapchatUsername = str.Split(" ").First().Substring(convention.Length);
                }
                else
                {
                    var firstWordWithConvention = str.Split(" ").SkipWhile(x => x != convention).FirstOrDefault();

                    if (firstWordWithConvention == null)
                    {
                        snapchatUsername = "";
                    }
                    else
                    {
                        snapchatUsername = firstWordWithConvention.Substring(convention.Length);
                    }
                }
            }
            else
            {
                snapchatUsername = string.Empty;
            }

            return !string.IsNullOrEmpty(snapchatUsername);
        }

        public bool TryExtractAgeFromString(string str, out int age)
        {
            var allowedAgesForSuccess = new List<int> { 12, 13, 14, 15, 16, 17, 18, 19 };

            var twelveYearsOldFilters = new List<string>
                {"12•", "//12", "|12|", "12 x", "year 7", "12💞", "12❤️", "1️⃣2️⃣", "12yo "};
            
            var thirteenYearsOldFilters = new List<string>
                {"13•", "//13", "|13|", "13 x", "year 8", "13💞", "13❤️", "1️⃣3️⃣", "13yo "};
            
            var fourteenYearsOldFilters = new List<string>
                {"14•", "//14", "|14|", "14 x", "year 9", "14💞", "14❤️", "1️⃣4️⃣", "14yo "};
            
            var fifteenYearsOldFilters = new List<string>
                {"15•", "//15", "|15|", "15 x", "year 10", "15💞", "15❤️", "1️⃣5️⃣", "15yo"};
            
            var sixteenYearsOldFilters = new List<string>
                {"16•", "//16", "|16|", "16 x", "year 11", "6teen", "16💞", "16❤️", "16~", "16yo "};
            
            var seventeenYearsOldFilters = new List<string>
                {"17•", "//17", "|17|", "17 x", "s•e•v•e•n•t•e•e•n", "seventeen xo", "year 12", "7teen", "17💞", "17❤️", "17~", "17yo "};
            
            var eighteenYearsOldFilters = new List<string>
                {"18•", "//18", "|18|", "18 x", "year 13", "8teen", "18💞", "18❤️", "18~", "18yo "};
            
            var nineteenYearsOldFilters = new List<string> {"19•", "//19", "|19|", "19 x", "9teen", "19💞", "19❤️", "19~"};
            
            var twentyYearsOldFilters = new List<string> {"20•", "//20", "|20|", "20 x", "20💞", "20❤️", "20~"};
            
            var twentyOneYearsOldFilters = new List<string> {"21•", "//21", "|21|", "21 x", "21💞", "21❤️", "21~"};
            
            var twentyTwoYearsOldFilters = new List<string> {"22•", "//22", "|22|", "22 x", "22💞", "22❤️", "22~"};
            
            var twentyThreeYearsOldFilters = new List<string> {"23•", "//23", "|23|", "23 x", "23💞", "23❤️", "23~"};
            
            var twentyFourYearsOldFilters = new List<string> {"24•", "//24", "|24|", "24 x", "24💞", "24❤️", "24~"};

            if (twelveYearsOldFilters.Where(str.Contains).Any())
            {
                age = 12;
            }
            else if (thirteenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 13;
            }
            else if (fourteenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 14;
            }
            else if (fifteenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 15;
            }
            else if (sixteenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 16;
            }
            else if (seventeenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 17;
            }
            else if (eighteenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 18;
            }
            else if (nineteenYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 19;
            }
            else if (twentyYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 20;
            }
            else if (twentyOneYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 21;
            }
            else if (twentyTwoYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 22;
            }
            else if (twentyThreeYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 23;
            }
            else if (twentyFourYearsOldFilters.Where(str.ContainsIgnoreCase).Any())
            {
                age = 24;
            }
            else
            {
                age = 0;
            }

            return allowedAgesForSuccess.Contains(age);
        }

        public bool StringContainsPromotion(string str)
        {
            var promotions = new List<string>()
            {
                "for booking", "for bookings", "dm for collab", "for collab", "dm for enquires", "entrepreneur", "forex trader", "save up to", "drinks from ", "10% off", "for sale"
            };

            return promotions.Any(x => str.ContainsIgnoreCase(x));
        }

        public bool StringContainsMakeUpPhrase(string str)
        {
            var makeupPhrases = new List<string>()
            {
                "makeupby", "hairby", "nailsby", ".nails", "nails.", "aesthetics", "lashesby", "lashes_by"
            };

            return makeupPhrases.Any(x => str.ContainsIgnoreCase(x));
        }

        public bool StringContainsDeletedProfilePhrase(string str)
        {
            var deletedPhrases = new List<string>()
            {
                "notactive", "notusing", "notbeingused", "deactivated", "deleted", "delete", "notused", "oldaccount"
            };

            return deletedPhrases.Any(x => str.ContainsIgnoreCase(x));
        }

        public bool StringContainsGenericBadPhrase(string str)
        {
            var genericBadPhrases = new List<string>()
            {
                "kik:", "whatsapp:", "bbm:"
            };

            return genericBadPhrases.Any(x => str.ContainsIgnoreCase(x));
        }

        /// <summary>
        /// Primary method to check all sub-methods
        /// Checks GenericBadPhrase, DeletedProfilePhrase and MakeUpPhrase
        /// </summary>
        /// <returns></returns>
        public bool StringContainsAnythingBad(string str)
        {
            return StringContainsGenericBadPhrase(str) ||
                   StringContainsDeletedProfilePhrase(str) ||
                   StringContainsMakeUpPhrase(str);
        }
    }
}
