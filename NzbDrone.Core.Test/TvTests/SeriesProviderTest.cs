﻿using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesProviderTest : CoreTest<SeriesService>
    {
        private Mock<ISeriesRepository> Repo;

        private Series fakeSeries;

        [SetUp]
        public void Setup()
        {
            Repo = Mocker.GetMock<ISeriesRepository>();
            fakeSeries = Builder<Series>.CreateNew().Build();
        }

  /*      [Test]
        public void Add_new_series()
        {
            const string path = "C:\\Test\\";
            const string title = "Test Title";
            const int tvDbId = 1234;
            const int qualityProfileId = 2;


            Series insertedModel = null;

            Repo.Setup(c => c.Insert(It.IsAny<Series>()))
                .Callback<Series>(c => insertedModel = c);

            Subject.AddSeries(title, path, tvDbId, qualityProfileId, null);

            insertedModel.Should().NotBeNull();
            insertedModel.Path.Should().Be(path);
            insertedModel.TvDbId.Should().Be(tvDbId);
            insertedModel.QualityProfileId.Should().Be(qualityProfileId);
            insertedModel.Title.Should().Be(title);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Add_new_series_should_use_config_for_season_folders(bool useSeasonFolder)
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(c => c.UseSeasonFolder).Returns(useSeasonFolder);

            const string path = "C:\\Test\\";
            const string title = "Test Title";
            const int tvDbId = 1234;
            const int qualityProfileId = 2;


            Series insertedModel = null;

            Repo.Setup(c => c.Insert(It.IsAny<Series>()))
                .Callback<Series>(c => insertedModel = c);

            Subject.AddSeries(title, path, tvDbId, qualityProfileId, null);

            insertedModel.SeasonFolder = useSeasonFolder;
        }*/

        [Test]
        public void is_monitored()
        {
            Repo.Setup(c => c.Get(12))
                .Returns(fakeSeries);

            fakeSeries.Monitored = true;

            Subject.IsMonitored(12).Should().Be(true);
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             