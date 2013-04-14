using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Providers
{
    public class DiskScanProvider
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly string[] MediaExtensions = new[] { ".mkv", ".avi", ".wmv", ".mp4", ".mpg", ".mpeg", ".xvid", ".flv", ".mov", ".rm", ".rmvb", ".divx", ".dvr-ms", ".ts", ".ogm", ".m4v", ".strm" };
        private readonly DiskProvider _diskProvider;
        private readonly ICleanGhostFiles _ghostFileCleaner;
        private readonly IMediaFileService _mediaFileService;
        private readonly RecycleBinProvider _recycleBinProvider;
        private readonly MediaInfoProvider _mediaInfoProvider;
        private readonly IMoveEpisodeFiles _moveEpisodeFiles;
        private readonly IEpisodeMappingService _episodeMappingService;

        public DiskScanProvider(DiskProvider diskProvider, ICleanGhostFiles ghostFileCleaner, IMediaFileService mediaFileService,
                                RecycleBinProvider recycleBinProvider, MediaInfoProvider mediaInfoProvider, IMoveEpisodeFiles moveEpisodeFiles,
            IEpisodeMappingService episodeMappingService)
        {
            _diskProvider = diskProvider;
            _ghostFileCleaner = ghostFileCleaner;
            _mediaFileService = mediaFileService;
            _recycleBinProvider = recycleBinProvider;
            _mediaInfoProvider = mediaInfoProvider;
            _moveEpisodeFiles = moveEpisodeFiles;
            _episodeMappingService = episodeMappingService;
        }

        public DiskScanProvider()
        {
        }

        public virtual List<EpisodeFile> Scan(Series series)
        {
            return Scan(series, series.Path);
        }

        public virtual List<EpisodeFile> Scan(Series series, string path)
        {
            if (!_diskProvider.FolderExists(path))
            {
                Logger.Warn("Series folder doesn't exist: {0}", path);
                return new List<EpisodeFile>();
            }


            _ghostFileCleaner.RemoveNonExistingFiles(series.Id);

            var mediaFileList = GetVideoFiles(path);
            var importedFiles = new List<EpisodeFile>();

            foreach (var filePath in mediaFileList)
            {
                var file = ImportFile(series, filePath);
                if (file != null)
                {
                    importedFiles.Add(file);
                }
            }

            //Todo: Find the "best" episode file for all found episodes and import that one
            //Todo: Move the episode linking to here, instead of import (or rename import)

            return importedFiles;
        }

        public virtual EpisodeFile ImportFile(Series series, string filePath)
        {
            Logger.Trace("Importing file to database [{0}]", filePath);

            if (_mediaFileService.Exists(filePath))
            {
                Logger.Trace("[{0}] already exists in the database. skipping.", filePath);
                return null;
            }

            var parseResult = _episodeMappingService.GetEpisodes(filePath);

            if (parseResult == null)
            {
                return null;
            }

            var size = _diskProvider.GetSize(filePath);
            var runTime = _mediaInfoProvider.GetRunTime(filePath);

            if (series.SeriesType == SeriesTypes.Daily || parseResult.ParsedEpisodeInfo.SeasonNumber > 0)
            {
                if (size < Constants.IgnoreFileSize && runTime < 180)
                {
                    Logger.Trace("[{0}] appears to be a sample. skipping.", filePath);
                    return null;
                }
            }

            if (!_diskProvider.IsChildOfPath(filePath, series.Path))
            {
                parseResult.ParsedEpisodeInfo.SceneSource = true;
            }


            //Make sure this file is an upgrade for ALL episodes already on disk
            if (parseResult.Episodes.All(e => e.EpisodeFile == null || e.EpisodeFile.Quality <= parseResult.ParsedEpisodeInfo.Quality))
            {
                Logger.Debug("Deleting the existing file(s) on disk to upgrade to: {0}", filePath);
                //Do the delete for files where there is already an episode on disk
                parseResult.Episodes.Where(e => e.EpisodeFile != null).Select(e => e.EpisodeFile.Path).Distinct().ToList().ForEach(p => _recycleBinProvider.DeleteFile(p));
            }

            else
            {
                //Skip this file because its not an upgrade
                Logger.Trace("This file isn't an upgrade for all episodes. Skipping {0}", filePath);
                return null;
            }

            var episodeFile = new EpisodeFile();
            episodeFile.DateAdded = DateTime.Now;
            episodeFile.SeriesId = series.Id;
            episodeFile.Path = filePath.NormalizePath();
            episodeFile.Size = size;
            episodeFile.Quality = parseResult.ParsedEpisodeInfo.Quality;
            episodeFile.SeasonNumber = parseResult.ParsedEpisodeInfo.SeasonNumber;
            episodeFile.SceneName = Path.GetFileNameWithoutExtension(filePath.NormalizePath());

            //Todo: We shouldn't actually import the file until we confirm its the only one we want.
            //Todo: Separate episodeFile creation from importing (pass file to import to import)
            _mediaFileService.Add(episodeFile);
            return episodeFile;
        }


        public virtual void CleanUpDropFolder(string path)
        {
            //Todo: We should rename files before importing them to prevent this issue from ever happening

            var filesOnDisk = GetVideoFiles(path);

            foreach (var file in filesOnDisk)
            {
                try
                {
                    var episodeFile = _mediaFileService.GetFileByPath(file);

                    if (episodeFile != null)
                    {
                        Logger.Trace("[{0}] was imported but not moved, moving it now", file);

                        _moveEpisodeFiles.MoveEpisodeFile(episodeFile, true);
                    }

                }
                catch (Exception ex)
                {
                    Logger.WarnException("Failed to move episode file from drop folder: " + file, ex);
                    throw;
                }
            }
        }

        public virtual List<string> GetVideoFiles(string path, bool allDirectories = true)
        {
            Logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption);

            var mediaFileList = filesOnDisk.Where(c => MediaExtensions.Contains(Path.GetExtension(c).ToLower())).ToList();

            Logger.Trace("{0} video files were found in {1}", mediaFileList.Count, path);
            return mediaFileList;
        }
    }
}