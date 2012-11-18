using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using MAB.Search.Index;

namespace MAB.Search.Spider
{
    public class Spider : ISpider
    {
        private List<string> _hosts;
        private int _limit = 20;
        private List<string> _urls;
        private Dictionary<string, Uri> _retrieved;
        private Uri _baseUri;
        private ISearchIndex _index;

        public event EventHandler<UrlRetrievedEventArgs> OnUrlRetrieved;

        public Spider(ISearchIndex index, List<string> urls)
        {
            _hosts = new List<string>();
            _urls = urls;
            _retrieved = new Dictionary<string, Uri>();
            _index = index;
        }

        public Spider(ISearchIndex index, List<string> urls, List<string> hosts)
        {
            _hosts = hosts;
            _urls = urls;
            _retrieved = new Dictionary<string, Uri>();
            _index = index;
        }

        public List<string> Urls
        {
            get { return _urls; }
            set { _urls = value; }
        }

        public List<string> Hosts
        {
            get { return _hosts; }
            set { _hosts = value; }
        }

        public void Begin()
        {
            foreach (string url in _urls)
            {
                _baseUri = new Uri(url);
                RetrieveAndProcessUrl(_baseUri);
            }
        }

        private void RetrieveAndProcessUrl(Uri uri)
        {
            using(var client = new WebClient())
            {
                if(!_retrieved.ContainsKey(uri.ToString()))
                {
                    var content = client.DownloadString(uri.ToString());

                    _index.AddDocument(new Document { 
                        Title = "TEST",
                        Url = uri.ToString(),
                        Content = content
                    });

                    _retrieved.Add(uri.ToString(), uri);

                    if(OnUrlRetrieved != null)
                        OnUrlRetrieved(this, new UrlRetrievedEventArgs(uri.ToString(), _index.DocumentCount));

                    foreach(var url in MatchUrls(content, x => x.StartsWith("/") || x.StartsWith(uri.GetLeftPart(UriPartial.Authority))))
                    {
                        //Console.WriteLine(url);
                    }
                }
            }
        }

        private List<string> MatchUrls(string content)
        {
            return MatchUrls(content, null);
        }

        private List<string> MatchUrls(string content, Func<string, bool> filter)
        {
            var urls = new List<string>();

            var rx = new Regex("href=[\"'](/{0,2}?.*?)[\"']", RegexOptions.Multiline);

            var matches = rx.Matches(content);

            foreach(Match match in matches)
                urls.Add(match.Groups[1].Value);

            return (filter != null) ? urls.Where(filter).ToList() : urls;
        }
    }
}
