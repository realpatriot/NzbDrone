using System;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbGetQueueItem
    {
        private string _nzbName;

        public Int32 NzbId { get; set; }

        public string NzbName
        {
            get { return _nzbName; }
            set
            {
                _nzbName = value;
                ParseResult = Parser.Parser.ParseTitle<ParseResult>(value.Replace("DUPLICATE / ", String.Empty));
            }
        }

        public String Category { get; set; }
        public Int32 FileSizeMb { get; set; }
        public Int32 RemainingSizeMb { get; set; }

        public ParseResult ParseResult { private set; get; }
    }
}
