using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IMakeDownloadDecision
    {
        IEnumerable<DownloadDecision> GetRssDecision(IEnumerable<ReportInfo> reports);
        IEnumerable<DownloadDecision> GetSearchDecision(IEnumerable<ReportInfo> reports, SearchDefinitionBase searchDefinitionBase);
    }

    public class DownloadDecisionMaker : IMakeDownloadDecision
    {
        private readonly IEnumerable<IRejectWithReason> _specifications;
        private readonly IParsingService _parsingService;

        public DownloadDecisionMaker(IEnumerable<IRejectWithReason> specifications, IParsingService parsingService)
        {
            _specifications = specifications;
            _parsingService = parsingService;
        }

        public IEnumerable<DownloadDecision> GetRssDecision(IEnumerable<ReportInfo> reports)
        {
            foreach (var report in reports)
            {
                var parseResult = _parsingService.Map(report);

                parseResult.Decision = new DownloadDecision(parseResult, GetGeneralRejectionReasons(parseResult).ToArray());
                yield return parseResult.Decision;
            }

        }

        public IEnumerable<DownloadDecision> GetSearchDecision(IEnumerable<ReportInfo> reports, SearchDefinitionBase searchDefinitionBase)
        {
            foreach (var report in reports)
            {
                var parseResult = _parsingService.Map(report);
                var generalReasons = GetGeneralRejectionReasons(parseResult);
                var searchReasons = GetSearchRejectionReasons(parseResult, searchDefinitionBase);

                parseResult.Decision = new DownloadDecision(parseResult, generalReasons.Union(searchReasons).ToArray());

                yield return parseResult.Decision;
            }
        }


        private IEnumerable<string> GetGeneralRejectionReasons(RemoteEpisode report)
        {
            return _specifications
                .OfType<IDecisionEngineSpecification>()
                .Where(spec => !spec.IsSatisfiedBy(report))
                .Select(spec => spec.RejectionReason);
        }

        private IEnumerable<string> GetSearchRejectionReasons(RemoteEpisode report, SearchDefinitionBase searchDefinitionBase)
        {
            return _specifications
                .OfType<IDecisionEngineSearchSpecification>()
                .Where(spec => !spec.IsSatisfiedBy(report, searchDefinitionBase))
                .Select(spec => spec.RejectionReason);
        }
    }
}