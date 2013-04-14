using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Model;
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

            var queue = downloadClient.GetQueue().Select(q => Parser.Parser.ParseTitle<IndexerParseResult>(q.Title));

            return !IsInQueue(subject, queue);
        }

        public virtual bool IsInQueue(IndexerParseResult newParseResult, IEnumerable<IndexerParseResult> queue)
        {
            var matchingTitle = queue.Where(q => String.Equals(q.CleanTitle, newParseResult.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

            var matchingTitleWithQuality = matchingTitle.Where(q => q.Quality >= newParseResult.Quality);

            if (newParseResult.Series.SeriesType == SeriesTypes.Daily)
            {
                return matchingTitleWithQuality.Any(q => q.AirDate.Value.Date == newParseResult.AirDate.Value.Date);
            }

            var matchingSeason = matchingTitleWithQuality.Where(q => q.SeasonNumber == newParseResult.SeasonNumber);

            if (newParseResult.FullSeason)
            {
                return matchingSeason.Any();
            }

            return matchingSeason.Any(q => q.EpisodeNumbers != null && q.EpisodeNumbers.Any(e => newParseResult.EpisodeNumbers.Contains(e)));
        }

    }
}
