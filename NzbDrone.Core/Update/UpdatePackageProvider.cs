﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        IEnumerable<UpdatePackage> GetAvailablePackages();
        UpdatePackage GetLatestUpdate();
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        private readonly IConfigService _configService;
        private readonly IHttpProvider _httpProvider;
        private readonly Logger _logger;

        private static readonly Regex ParseRegex = new Regex(@"(?:\>)(?<filename>NzbDrone.+?(?<version>\d+\.\d+\.\d+\.\d+).+?)(?:\<\/A\>)", RegexOptions.IgnoreCase);

        public UpdatePackageProvider(IConfigService configService, IHttpProvider httpProvider, Logger logger)
        {
            _configService = configService;
            _httpProvider = httpProvider;
            _logger = logger;
        }

        public IEnumerable<UpdatePackage> GetAvailablePackages()
        {
            var updateList = new List<UpdatePackage>();
            var updateUrl = _configService.UpdateUrl;

            _logger.Debug("Getting a list of updates from {0}", updateUrl);

            var rawUpdateList = _httpProvider.DownloadString(updateUrl);
            var matches = ParseRegex.Matches(rawUpdateList);

            foreach (Match match in matches)
            {
                var updatePackage = new UpdatePackage();
                updatePackage.FileName = match.Groups["filename"].Value;
                updatePackage.Url = updateUrl + updatePackage.FileName;
                updatePackage.Version = new Version(match.Groups["version"].Value);
                updateList.Add(updatePackage);
            }

            _logger.Debug("Found {0} update packages", updateUrl.Length);

            return updateList;
        }

        public UpdatePackage GetLatestUpdate()
        {
            return GetAvailablePackages().OrderByDescending(c => c.Version).FirstOrDefault();
        }
    }
}