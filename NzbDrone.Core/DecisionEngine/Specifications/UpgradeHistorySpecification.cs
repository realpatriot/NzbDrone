using NLog;
using NzbDrone.Core.History;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class UpgradeHistorySpecification : IDecisionEngineSpecification
    {
        private readonly IHistoryService _historyService;
        private readonly QualityUpgradableSpecification _qualityUpgradableSpecification;
        private readonly Logger _logger;

        public UpgradeHistorySpecification(IHistoryService historyService, QualityUpgradableSpecification qualityUpgradableSpecification, Logger logger)
        {
            _historyService = historyService;
            _qualityUpgradableSpecification = qualityUpgradableSpecification;
            _logger = logger;
        }

        public string RejectionReason
        {
            get
            {
                return "Higher quality report exists in history";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            foreach (var episode in subject.Episodes)
            {
                var bestQualityInHistory = _historyService.GetBestQualityInHistory(episode.Id);
                if (bestQualityInHistory != null)
                {
                    _logger.Trace("Comparing history quality with report. History is {0}", bestQualityInHistory);
                    if (!_qualityUpgradableSpecification.IsUpgradable(subject.Series.QualityProfile, bestQualityInHistory, subject.ParsedInfo.Quality))
                        return false;
                }
            }

            return true;
        }
    }
}
