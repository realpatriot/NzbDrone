﻿using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AllowedReleaseGroupSpecificationFixture : CoreTest<AllowedReleaseGroupSpecification>
    {
        private IndexerParseResult parseResult;

        [SetUp]
        public void Setup()
        {
            parseResult = new IndexerParseResult
                                    {
                                        SeriesTitle = "Title",
                                        Language = Language.English,
                                        Quality = new QualityModel(Quality.SDTV, true),
                                        EpisodeNumbers = new List<int> { 3 },
                                        SeasonNumber = 12,
                                        AirDate = DateTime.Now.AddDays(-12).Date,
                                        ReleaseGroup = "2HD"
                                    };
        }

        [Test]
        public void should_be_true_when_allowedReleaseGroups_is_empty()
        {
            Subject.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_allowedReleaseGroups_is_nzbs_releaseGroup()
        {
            Subject.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void should_be_true_when_allowedReleaseGroups_contains_nzbs_releaseGroup()
        {
            Subject.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void should_be_false_when_allowedReleaseGroups_does_not_contain_nzbs_releaseGroup()
        {
            Subject.IsSatisfiedBy(parseResult).Should().BeFalse();
        }
    }
}