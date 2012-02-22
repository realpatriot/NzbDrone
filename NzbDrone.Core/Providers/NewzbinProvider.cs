using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NLog;

namespace NzbDrone.Core.Providers
{
    public class NewzbinProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public virtual Stream DownloadNzbStream(string username, string password, int id)
        {
            //Follows Newzbin's X-DNZB Spec here: https://docs.newzbin2.es/index.php/Newzbin_API:DirectNZB

            const string url = "https://www.newzbin2.es/api/dnzb";

            var postData = String.Format("username={0}&password={1}&reportid={2}", username, password, id);
            var bytes = new ASCIIEncoding().GetBytes(postData);

            var request = WebRequest.Create(url);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            logger.Trace("Writing postData to request stream.");
            var dataStream = request.GetRequestStream();
            dataStream.Write(bytes, 0, bytes.Length);
            dataStream.Close();

            var response = request.GetResponse();

            if (Int32.Parse(response.Headers["X-DNZB-RCode"]) != 200)
            {
                logger.Warn("Failed to download NZB. RCode: {0}, RText: {1}", response.Headers["X-DNZB-RCode"], response.Headers["X-DNZB-RText"]);
                return null;
            }

            return response.GetResponseStream();
        }

        public virtual void DownloadNzb(string username, string password, int id, string filename)
        {
            var stream = DownloadNzbStream(username, password, id);

            using (Stream file = File.OpenWrite(filename))
            {
                stream.CopyTo(file);
            }
        }
    }
}
