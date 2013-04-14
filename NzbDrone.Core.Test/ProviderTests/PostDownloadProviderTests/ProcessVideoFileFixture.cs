
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using FizzWare.NBuilder;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;

using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.PostDownloadProviderTests
{
    [TestFixture]
    public class ProcessVideoFileFixture : CoreTest
    {
        Series fakeSeries;

        [SetUp]
        public void Setup()
        {
            fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.RootFolder = new LazyLoaded<RootFolder>(new RootFolder { Path = @"C:\Test\TV" }))
                .With(s => s.FolderName = "30 Rock")
                .Build();
        }

        private void WithOldWrite()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetLastFileWrite(It.IsAny<String>()))
                .Returns(DateTime.Now.AddDays(-5));
        }

        private void WithRecentWrite()
        {
            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.GetLastFileWrite(It.IsAny<String>()))
                .Returns(DateTime.UtcNow);
        }

        private void WithValidSeries()
        {
            Mocker.GetMock<ISeriesRepository>()
                .Setup(c => c.FindByTitle(It.IsAny<string>()))
                .Returns(fakeSeries);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.FolderExists(fakeSeries.Path))
                    .Returns(true);
        }

        private void WithImportableFiles()
        {
            Mocker.GetMock<DiskScanProvider>()
                .Setup(c => c.Scan(It.IsAny<Series>(), It.IsAny<string>()))
                .Returns(Builder<EpisodeFile>.CreateListOfSize(1).Build().ToList());
        }

        private void WithLotsOfFreeDiskSpace()
        {
            Mocker.GetMock<DiskProvider>().Setup(s => s.FreeDiskSpace(It.IsAny<string>())).Returns(1000000000);
        }

        private void WithImportedFile(string file)
        {
            var fakeEpisodeFile = Builder<EpisodeFile>.CreateNew()
                .With(f => f.SeriesId = fakeSeries.Id)
                .Build();

            Mocker.GetMock<DiskScanProvider>().Setup(s => s.ImportFile(fakeSeries, file)).Returns(fakeEpisodeFile);
        }

        [Test]
        public void should_skip_if_and_too_fresh()
        {
            WithStrictMocker();
            WithRecentWrite();

            var file = Path.Combine(TempFolder, "test.avi");

            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(file);
        }

        [Test]
        public void should_continue_processing_if_not_fresh()
        {
            WithOldWrite();

            var file = Path.Combine(TempFolder, "test.avi");

            
            Mocker.GetMock<ISeriesRepository>().Setup(s => s.FindByTitle(It.IsAny<String>())).Returns<Series>(null).Verifiable();
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(file);

            
            Mocker.GetMock<ISeriesRepository>().Verify(s => s.FindByTitle(It.IsAny<String>()), Times.Once());
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_return_if_series_is_not_found()
        {
            WithOldWrite();

            var file = Path.Combine(TempFolder, "test.avi");

            
            Mocker.GetMock<ISeriesRepository>().Setup(s => s.FindByTitle(It.IsAny<String>())).Returns<Series>(null);
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(file);

            
            Mocker.GetMock<DiskProvider>().Verify(s => s.GetSize(It.IsAny<String>()), Times.Never());
            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_move_file_if_imported()
        {
            WithLotsOfFreeDiskSpace();
            WithOldWrite();

            var file = Path.Combine(TempFolder, "test.avi");

            WithValidSeries();
            WithImportedFile(file);

            
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(file);

            
            Mocker.GetMock<IMoveEpisodeFiles>().Verify(s => s.MoveEpisodeFile(It.IsAny<EpisodeFile>(), true), Times.Once());
            ExceptionVerification.IgnoreWarns();
        }
        
        [Test]
        public void should_logError_and_return_if_size_exceeds_free_space()
        {
            var downloadName = @"C:\Test\Drop\30.Rock.S01E01.Pilot.mkv";

            var series = Builder<Series>.CreateNew()
                    .With(s => s.Title = "30 Rock")
                    .With(s => s.RootFolder = new LazyLoaded<RootFolder>(new RootFolder { Path = @"C:\Test\TV" }))
                    .With(s => s.FolderName = "30 Rock")
                    .Build();

            Mocker.GetMock<ISeriesRepository>()
                .Setup(c => c.FindByTitle("rock"))
                .Returns(series);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.GetSize(downloadName))
                    .Returns(10);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.FreeDiskSpace(series.Path))
                    .Returns(9);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.FolderExists(series.Path))
                    .Returns(true);

            
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(downloadName);


            
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(series, downloadName), Times.Never());
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_process_if_free_disk_space_exceeds_size()
        {
            WithLotsOfFreeDiskSpace();
            WithValidSeries();

            var downloadName = @"C:\Test\Drop\30.Rock.S01E01.Pilot.mkv";

            Mocker.GetMock<ISeriesRepository>()
                .Setup(c => c.FindByTitle("rock"))
                .Returns(fakeSeries);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.GetSize(downloadName))
                    .Returns(8);

            
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(downloadName);


            
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(fakeSeries, downloadName), Times.Once());
        }

        [Test]
        public void should_process_if_free_disk_space_equals_size()
        {
            var downloadName = @"C:\Test\Drop\30.Rock.S01E01.Pilot.mkv";

            WithValidSeries();

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.GetDirectorySize(downloadName))
                    .Returns(10);

            Mocker.GetMock<DiskProvider>()
                    .Setup(s => s.FreeDiskSpace(It.IsAny<string>()))
                    .Returns(10);

            
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(downloadName);


            
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(fakeSeries, downloadName), Times.Once());
        }

        [Test]
        public void should_return_if_series_Path_doesnt_exist()
        {
            var downloadName = @"C:\Test\Drop\30.Rock.S01E01.Pilot.mkv";

            WithValidSeries();

            Mocker.GetMock<DiskProvider>()
                                .Setup(s => s.FolderExists(fakeSeries.Path))
                                .Returns(false);

            
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(downloadName);


            
            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_skip_if_file_is_in_use_by_another_process()
        {
            var downloadName = @"C:\Test\Drop\30.Rock.S01E01.Pilot.mkv";

            WithValidSeries();

            Mocker.GetMock<DiskProvider>()
                                .Setup(s => s.IsFileLocked(It.Is<FileInfo>(f => f.FullName == downloadName)))
                                .Returns(true);

            
            Mocker.Resolve<DropFolderImportService>().ProcessVideoFile(downloadName);


            
            Mocker.GetMock<DiskScanProvider>().Verify(c => c.ImportFile(fakeSeries, downloadName), Times.Never());
        }
    }
}