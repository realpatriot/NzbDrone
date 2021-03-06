using System.Collections.Generic;
using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Tv
{
    public interface ISeasonRepository : IBasicRepository<Season>
    {
        IList<int> GetSeasonNumbers(int seriesId);
        Season Get(int seriesId, int seasonNumber);
        bool IsIgnored(int seriesId, int seasonNumber);
        List<Season> GetSeasonBySeries(int seriesId);
    }

    public class SeasonRepository : BasicRepository<Season>, ISeasonRepository
    {
        private readonly IDbConnection _database;

        public SeasonRepository(IDatabase database)
            : base(database)
        {
        }

        public IList<int> GetSeasonNumbers(int seriesId)
        {
            return Query.Where(c => c.SeriesId == seriesId).Select(c => c.SeasonNumber).ToList();
        }

        public Season Get(int seriesId, int seasonNumber)
        {
            return Query.Single(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);
        }

        public bool IsIgnored(int seriesId, int seasonNumber)
        {
            var season = Query.SingleOrDefault(s => s.SeriesId == seriesId && s.SeasonNumber == seasonNumber);

            if (season == null) return false;

            return season.Ignored;
        }

        public List<Season> GetSeasonBySeries(int seriesId)
        {
            return Query.Where(s => s.SeriesId == seriesId);
        }
    }
}