using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.NzbClub
{
    public class NzbClub : Indexer
    {
        public override IEnumerable<string> RecentFeed
        {
            get
            {
                return new[]
                           {
                               String.Format("http://www.nzbclub.com/nzbfeed.aspx?ig=2&gid=102952&st=1&ns=1&q=%23a.b.teevee"),
                               String.Format("http://www.nzbclub.com/nzbfeed.aspx?ig=2&gid=5542&st=1&ns=1&q=")
                           };
            }
        }

        public override string Name
        {
            get { return "NzbClub"; }
        }



        public override IEnumerable<string> GetEpisodeSearchUrls(string seriesTitle, int seasonNumber, int episodeNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}+{1}+s{2:00}e{3:00}", url, seriesTitle, seasonNumber, episodeNumber));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetSeasonSearchUrls(string seriesTitle, int seasonNumber)
        {
            var searchUrls = new List<string>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}+{1}+s{2:00}", url, seriesTitle, seasonNumber));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetDailyEpisodeSearchUrls(string seriesTitle, DateTime date)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}+{1}+{2:yyyy MM dd}", url, seriesTitle, date));
            }

            return searchUrls;
        }

        public override IEnumerable<string> GetPartialSeasonSearchUrls(string seriesTitle, int seasonNumber, int episodeWildcard)
        {
            var searchUrls = new List<String>();

            foreach (var url in RecentFeed)
            {
                searchUrls.Add(String.Format("{0}+{1}+S{2:00}E{3}", url, seriesTitle, seasonNumber, episodeWildcard));
            }

            return searchUrls;
        }
    }
}