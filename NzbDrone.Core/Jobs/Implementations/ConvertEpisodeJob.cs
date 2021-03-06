using System;
using System.Linq;
using NLog;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Converting;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Jobs.Implementations
{
    public class ConvertEpisodeJob : IJob
    {
        private readonly HandbrakeProvider _handbrakeProvider;
        private readonly AtomicParsleyProvider _atomicParsleyProvider;
        private readonly IEpisodeService _episodeService;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ConvertEpisodeJob(HandbrakeProvider handbrakeProvider, AtomicParsleyProvider atomicParsleyProvider,
                                    IEpisodeService episodeService)
        {
            _handbrakeProvider = handbrakeProvider;
            _atomicParsleyProvider = atomicParsleyProvider;
            _episodeService = episodeService;
        }

        public string Name
        {
            get { return "Convert Episode"; }
        }

        public TimeSpan DefaultInterval
        {
            get { return TimeSpan.FromTicks(0); }
        }

        public void Start(ProgressNotification notification, dynamic options)
        {

            if (options == null || options.EpisodeId <= 0)
                throw new ArgumentNullException(options);

            Episode episode = _episodeService.GetEpisode((int)options.EpisodeId);
            notification.CurrentMessage = String.Format("Starting Conversion for {0}", episode);
            var outputFile = _handbrakeProvider.ConvertFile(episode, notification);

            if (String.IsNullOrEmpty(outputFile))
                notification.CurrentMessage = String.Format("Conversion failed for {0}", episode);

            _atomicParsleyProvider.RunAtomicParsley(episode, outputFile);

            notification.CurrentMessage = String.Format("Conversion completed for {0}", episode);
        }
    }
}