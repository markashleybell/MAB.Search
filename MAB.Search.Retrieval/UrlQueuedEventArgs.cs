using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Retrieval
{
    public class UrlQueuedEventArgs : EventArgs
    {
        private string _url;
        private int _urlsQueued;
        
        public UrlQueuedEventArgs(string url)
        {
            _url = url;
        }

        public UrlQueuedEventArgs(string url, int urlsQueued)
        {
            _url = url;
            _urlsQueued = urlsQueued;
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public int UrlsQueued
        {
            get { return _urlsQueued; }
            set { _urlsQueued = value; }
        }
    }
}
