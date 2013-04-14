using System.Collections.Generic;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Parser
{
    public class LocalEpisode
    {
        public ParsedEpisodeInfo ParsedEpisodeInfo { get; set; }
        public List<Episode> Episodes { get; set; }
        public Series Series { get; set; }
    }
}