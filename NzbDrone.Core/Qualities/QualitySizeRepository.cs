﻿using System.Data;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public interface IQualitySizeRepository : IBasicRepository<QualitySize>
    {
        QualitySize GetByQualityId(int qualityId);
    }

    public class QualitySizeRepository : BasicRepository<QualitySize>, IQualitySizeRepository
    {
        public QualitySizeRepository(IDatabase database)
            : base(database)
        {
        }

        public QualitySize GetByQualityId(int qualityId)
        {
            return Query.Single(q => q.QualityId == qualityId);
        }
    }
}
