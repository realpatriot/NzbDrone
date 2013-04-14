﻿

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class AcceptableSizeSpecificationFixture : CoreTest
    {
        private IndexerParseResult parseResultMulti;
        private IndexerParseResult parseResultSingle;
        private Series series30minutes;
        private Series series60minutes;
        private QualitySize qualityType;

        [SetUp]
        public void Setup()
        {
            parseResultMulti = new IndexerParseResult
                                   {
                                       SeriesTitle = "Title",
                                       Language = Language.English,
                                       Quality = new QualityModel(Quality.SDTV, true),
                                       EpisodeNumbers = new List<int> { 3, 4 },
                                       SeasonNumber = 12,
                                       AirDate = DateTime.Now.AddDays(-12).Date
                                   };

            parseResultSingle = new IndexerParseResult
                                    {
                                        SeriesTitle = "Title",
                                        Language = Language.English,
                                        Quality = new QualityModel(Quality.SDTV, true),
                                        EpisodeNumbers = new List<int> { 3 },
                                        SeasonNumber = 12,
                                        AirDate = DateTime.Now.AddDays(-12).Date
                                    };

            series30minutes = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
                .With(c => c.Runtime = 30)
                .Build();

            series60minutes = Builder<Series>.CreateNew()
                .With(c => c.Monitored = true)
                .With(d => d.CleanTitle = parseResultMulti.CleanTitle)
                .With(c => c.Runtime = 60)
                .Build();

            qualityType = Builder<QualitySize>.CreateNew()
                .With(q => q.MinSize = 0)
                .With(q => q.MaxSize = 10)
                .With(q => q.QualityId = 1)
                .Build();

        }

        [Test]
        public void IsAcceptableSize_true_single_episode_not_first_or_last_30_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 184572800;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_not_first_or_last_60_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 368572800;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_not_first_or_last_30_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 1.Gigabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_not_first_or_last_60_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 1.Gigabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_true_multi_episode_not_first_or_last_30_minute()
        {
            WithStrictMocker();

            parseResultMulti.Series = series30minutes;
            parseResultMulti.Size = 184572800;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultMulti);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_multi_episode_not_first_or_last_60_minute()
        {
            WithStrictMocker();

            parseResultMulti.Series = series60minutes;
            parseResultMulti.Size = 368572800;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultMulti);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_false_multi_episode_not_first_or_last_30_minute()
        {
            WithStrictMocker();

            parseResultMulti.Series = series30minutes;
            parseResultMulti.Size = 1.Gigabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultMulti);

            
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_false_multi_episode_not_first_or_last_60_minute()
        {
            WithStrictMocker();

            parseResultMulti.Series = series60minutes;
            parseResultMulti.Size = 10.Gigabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(false);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultMulti);

            
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_first_30_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 184572800;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_single_episode_first_60_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 368572800;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_first_30_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 1.Gigabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_false_single_episode_first_60_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 10.Gigabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeFalse();
        }

        [Test]
        public void IsAcceptableSize_true_unlimited_30_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series30minutes;
            parseResultSingle.Size = 18457280000;
            qualityType.MaxSize = 0;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_true_unlimited_60_minute()
        {
            WithStrictMocker();

            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 36857280000;
            qualityType.MaxSize = 0;

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void IsAcceptableSize_should_treat_daily_series_as_single_episode()
        {
            parseResultSingle.Series = series60minutes;
            parseResultSingle.Size = 300.Megabytes();
            parseResultSingle.AirDate = DateTime.Today;
            parseResultSingle.EpisodeNumbers = null;

            qualityType.MaxSize = (int)600.Megabytes();

            Mocker.GetMock<IQualitySizeService>().Setup(s => s.Get(1)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.IsFirstOrLastEpisodeOfSeason(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(true);

            
            bool result = Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResultSingle);

            
            result.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_RAWHD()
        {
            var parseResult = new IndexerParseResult
                {
                        Quality = new QualityModel(Quality.RAWHD, false)
                };

            Mocker.Resolve<AcceptableSizeSpecification>().IsSatisfiedBy(parseResult).Should().BeTrue();
        }
    }
}