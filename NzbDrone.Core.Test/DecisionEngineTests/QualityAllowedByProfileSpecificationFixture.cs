﻿

using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class QualityAllowedByProfileSpecificationFixture : CoreTest<QualityAllowedByProfileSpecification>
    {
        private RemoteEpisode parseResult;

        public static object[] AllowedTestCases =
        {
            new object[] { Quality.DVD },
            new object[] { Quality.HDTV720p },
            new object[] { Quality.Bluray1080p }
        };

        public static object[] DeniedTestCases =
        {
            new object[] { Quality.SDTV },
            new object[] { Quality.WEBDL720p },
            new object[] { Quality.Bluray720p }
        };

        [SetUp]
        public void Setup()
        {
            var fakeSeries = Builder<Series>.CreateNew()
                         .With(c => c.QualityProfile = new QualityProfile { Cutoff = Quality.Bluray1080p })
                         .Build();

            parseResult = new RemoteEpisode
            {
                Series = fakeSeries,
                Quality = new QualityModel(Quality.DVD, true),
            };
        }

        [Test, TestCaseSource("AllowedTestCases")]
        public void should_allow_if_quality_is_defined_in_profile(Quality qualityType)
        {
            parseResult.Quality.Quality = qualityType;
            parseResult.Series.QualityProfile.Allowed = new List<Quality> { Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p };

            Subject.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test, TestCaseSource("DeniedTestCases")]
        public void should_not_allow_if_quality_is_not_defined_in_profile(Quality qualityType)
        {
            parseResult.Quality.Quality = qualityType;
            parseResult.Series.QualityProfile.Allowed = new List<Quality> { Quality.DVD, Quality.HDTV720p, Quality.Bluray1080p };

            Subject.IsSatisfiedBy(parseResult).Should().BeFalse();
        }
    }
}