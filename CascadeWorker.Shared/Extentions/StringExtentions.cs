using System;

namespace CascadeWorker.Shared.Extentions
{
    public static class StringExtentions
    {
        public static string GetInbetween(this string s, string start, string end)
        {
            return s[(s.IndexOf(start) + start.Length)..s.IndexOf(end, s.IndexOf(start))];
        }

        public static string CleanQueueItem(this string s)
        {
            return s
                .Replace("https://instagram.com/", "")
                .Replace("https://facebook.com/", "")
                .Replace(".", "")
                .Replace("_", "")
                .Replace("/", "")
                .Replace("0", "o")
                .Replace("3", "e")
                .Replace("4", "a")
                .Replace(" ", "");
        }

        public static bool ContainsIgnoreCase(this string source, string substring)
        {
            return source?.IndexOf(substring ?? "", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string Truncate(this string s, int maxLength)
        {
            return s.Substring(0, maxLength);
        }
    }
}
