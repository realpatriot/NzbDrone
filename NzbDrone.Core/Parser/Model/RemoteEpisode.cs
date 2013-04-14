using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser.Model
{
    public class RemoteEpisode
    {
        public ReportInfo Report { get; set; }
        public ParsedEpisodeInfo ParsedInfo { get; set; }

        public Series Series { get; set; }
        public List<Episode> Episodes { get; set; }

        public string GetDownloadTitle()
        {
            var seriesTitle = FileNameBuilder.CleanFilename(Series.Title);

            //Handle Full Naming
            if (ParsedInfo.FullSeason)
            {
                var seasonResult = String.Format("{0} - Season {1} [{2}]", seriesTitle, ParsedInfo.SeasonNumber, ParsedInfo.Quality.Quality);

                if (ParsedInfo.Quality.Proper)
                    seasonResult += " [Proper]";

                return seasonResult;
            }

            if (Series.SeriesType == SeriesTypes.Daily)
            {
                var dailyResult = String.Format("{0} - {1:yyyy-MM-dd} - {2} [{3}]", seriesTitle,
                                     ParsedInfo.AirDate, Episodes.First().Title, ParsedInfo.Quality.Quality);

                if (ParsedInfo.Quality.Proper)
                    dailyResult += " [Proper]";

                return dailyResult;
            }

            //Show Name - 1x01-1x02 - Episode Name
            //Show Name - 1x01 - Episode Name
            var episodeString = new List<string>();
            var episodeNames = new List<string>();

            foreach (var episode in Episodes)
            {
                episodeString.Add(String.Format("{0}x{1:00}", episode.SeasonNumber, episode.EpisodeNumber));
                episodeNames.Add(Core.Parser.Parser.CleanupEpisodeTitle(episode.Title));
            }

            var epNumberString = String.Join("-", episodeString);
            string episodeName;


            if (episodeNames.Distinct().Count() == 1)
                episodeName = episodeNames.First();

            else
                episodeName = String.Join(" + ", episodeNames.Distinct());

            var result = String.Format("{0} - {1} - {2} [{3}]", seriesTitle, epNumberString, episodeName, ParsedInfo.Quality.Quality);

            if (ParsedInfo.Quality.Proper)
            {
                result += " [Proper]";
            }

            return result;
        }
    }
}