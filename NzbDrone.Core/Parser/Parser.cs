using System;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Parser
{


    public static class Parser
    {

        private static readonly Regex NormalizeRegex = new Regex(@"((^|\W)(a|an|the|and|or|of)($|\W|_))|\W|_|(?:(?<=[^0-9]+)|\b)(?!(?:19\d{2}|20\d{2}))\d+(?=[^0-9ip]+|\b)",
                                                                 RegexOptions.IgnoreCase | RegexOptions.Compiled);



        private static readonly Regex MultiPartCleanupRegex = new Regex(@"\(\d+\)$", RegexOptions.Compiled);




        public static string NormalizeTitle(string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (Int64.TryParse(title, out number))
                return title;

            return NormalizeRegex.Replace(title, String.Empty).ToLower();
        }

        public static string CleanupEpisodeTitle(string title)
        {
            //this will remove (1),(2) from the end of multi part episodes.
            return MultiPartCleanupRegex.Replace(title, string.Empty).Trim();
        }
    }
}