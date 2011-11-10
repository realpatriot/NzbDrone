﻿using System;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using Prowlin;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Prowl : ExternalNotificationBase
    {
        private readonly ProwlProvider _prowlProvider;

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Prowl(ConfigProvider configProvider, ProwlProvider prowlProvider)
            : base(configProvider)
        {
            _prowlProvider = prowlProvider;
        }

        public override string Name
        {
            get { return "Prowl"; }
        }

        public override void OnGrab(string message)
        {
            try
            {
                if(_configProvider.GrowlNotifyOnGrab)
                {
                    _logger.Trace("Sending Notification to Prowl");
                    const string title = "Episode Grabbed";

                    var apiKeys = _configProvider.ProwlApiKeys;
                    var priority = _configProvider.ProwlPriority;

                    _prowlProvider.SendNotification(title, message, apiKeys, (NotificationPriority)priority);
                }
            }

            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);
                throw;
            }
        }

        public override void OnDownload(string message, Series series)
        {
            try
            {
                if (_configProvider.GrowlNotifyOnDownload)
                {
                    _logger.Trace("Sending Notification to Prowl");
                    const string title = "Episode Downloaded";

                    var apiKeys = _configProvider.ProwlApiKeys;
                    var priority = _configProvider.ProwlPriority;

                    _prowlProvider.SendNotification(title, message, apiKeys, (NotificationPriority)priority);
                }
            }

            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);
                throw;
            }
        }

        public override void OnRename(string message, Series series)
        {
            
        }
    }
}