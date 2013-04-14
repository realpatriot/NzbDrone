﻿using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Model
{

    public class ReportInfo
    {
        public string Title { get; set; }
        public long Size { get; set; }
        public string NzbUrl { get; set; }
        public string NzbInfoUrl { get; set; }
        public String Indexer { get; set; }
        public int Age { get; set; }
        public string ReleaseGroup { get; set; }
    }

    public class IndexerParseResult
    {
        public ReportInfo Report { get; set; }
        public ParsedEpisodeInfo ParsedInfo { get; set; }

        public Series Series { get; set; }
        public IList<Episode> Episodes { get; set; }

        public DownloadDecision Decision { get; set; }

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
            var episodeString = new List<String>();
            var episodeNames = new List<String>();

            foreach (var episode in Episodes)
            {
                episodeString.Add(String.Format("{0}x{1:00}", episode.SeasonNumber, episode.EpisodeNumber));
                episodeNames.Add(Parser.Parser.CleanupEpisodeTitle(episode.Title));
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