using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers.DownloadClients
{
    public class SabProvider : IDownloadClient
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;

        [Inject]
        public SabProvider(ConfigProvider configProvider, HttpProvider httpProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
        }


        public SabProvider()
        {
        }

        private static string GetNzbName(string urlString)
        {
            var url = new Uri(urlString);
            if (url.Host.ToLower().Contains("newzbin"))
            {
                var postId = Regex.Match(urlString, @"\d{5,10}").Value;
                return postId;
            }

            return urlString.Replace("&", "%26");
        }

        public virtual bool IsInQueue(EpisodeParseResult newParseResult)
        {
            var queue = GetQueue().Where(c => c.ParseResult != null);

            var matchigTitle = queue.Where(q => String.Equals(q.ParseResult.CleanTitle, newParseResult.Series.CleanTitle, StringComparison.InvariantCultureIgnoreCase));

            var matchingTitleWithQuality = matchigTitle.Where(q => q.ParseResult.Quality >= newParseResult.Quality);


            if (newParseResult.Series.IsDaily)
            {
                return matchingTitleWithQuality.Any(q => q.ParseResult.AirDate.Value.Date == newParseResult.AirDate.Value.Date);
            }

            var matchingSeason = matchingTitleWithQuality.Where(q => q.ParseResult.SeasonNumber == newParseResult.SeasonNumber);

            if (newParseResult.FullSeason)
            {
                return matchingSeason.Any();
            }

            return matchingSeason.Any(q => q.ParseResult.EpisodeNumbers != null && q.ParseResult.EpisodeNumbers.Any(e => newParseResult.EpisodeNumbers.Contains(e)));
        }

        public virtual bool DownloadNzb(string url, string title)
        {
            string cat = _configProvider.SabTvCategory;
            int priority = (int)_configProvider.SabTvPriority;;
            string nzbName = title;

            string action = string.Format("mode=addfile&priority={0}&pp=3&cat={1}", priority, cat);

            NetworkCredential credentials = null;
            if (new Uri(url).Host.ToLower().Contains("newzbin"))
                credentials = new NetworkCredential(_configProvider.NewzbinUsername, _configProvider.NewzbinPassword);

            string requestUrl = GetSabRequest(action);

            //Todo: Download NZB using WebRequest/WebResponse so we can handle downloading errors
            logger.Debug("Downloading NZB as Stream: {0}", url);
            var nzbStream = _httpProvider.DownloadStream(url, credentials);

            //Todo: Handle SABnzbd returning nzo_id (0.7.0+) so we can track it
            logger.Info("Adding report [{0}] to the queue.", title);
            var response = UploadNzb(requestUrl, nzbName, nzbStream).Replace("\n", String.Empty);
            logger.Debug("Queue Response: [{0}]", response);

            if (response == "ok")
                return true;

            logger.Warn("SAB returned unexpected response '{0}'", response);

            return false;
        }

        public virtual List<SabQueueItem> GetQueue(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=queue&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            return JsonConvert.DeserializeObject<SabQueue>(JObject.Parse(response).SelectToken("queue").ToString()).Items;
        }

        public virtual List<SabHistoryItem> GetHistory(int start = 0, int limit = 0)
        {
            string action = String.Format("mode=history&output=json&start={0}&limit={1}", start, limit);
            string request = GetSabRequest(action);
            string response = _httpProvider.DownloadString(request);

            CheckForError(response);

            var items = JsonConvert.DeserializeObject<SabHistory>(JObject.Parse(response).SelectToken("history").ToString()).Items;
            return items ?? new List<SabHistoryItem>();
        }

        public virtual SabCategoryModel GetCategories(string host = null, int port = 0, string apiKey = null, string username = null, string password = null)
        {
            //Get saved values if any of these are defaults
            if (host == null)
                host = _configProvider.SabHost;

            if (port == 0)
                port = _configProvider.SabPort;

            if (apiKey == null)
                apiKey = _configProvider.SabApiKey;

            if (username == null)
                username = _configProvider.SabUsername;

            if (password == null)
                password = _configProvider.SabPassword;

            const string action = "mode=get_cats&output=json";

            var command = string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 host, port, action, apiKey, username, password);

            var response = _httpProvider.DownloadString(command);

            if (String.IsNullOrWhiteSpace(response))
                return new SabCategoryModel { categories = new List<string>() };

            var categories = JsonConvert.DeserializeObject<SabCategoryModel>(response);

            return categories;
        }

        private string GetSabRequest(string action)
        {
            return string.Format(@"http://{0}:{1}/api?{2}&apikey={3}&ma_username={4}&ma_password={5}",
                                 _configProvider.SabHost,
                                 _configProvider.SabPort,
                                 action,
                                 _configProvider.SabApiKey,
                                 _configProvider.SabUsername,
                                 _configProvider.SabPassword);
        }

        private void CheckForError(string response)
        {
            var result = JsonConvert.DeserializeObject<SabJsonError>(response);

            if (result.Status != null && result.Status.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                throw new ApplicationException(result.Error);
        }

        public string UploadNzb(string url, string name, Stream nzbFile)
        {
            try
            {
                var sb = new StringBuilder();

                string boundary = string.Format("--------{0}", DateTime.Now.Ticks.ToString("x", CultureInfo.InvariantCulture));

                sb.AppendFormat("--{0}", boundary);
                sb.Append("\r\n");
                sb.AppendFormat("Content-Disposition: form-data; name=\"name\"; filename=\"{0}\"", name);
                sb.Append("\r\n");
                sb.AppendFormat("Content-Type: application/x-nzb");
                sb.Append("\r\n");
                sb.Append("\r\n");

                byte[] bufferHeader = Encoding.ASCII.GetBytes(sb.ToString());
                byte[] bufferFooter = Encoding.ASCII.GetBytes(string.Format("\r\n--{0}--\r\n", boundary));

                using (var requestStream = new MemoryStream())
                {
                    requestStream.Write(bufferHeader, 0, bufferHeader.Length);
                    nzbFile.CopyTo(requestStream);
                    requestStream.Write(bufferFooter, 0, bufferFooter.Length);

                    var header = string.Format("Content-Type: multipart/form-data; boundary={0}", boundary);
                    return _httpProvider.PostFile(url, header, requestStream.ToArray());
                }
            }
            catch(Exception ex)
            {
                logger.WarnException("Failed to send NZB to SABnzbd", ex);
            }

            return String.Empty;
        }
    }
}