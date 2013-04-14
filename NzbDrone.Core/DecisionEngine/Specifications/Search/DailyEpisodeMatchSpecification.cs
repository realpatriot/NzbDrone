using NLog;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class DailyEpisodeMatchSpecification : IDecisionEngineSearchSpecification
    {
        private readonly Logger _logger;
        private readonly IEpisodeService _episodeService;

        public DailyEpisodeMatchSpecification(Logger logger, IEpisodeService episodeService)
        {
            _logger = logger;
            _episodeService = episodeService;
        }

        public string RejectionReason
        {
            get
            {
                return "Episode doesn't match";
            }
        }
        public bool IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchDefinitionBase searchDefinitionBase)
        {
            var dailySearchSpec = searchDefinitionBase as DailyEpisodeSearchDefinition;

            if (dailySearchSpec == null) return true;

            var episode = _episodeService.GetEpisode(dailySearchSpec.SeriesId, dailySearchSpec.Airtime);

            if (!remoteEpisode.ParsedInfo.AirDate.HasValue || remoteEpisode.ParsedInfo.AirDate.Value != episode.AirDate.Value)
            {
                _logger.Trace("Episode AirDate does not match searched episode number, skipping.");
                return false;
            }

            return true;
        }
    }
}