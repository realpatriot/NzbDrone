using System;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AllowedReleaseGroupSpecification : IDecisionEngineSpecification
    {
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public AllowedReleaseGroupSpecification(IConfigService configService, Logger logger)
        {
            _configService = configService;
            _logger = logger;
        }


        public string RejectionReason
        {
            get
            {
                return "Release group is blacklisted.";
            }
        }

        public virtual bool IsSatisfiedBy(RemoteEpisode subject)
        {
            _logger.Trace("Beginning release group check for: {0}", subject);

            //Todo: Make this use NzbRestrictions - How should whitelist be used? Will it override blacklist or vice-versa?

            //var allowed = _configService.AllowedReleaseGroups;
            const string allowed = "";

            if (string.IsNullOrWhiteSpace(allowed))
                return true;

            var releaseGroup = subject.Report.ReleaseGroup;

            foreach (var group in allowed.Trim(',', ' ').Split(','))
            {
                if (releaseGroup.Equals(group.Trim(' '), StringComparison.CurrentCultureIgnoreCase))
                {
                    _logger.Trace("Item: {0}'s release group is wanted: {1}", subject, releaseGroup);
                    return true;
                }
            }

            _logger.Trace("Item: {0}'s release group is not wanted: {1}", subject, releaseGroup);
            return false;
        }
    }
}
