using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace CascadeWorker.Shared
{
    public static class Utilities
    {
        public static List<string> GetListFromLineOfStrings(string gistUrl)
        {
            using var webClient = new WebClient();
            return webClient.DownloadString(gistUrl).Split("\r\n").Where(x => !string.IsNullOrEmpty(x) && x.Length >= 4).ToList();
        }

        public static string GetIpAddress()
        {
            using var webClient = new WebClient();
            return webClient.DownloadString("http://ip-api.com/line").Split("\n").Last(x => !string.IsNullOrEmpty(x));
        }
        
        public static bool IsValidSnapchatUsername(string snapchatUsername)
        {
            if (snapchatUsername.Length < 3 || snapchatUsername.Length > 15)
            {
                return false;
            }

            var allowedCharacters = "abcdefghijklmnopqrstuvwxyz-_.0123456789".ToCharArray();

            if (snapchatUsername.ToCharArray().Any(x => !allowedCharacters.Contains(x)))
            {
                return false;
            }
	
            if ("0123456789-_.".ToCharArray().Contains(snapchatUsername[0]))
            {
                return false;
            }
	
            if ("-_.".ToCharArray().Contains(snapchatUsername[snapchatUsername.Length - 1]))
            {
                return false;
            }
	
            return true;
        }
        
        public static string GetConsistentSocialUrl(string url)
        {
            var uri = new Uri(url);
            return $"https://{uri.Host.Replace("www.", "")}/{uri.AbsolutePath.Replace("/", "")}";
        }
    }
}
