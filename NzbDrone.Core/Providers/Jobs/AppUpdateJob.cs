﻿using System.Diagnostics;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.Jobs
{
    public class AppUpdateJob : IJob
    {
        private readonly UpdateProvider _updateProvider;
        private readonly EnviromentProvider _enviromentProvider;
        private readonly DiskProvider _diskProvider;
        private readonly HttpProvider _httpProvider;
        private readonly ProcessProvider _processProvider;
        private readonly ArchiveProvider _archiveProvider;
        private readonly ConfigFileProvider _configFileProvider;

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public AppUpdateJob(UpdateProvider updateProvider, EnviromentProvider enviromentProvider, DiskProvider diskProvider,
            HttpProvider httpProvider, ProcessProvider processProvider, ArchiveProvider archiveProvider, ConfigFileProvider configFileProvider)
        {
            _updateProvider = updateProvider;
            _enviromentProvider = enviromentProvider;
            _diskProvider = diskProvider;
            _httpProvider = httpProvider;
            _processProvider = processProvider;
            _archiveProvider = archiveProvider;
            _configFileProvider = configFileProvider;
        }

        public string Name
        {
            get { return "Update Application Job"; }
        }

        public int DefaultInterval
        {
            get { return 0; }
        }

        public virtual void Start(ProgressNotification notification, int targetId, int secondaryTargetId)
        {
            notification.CurrentMessage = "Updating NzbDrone";

            var updatePackage = _updateProvider.GetAvilableUpdate();

            var packageDestination = Path.Combine(_enviromentProvider.GetUpdateSandboxFolder(), updatePackage.FileName);

            if (_diskProvider.FolderExists(_enviromentProvider.GetUpdateSandboxFolder()))
            {
                logger.Info("Deleting old update files");
                _diskProvider.DeleteFolder(_enviromentProvider.GetUpdateSandboxFolder(), true);
            }

            logger.Info("Downloading update package from [{0}] to [{1}]", updatePackage.Url, packageDestination);
            notification.CurrentMessage = "Downloading Update " + updatePackage.Version;
            _httpProvider.DownloadFile(updatePackage.Url, packageDestination);
            logger.Info("Download completed for update package from [{0}]", updatePackage.FileName);

            logger.Info("Extracting Update package");
            notification.CurrentMessage = "Extracting Update";
            _archiveProvider.ExtractArchive(packageDestination, _enviromentProvider.GetUpdateSandboxFolder());
            logger.Info("Update package extracted successfully");

            logger.Info("Preparing client");
            notification.CurrentMessage = "Preparing to start Update";
            _diskProvider.CopyDirectory(_enviromentProvider.GetUpdateClientFolder(), _enviromentProvider.GetUpdateSandboxFolder());


            logger.Info("Starting update client");
            var startInfo = new ProcessStartInfo()
            {
                FileName = _enviromentProvider.GetUpdateClientExePath(),
                Arguments = string.Format("{0} {1}", _enviromentProvider.NzbDroneProcessIdFromEnviroment, _configFileProvider.Guid)
            };

            var updateProcess = _processProvider.Start(startInfo);
            notification.CurrentMessage = "Update in progress. NzbDrone will restart shortly.";
        }
    }
}