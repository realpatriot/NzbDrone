using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class NotInQueueSpecification : IDecisionEngineSpecification
    {
        private readonly IProvideDownloadClient _downloadClientProvider;

        public NotInQueueSpecification(IProvideDownloadClient downloadClientProvider)
        {
            _downloadClientProvider = downloadClientProvider;
        }

        public string RejectionReason
        {
            get
            {
                return "Already in download queue.";
            }
        }

        public virtual bool IsSatisfiedBy(IndexerParseResult subject)
        {
            var downloadClient = _downloadClientProvider.GetDownloadClient();

            var queue = downloadClient.GetQueue().Select(q => Parser.SimpleParser.ParseTitle(q.Title));

            return !IsInQueue(subject, queue);
        }

        public virtual bool IsInQueue(IndexerParseResult newParseResult, IEnumerable<ParsedEpisodeInfo> queue)
        {
            var matchingTitle = queue.Where(q => String.Equals(q.SeriesTitle, newParseResult.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

            var matchingTitleWithQuality = matchingTitle.Where(q => q.Quality >= newParseResult.ParsedInfo.Quality);

            if (newParseResult.Series.SeriesType == SeriesTypes.Daily)
            {
                return matchingTitleWithQuality.Any(q => q.AirDate.Value.Date == newParseResult.ParsedInfo.AirDate.Value.Date);
            }

            var matchingSeason = matchingTitleWithQuality.Where(q => q.SeasonNumber == newParseResult.ParsedInfo.SeasonNumber);

            if (newParseResult.ParsedInfo.FullSeason)
            {
                return matchingSeason.Any();
            }

            return matchingSeason.Any(q => q.EpisodeNumbers != null && q.EpisodeNumbers.Any(e => newParseResult.ParsedInfo.EpisodeNumbers.Contains(e)));
        }

    }
}
