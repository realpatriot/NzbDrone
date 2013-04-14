﻿

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.DecisionEngine;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    
    public class LanguageSpecificationFixture : CoreTest
    {
        private IndexerParseResult parseResult;

        private void WithEnglishRelease()
        {
            parseResult = Builder<IndexerParseResult>
                    .CreateNew()
                    .With(p => p.Language = Language.English)
                    .Build();
        }

        private void WithGermanRelease()
        {
            parseResult = Builder<IndexerParseResult>
                    .CreateNew()
                    .With(p => p.Language = Language.German)
                    .Build();
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(parseResult).Should().BeFalse();
        }
    }
}