using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.DecisionEngine
{
    public interface IDecisionEngineSpecification : IRejectWithReason
    {
        bool IsSatisfiedBy(RemoteEpisode subject);
    }
}