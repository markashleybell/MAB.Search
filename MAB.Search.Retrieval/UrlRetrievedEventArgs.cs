using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Retrieval
{
    public class UrlRetrievedEventArgs : EventArgs
    {
        private string _url;
        private int _urlsSpidered;
        
        public UrlRetrievedEventArgs(string url)
        {
            _url = url;
        }

        public UrlRetrievedEventArgs(string url, int urlsSpidered)
        {
            _url = url;
            _urlsSpidered = urlsSpidered;
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public int UrlsSpidered
        {
            get { return _urlsSpidered; }
            set { _urlsSpidered = value; }
        }
    }
}
