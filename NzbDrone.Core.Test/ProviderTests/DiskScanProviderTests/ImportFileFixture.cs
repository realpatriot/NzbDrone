﻿using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{

    public class ImportFileFixture : CoreTest<DiskScanProvider>
    {


        private long _size = 80.Megabytes();
        private Series _fakeSeries;
        private List<Episode> _fakeEpisodes;
        private Episode _fakeEpisode;

        [SetUp]
        public void Setup()
        {
            _fakeSeries = Builder<Series>
                    .CreateNew()
                    .Build();

            _fakeEpisode = Builder<Episode>
                    .CreateNew()
                    .Build();


            _fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.EpisodeFile = new EpisodeFile())
                                        .BuildList();

            GivenNewFile();

            GivenVideoDuration(TimeSpan.FromMinutes(20));

        }

        private void GivenFileSize(long size)
        {
            _size = size;

            Mocker.GetMock<DiskProvider>()
                    .Setup(d => d.GetSize(It.IsAny<String>()))
                    .Returns(size);
        }

        private void GivenVideoDuration(TimeSpan duration)
        {
            Mocker.GetMock<VideoFileInfoReader>()
                    .Setup(d => d.GetRunTime(It.IsAny<String>()))
                    .Returns(duration);
        }


        private void GivenEpisodes(IEnumerable<Episode> episodes, QualityModel quality)
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetEpisodes(It.IsAny<string>(), It.IsAny<Series>()))
                  .Returns(new LocalEpisode
                      {
                          Episodes = episodes.ToList(),
                          Quality = quality
                      });
        }

        private void GivenNewFile()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(p => p.Exists(It.IsAny<String>()))
                  .Returns(false);
        }

        [Test]
        public void import_new_file_should_succeed()
        {
            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel());

            var result = Subject.ImportFile(_fakeSeries, "file.ext");
            VerifyFileImport(result);
        }



        [Test]
        public void import_new_file_with_better_same_quality_should_succeed()
        {
            _fakeEpisode.EpisodeFile.Quality = new QualityModel(Quality.SDTV);
            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.SDTV));
            
            GivenEpisodes(_fakeEpisodes, new QualityModel(Quality.Bluray1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");
            VerifyFileImport(result);
        }

        [Test]
        public void import_new_file_episode_has_better_quality_should_skip()
        {
            _fakeEpisode.EpisodeFile.Quality = new QualityModel(Quality.HDTV1080p);
            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.SDTV));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifySkipImport(result);
        }

        [Test]
        public void import_new_file_episode_has_same_quality_should_skip()
        {
            _fakeEpisode.EpisodeFile.Quality = new QualityModel(Quality.SDTV);
            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.SDTV));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");


            VerifySkipImport(result);
        }

        [Test]
        public void import_unparsable_file_should_skip()
        {
            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.GetEpisodes(It.IsAny<string>(), It.IsAny<Series>()))
                  .Returns<LocalEpisode>(null);

            var result = Subject.ImportFile(_fakeSeries, "file.ext");


            VerifySkipImport(result);
        }

        [Test]
        public void import_existing_file_should_skip()
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(p => p.Exists(It.IsAny<String>()))
                .Returns(true);

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifySkipImport(result);
        }

        [Test]
        public void import_file_with_no_episode_in_db_should_skip()
        {
            GivenEpisodes(new List<Episode>(), new QualityModel());

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifySkipImport(result);
        }

        [Test]
        public void import_new_multi_part_file_episode_with_better_quality_than_existing()
        {

            _fakeEpisodes[0].EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) };
            _fakeEpisodes[1].EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.SDTV) };

            GivenEpisodes(_fakeEpisodes, new QualityModel(Quality.HDTV1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");


            VerifyFileImport(result);
        }

        [Test]
        public void skip_import_new_multi_part_file_episode_existing_has_better_quality(string fileName)
        {
            _fakeEpisodes[0].EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV1080p) };
            _fakeEpisodes[1].EpisodeFile = new EpisodeFile { Quality = new QualityModel(Quality.HDTV1080p) };

            GivenEpisodes(_fakeEpisodes, new QualityModel(Quality.SDTV));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");


            VerifySkipImport(result);
        }


        [Test]
        public void should_skip_if_file_size_is_under_70MB_and_runTime_under_3_minutes()
        {
            GivenFileSize(50.Megabytes());
            GivenVideoDuration(TimeSpan.FromMinutes(1));

            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.HDTV1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifySkipImport(result);
        }

        [Test]
        public void should_import_if_file_size_is_under_70MB_but_runTime_over_3_minutes()
        {
            GivenFileSize(50.Megabytes());
            GivenVideoDuration(TimeSpan.FromMinutes(20));

            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.HDTV1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifyFileImport(result);
            Mocker.GetMock<DiskProvider>().Verify(p => p.DeleteFile(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void should_import_if_file_size_is_over_70MB_but_runTime_under_3_minutes()
        {
            GivenFileSize(100.Megabytes());
            GivenVideoDuration(TimeSpan.FromMinutes(1));

            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.HDTV1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifyFileImport(result);
        }

        [Test]
        public void should_import_special_even_if_file_size_is_under_70MB_and_runTime_under_3_minutes()
        {
            GivenFileSize(10.Megabytes());
            GivenVideoDuration(TimeSpan.FromMinutes(1));

            _fakeEpisode.SeasonNumber = 0;

            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.HDTV1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifyFileImport(result);
        }

        [Test]
        public void should_skip_if_daily_series_with_file_size_is_under_70MB_and_runTime_under_3_minutes()
        {
            GivenFileSize(10.Megabytes());
            GivenVideoDuration(TimeSpan.FromMinutes(1));

            _fakeEpisode.SeasonNumber = 0;
            _fakeSeries.SeriesType = SeriesTypes.Daily;

            GivenEpisodes(new[] { _fakeEpisode }, new QualityModel(Quality.HDTV1080p));

            var result = Subject.ImportFile(_fakeSeries, "file.ext");

            VerifySkipImport(result);
        }

        private void VerifyFileImport(EpisodeFile result)
        {
            result.Should().NotBeNull();
            result.SeriesId.Should().Be(_fakeSeries.Id);
            result.Size.Should().Be(_size);
            result.DateAdded.Should().HaveDay(DateTime.Now.Day);

            Mocker.GetMock<IMediaFileService>().Verify(c => c.Add(result), Times.Once());
        }

        private void VerifySkipImport(EpisodeFile result)
        {
            result.Should().BeNull();
            Mocker.GetMock<IMediaFileService>().Verify(p => p.Add(It.IsAny<EpisodeFile>()), Times.Never());
        }
    }
}
