﻿using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Download.Clients
{
    public class BlackholeProvider : IDownloadClient
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;
        private readonly Logger _logger;


        public BlackholeProvider(IConfigService configService, IHttpProvider httpProvider,
                                    DiskProvider diskProvider, Logger logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
            _logger = logger;
        }


        public bool IsInQueue(IndexerParseResult newParseResult)
        {
            throw new NotImplementedException();
        }

        public bool DownloadNzb(string url, string title, bool recentlyAired)
        {
            try
            {
                title = FileNameBuilder.CleanFilename(title);

                var filename = Path.Combine(_configService.BlackholeDirectory, title + ".nzb");

                if (_diskProvider.FileExists(filename))
                {
                    //Return true so a lesser quality is not returned.
                    _logger.Info("NZB already exists on disk: {0}", filename);
                    return true;
                }

                _logger.Trace("Downloading NZB from: {0} to: {1}", url, filename);
                _httpProvider.DownloadFile(url, filename);

                _logger.Trace("NZB Download succeeded, saved to: {0}", filename);
                return true;
            }
            catch (Exception ex)
            {
                _logger.WarnException("Failed to download NZB: " + url, ex);
                return false;
            }
        }

        public IEnumerable<QueueItem> GetQueue()
        {
            return new QueueItem[0];
        }
    }
}
