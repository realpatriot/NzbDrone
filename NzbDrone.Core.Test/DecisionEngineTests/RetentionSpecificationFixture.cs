﻿using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class RetentionSpecificationFixture : CoreTest
    {
        private RetentionSpecification retentionSpecification;

        private RemoteEpisode parseResult;

        [SetUp]
        public void Setup()
        {
            retentionSpecification = Mocker.Resolve<RetentionSpecification>();

            parseResult = new RemoteEpisode
            {
                Age = 100
            };
        }

        private void WithUnlimitedRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(0);
        }

        private void WithLongRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(1000);
        }

        private void WithShortRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(10);
        }

        private void WithEqualRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(100);
        }

        [Test]
        public void unlimited_retention_should_return_true()
        {
            WithUnlimitedRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void longer_retention_should_return_true()
        {
            WithLongRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void equal_retention_should_return_true()
        {
            WithEqualRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void shorter_retention_should_return_false()
        {
            WithShortRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeFalse();
        }

        [Test]
        public void zeroDay_report_should_return_true()
        {
            WithUnlimitedRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }
    }
}