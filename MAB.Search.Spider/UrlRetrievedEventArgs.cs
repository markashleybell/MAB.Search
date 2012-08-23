using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Spider
{
    public class UrlRetrievedEventArgs : EventArgs
    {
        private string _url;

        public UrlRetrievedEventArgs(string url)
        {
            _url = url;
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }
    }
}
