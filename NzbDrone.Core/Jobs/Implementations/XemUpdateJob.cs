using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class XemUpdateJob : IJob
    {
        private readonly XemProvider _xemProvider;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public XemUpdateJob(XemProvider xemProvider)
        {
            _xemProvider = xemProvider;
        }

        public string Name
        {
            get { return "XEM Update"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromHours(12); }
        }

        public virtual void Start(ProgressNotification notification, dynamic options)
        {
            if (options == null || options.SeriesId == 0)
            {
                _logger.Trace("Starting XEM Update for all series");
                _xemProvider.UpdateMappings();
            }

            else
            {
                _logger.Trace("Starting XEM Update for series: {0}", options.SeriesId);
                _xemProvider.UpdateMappings((int)options.SeriesId);
            }

            _logger.Trace("XEM Update complete");
        }
    }
}