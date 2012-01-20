﻿// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.InventoryProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IsUpgradePossibleFixture : CoreTest
    {
        private void WithWebdlCutoff()
        {
            var profile = new QualityProfile { Cutoff = QualityTypes.WEBDL };
            Mocker.GetMock<QualityProvider>().Setup(s => s.Get(It.IsAny<int>())).Returns(profile);
        }

        private Series _series;
        private EpisodeFile _episodeFile;
        private Episode _episode;

        [SetUp]
        public void SetUp()
        {
            _series = Builder<Series>.CreateNew()
                    .Build();

            _episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.Quality = QualityTypes.SDTV)
                    .Build();

            _episode = Builder<Episode>.CreateNew()
                    .With(e => e.EpisodeFileId = 0)
                    .With(e => e.SeriesId = _series.SeriesId)
                    .With(e => e.Series = _series)
                    .With(e => e.EpisodeFileId = _episodeFile.EpisodeFileId)
                    .With(e => e.EpisodeFile = _episodeFile)
                    .Build();
        }

        [Test]
        public void IsUpgradePossible_should_return_true_if_no_episode_file_exists()
        {
            var episode = Builder<Episode>.CreateNew()
                    .With(e => e.EpisodeFileId = 0)
                    .Build();

            //Act
            bool result = Mocker.Resolve<InventoryProvider>().IsUpgradePossible(episode);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpgradePossible_should_return_true_if_current_episode_is_less_than_cutoff()
        {
            WithWebdlCutoff();

            //Act
            bool result = Mocker.Resolve<InventoryProvider>().IsUpgradePossible(_episode);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpgradePossible_should_return_false_if_current_episode_is_equal_to_cutoff()
        {
            WithWebdlCutoff();

            _episodeFile.Quality = QualityTypes.WEBDL;

            //Act
            bool result = Mocker.Resolve<InventoryProvider>().IsUpgradePossible(_episode);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgradePossible_should_return_false_if_current_episode_is_greater_than_cutoff()
        {
            WithWebdlCutoff();

            _episodeFile.Quality = QualityTypes.Bluray720p;

            //Act
            bool result = Mocker.Resolve<InventoryProvider>().IsUpgradePossible(_episode);

            //Assert
            result.Should().BeFalse();
        }
    }
}