﻿using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Jobs.Implementations;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    public class RenameSeasonJobFixture : TestBase
    {
        private ProgressNotification _testNotification;
        private Series _series;
        private IList<EpisodeFile> _episodeFiles;

        [SetUp]
        public void Setup()
        {
            _testNotification = new ProgressNotification("TEST");

            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episodeFiles = Builder<EpisodeFile>
                    .CreateListOfSize(5)
                    .All()
                    .With(e => e.SeasonNumber = 5)
                    .Build();

            Mocker.GetMock<ISeriesRepository>()
                  .Setup(s => s.Get(_series.Id))
                  .Returns(_series);

            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.GetFilesBySeason(_series.Id, 5))
                  .Returns(_episodeFiles.ToList());
        }


        [Test]
        public void should_throw_if_seriesId_is_zero()
        {
            Assert.Throws<ArgumentException>(() =>
                Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = 0, SeasonNumber = 10 }));
        }

        [Test]
        public void should_throw_if_seasonId_is_less_than_zero()
        {
            Assert.Throws<ArgumentException>(() =>
                Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = _series.Id, SeasonNumber = -10 }));
        }

        [Test]
        public void should_log_warning_if_no_episode_files_are_found()
        {
            Mocker.Resolve<RenameSeasonJob>().Start(_testNotification, new { SeriesId = _series.Id, SeasonNumber = 10 });

            ExceptionVerification.ExpectedWarns(1);
        }


    }
}
