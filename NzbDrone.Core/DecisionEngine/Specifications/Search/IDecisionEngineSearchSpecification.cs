using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public interface IDecisionEngineSearchSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(RemoteEpisode remoteEpisode, SearchDefinitionBase searchDefinitionBase);
    }
}