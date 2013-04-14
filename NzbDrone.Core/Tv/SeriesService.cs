﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marr.Data;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Eventing;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Model;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv
{
    public interface ISeriesService
    {
        bool IsMonitored(int id);
        Series UpdateSeriesInfo(int seriesId);
        Series GetSeries(int seriesId);
        void AddSeries(Series newSeries);
        void UpdateFromSeriesEditor(IList<Series> editedSeries);
        Series FindByTvdbId(int tvdbId);
        void SetSeriesType(int seriesId, SeriesTypes seriesTypes);
        void DeleteSeries(int seriesId, bool deleteFiles);
        List<Series> GetAllSeries();
        void UpdateSeries(Series series);
    }

    public class SeriesService : ISeriesService, IHandleAsync<SeriesAddedEvent>
    {
        private readonly ISeriesRepository _seriesRepository;
        private readonly IConfigService _configService;
        private readonly IProvideSeriesInfo _seriesInfoProxy;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRootFolderService _rootFolderService;
        private readonly DiskProvider _diskProvider;
        private readonly Logger _logger;

        public SeriesService(ISeriesRepository seriesRepository, IConfigService configServiceService,
                             IProvideSeriesInfo seriesInfoProxy, IEventAggregator eventAggregator,
                             IRootFolderService rootFolderService, DiskProvider diskProvider, Logger logger)
        {
            _seriesRepository = seriesRepository;
            _configService = configServiceService;
            _seriesInfoProxy = seriesInfoProxy;
            _eventAggregator = eventAggregator;
            _rootFolderService = rootFolderService;
            _diskProvider = diskProvider;
            _logger = logger;
        }


        public bool IsMonitored(int id)
        {
            return _seriesRepository.Get(id).Monitored;
        }


        public Series UpdateSeriesInfo(int seriesId)
        {
            var series = _seriesRepository.Get(seriesId);
            var tvDbSeries = _seriesInfoProxy.GetSeriesInfo(series.TvDbId);

            series.Title = tvDbSeries.Title;
            series.AirTime = tvDbSeries.AirTime;
            series.Overview = tvDbSeries.Overview;
            series.Status = tvDbSeries.Status;
            series.CleanTitle = Parser.Parser.NormalizeTitle(tvDbSeries.Title);
            series.LastInfoSync = DateTime.Now;
            series.Runtime = tvDbSeries.Runtime;
            series.Images = tvDbSeries.Images;
            series.Network = tvDbSeries.Network;
            series.FirstAired = tvDbSeries.FirstAired;
            _seriesRepository.Update(series);

            _eventAggregator.Publish(new SeriesUpdatedEvent(series));

            return series;
        }


        public Series GetSeries(int seriesId)
        {
            return _seriesRepository.Get(seriesId);
        }

        public void AddSeries(Series newSeries)
        {
            Ensure.That(() => newSeries).IsNotNull();

            if(String.IsNullOrWhiteSpace(newSeries.FolderName))
            {
                newSeries.FolderName = FileNameBuilder.CleanFilename(newSeries.Title);
                _diskProvider.CreateDirectory(Path.Combine(_rootFolderService.Get(newSeries.RootFolderId).Path, newSeries.FolderName));
            }

            _logger.Info("Adding Series [{0}] Path: [{1}]", newSeries.Title, newSeries.Path);

            newSeries.Monitored = true;
            newSeries.CleanTitle = Parser.Parser.NormalizeTitle(newSeries.Title);
            if (newSeries.QualityProfileId == 0)
                newSeries.QualityProfileId = _configService.DefaultQualityProfile;

            newSeries.SeasonFolder = _configService.UseSeasonFolder;
            newSeries.BacklogSetting = BacklogSettingType.Inherit;

            _seriesRepository.Insert(newSeries);
            _eventAggregator.Publish(new SeriesAddedEvent(newSeries));
        }

        public void UpdateFromSeriesEditor(IList<Series> editedSeries)
        {
            var allSeries = _seriesRepository.All();

            foreach (var series in allSeries)
            {
                //Only update parameters that can be changed in MassEdit
                var edited = editedSeries.Single(s => s.Id == series.Id);
                series.QualityProfileId = edited.QualityProfileId;
                series.Monitored = edited.Monitored;
                series.SeasonFolder = edited.SeasonFolder;
                series.BacklogSetting = edited.BacklogSetting;
                //series.Path = edited.Path;
                series.CustomStartDate = edited.CustomStartDate;

                _seriesRepository.Update(series);
            }

        }

        public void SetTvRageId(int seriesId, int tvRageId)
        {
            _seriesRepository.SetTvRageId(seriesId, tvRageId);
        }

        public Series FindByTvdbId(int tvdbId)
        {
            return _seriesRepository.FindByTvdbId(tvdbId);
        }

        public void SetSeriesType(int seriesId, SeriesTypes seriesTypes)
        {
            _seriesRepository.SetSeriesType(seriesId, seriesTypes);
        }

        public void DeleteSeries(int seriesId, bool deleteFiles)
        {
            var series = _seriesRepository.Get(seriesId);
            _seriesRepository.Delete(seriesId);
            _eventAggregator.Publish(new SeriesDeletedEvent(series, deleteFiles));
        }

        public List<Series> GetAllSeries()
        {
            return _seriesRepository.All().ToList();
        }

        public void UpdateSeries(Series series)
        {
            _seriesRepository.Update(series);
        }

        public void HandleAsync(SeriesAddedEvent message)
        {
            UpdateSeriesInfo(message.Series.Id);
        }
    }
}
