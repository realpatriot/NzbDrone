using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Model;


namespace NzbDrone.Core.Tv
{
    public interface IEpisodeRepository : IBasicRepository<Episode>
    {
        Episode Get(int seriesId, int season, int episodeNumber);
        Episode Get(int seriesId, DateTime date);
        List<Episode> GetEpisodes(int seriesId);
        List<Episode> GetEpisodes(int seriesId, int seasonNumber);
        List<Episode> GetEpisodeByFileId(int fileId);
        List<Episode> EpisodesWithoutFiles(bool includeSpecials);
        Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber);
        List<Episode> EpisodesWithFiles();
        List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate);
        void SetIgnoreFlat(Episode episode, bool ignoreFlag);
        void SetPostDownloadStatus(int episodeId, PostDownloadStatusType status);
        void SetFileId(int episodeId, int fileId);
    }

    public class EpisodeRepository : BasicRepository<Episode>, IEpisodeRepository
    {
        public EpisodeRepository(IDatabase database)
            : base(database)
        {
        }

        public Episode Get(int seriesId, int season, int episodeNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == season && s.EpisodeNumber == episodeNumber);
        }

        public Episode Get(int seriesId, DateTime date)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.AirDate.HasValue && s.AirDate.Value.Date == date.Date);
        }

        public List<Episode> GetEpisodes(int seriesId)
        {
            return Query.Where(s => s.SeriesId == seriesId).ToList();
        }

        public List<Episode> GetEpisodes(int seriesId, int seasonNumber)
        {
            return Query.Where(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber).ToList();
        }

        public List<Episode> GetEpisodeByFileId(int fileId)
        {
            return Query.Where(s => s.EpisodeFile != null && s.EpisodeFile.Id == fileId).ToList();
        }

        public List<Episode> EpisodesWithoutFiles(bool includeSpecials)
        {
            if (!includeSpecials)
            {
                return Query.Where(s => s.EpisodeFileId == 0 && s.SeasonNumber != 0).ToList();
            }

            return Query.Where(s => s.EpisodeFileId == 0).ToList();
        }

        public Episode GetEpisodeBySceneNumbering(int seriesId, int seasonNumber, int episodeNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber && s.SceneEpisodeNumber == episodeNumber);
        }

        public List<Episode> EpisodesWithFiles()
        {
            return Query.Where(s => s.EpisodeFile != null).ToList();
        }

        public List<Episode> EpisodesBetweenDates(DateTime startDate, DateTime endDate)
        {
            return Query//.Join<Episode, Series>(JoinType.None, e => e.Series, (e, s) => e.SeriesId == s.Id);
                          .Where<Episode>(e => e.AirDate >= startDate && e.AirDate <= endDate).ToList();
        }

        public void SetIgnoreFlat(Episode episode, bool ignoreFlag)
        {
            episode.Ignored = ignoreFlag;
            SetFields(episode, p => p.Ignored);
        }

        public void SetPostDownloadStatus(int episodeId, PostDownloadStatusType status)
        {
            SetFields(new Episode { Id = episodeId, PostDownloadStatus = status }, episode => episode.PostDownloadStatus);
        }

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Episode { Id = episodeId, EpisodeFileId = fileId }, episode => episode.EpisodeFileId);
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      