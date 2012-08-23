using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Spider
{
    public class UrlRetrievedEventArgs : EventArgs
    {
        private string _url;
        private Dictionary<string, int> _wordCounts;
        
        public UrlRetrievedEventArgs(string url)
        {
            _url = url;
            _wordCounts = null;
        }

        public UrlRetrievedEventArgs(string url, Dictionary<string, int> wordCounts)
        {
            _url = url;
            _wordCounts = wordCounts;
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public Dictionary<string, int> WordCounts
        {
            get { return _wordCounts; }
            set { _wordCounts = value; }
        }
    }
}
