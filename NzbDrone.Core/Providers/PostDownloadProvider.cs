﻿using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Providers
{
    public class PostDownloadProvider
    {
        private readonly DiskProvider _diskProvider;
        private readonly DiskScanProvider _diskScanProvider;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IMoveEpisodeFiles _episodeFileMover;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public PostDownloadProvider(DiskProvider diskProvider, DiskScanProvider diskScanProvider, ISeriesRepository seriesRepository, IMoveEpisodeFiles episodeFileMover, IParsingService parsingService, Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanProvider = diskScanProvider;
            _seriesRepository = seriesRepository;
            _episodeFileMover = episodeFileMover;
            _parsingService = parsingService;
            _logger = logger;
        }

        public virtual void ProcessDropFolder(string dropFolder)
        {
            foreach (var subfolder in _diskProvider.GetDirectories(dropFolder))
            {
                try
                {
                    if (!_seriesRepository.SeriesPathExists(subfolder))
                    {
                        ProcessDownload(new DirectoryInfo(subfolder));
                    }
                }
                catch (Exception e)
                {
                    _logger.ErrorException("An error has occurred while importing folder: " + subfolder, e);
                }
            }

            foreach (var videoFile in _diskScanProvider.GetVideoFiles(dropFolder, false))
            {
                try
                {
                    var series = _parsingService.GetSeries(videoFile);
                    ProcessVideoFile(videoFile, series);
                }
                catch (Exception ex)
                {
                    _logger.ErrorException("An error has occurred while importing video file" + videoFile, ex);
                }
            }
        }

        private void ProcessDownload(DirectoryInfo subfolderInfo)
        {
            var series = _parsingService.GetSeries(subfolderInfo.Name);

            if (series == null)
            {
                _logger.Trace("Unknown Series {0}", subfolderInfo.Name);
                return;
            }

            var files = _diskScanProvider.GetVideoFiles(subfolderInfo.FullName);

            foreach (var file in files)
            {
                ProcessVideoFile(file, series);
            }
        }


        private void ProcessVideoFile(string videoFile, Series series)
        {
            if (_diskProvider.GetLastFileWrite(videoFile).AddMinutes(2) > DateTime.UtcNow)
            {
                _logger.Trace("[{0}] is too fresh. skipping", videoFile);
                return;
            }

            if (_diskProvider.IsFileLocked(new FileInfo(videoFile)))
            {
                _logger.Trace("[{0}] is currently locked by another process, skipping", videoFile);
                return;
            }

            _diskProvider.EnsureFolder(series.Path);

            var size = _diskProvider.GetSize(videoFile);
            var freeSpace = _diskProvider.GetAvilableSpace(series.Path);

            if (Convert.ToUInt64(size) > freeSpace)
            {
                _logger.Error("Not enough space to move episode to: {0}", series.Path);
                return;
            }

            var episodeFile = _diskScanProvider.ImportFile(series, videoFile);

            if (episodeFile != null)
            {
                _episodeFileMover.MoveEpisodeFile(episodeFile, true);
            }
        }

    }
}