using System;
using System.Text.RegularExpressions;

namespace LowDig
{
    static class Functions
    {
        /// <summary>
        /// Converts from SteamId to SteamId64
        /// </summary>
        /// <param name="steamId">SteamId to convert</param>
        /// <returns>Returns a converted SteamId64</returns>
        public static string ConvertToSteam64(string steamId)
        {
            steamId.Replace("STEAM_", "");
            string[] split = steamId.Split(':');
            return "765611979" + ((Convert.ToInt64(split[2]) * 2) + 60265728 + Convert.ToInt64(split[1])).ToString();
        }


        /// <summary>
        /// Gets a string inbetween two strings
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="start">Start string</param>
        /// <param name="end">End string</param>
        /// <returns>Returns string if found</returns>
        public static string GetStringBetween(string source, string start, string end)
        {
            int startIndex = source.IndexOf(start);
            if (startIndex != -1)
            {
                int endIndex = source.IndexOf(end, startIndex + 1);
                if (endIndex != -1)
                {
                    return source.Substring(startIndex + start.Length, endIndex - startIndex - start.Length);
                }
            }
            return string.Empty;
        }


        /// <summary>
        /// Checks if an email adress looks okay
        /// </summary>
        /// <param name="str">String to check</param>
        /// <returns>Returns true if email looks ok</returns>
        public static bool IsProperEmail(string str)
        {
            return Regex.IsMatch(str, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
                RegexOptions.IgnoreCase);
        }
    }
}
