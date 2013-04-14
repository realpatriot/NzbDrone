using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IDecisionEngineSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(RemoteEpisode subject);
    }
}