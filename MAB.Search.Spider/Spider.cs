using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq.Expressions;

namespace MAB.Search.Spider
{
    public class Spider : ISpider
    {
        private List<string> _hosts;
        private int _limit = 20;
        private Dictionary<string, Uri> _retrieved;
        private Uri _baseUri;
        private IContentProcessor _contentProcessor;

        public event EventHandler<UrlRetrievedEventArgs> OnUrlRetrieved;

        public Spider(IContentProcessor contentProcessor)
        {
            _hosts = new List<string>();
            _retrieved = new Dictionary<string, Uri>();
            _contentProcessor = contentProcessor;
        }

        public Spider(IContentProcessor contentProcessor, List<string> hosts)
        {
            _hosts = hosts;
            _retrieved = new Dictionary<string, Uri>();
            _contentProcessor = contentProcessor;
        }

        public List<string> Hosts
        {
            get { return _hosts; }
            set { _hosts = value; }
        }

        public void Begin(string url)
        {
            _baseUri = new Uri(url);
            RetrieveAndProcessUrl(_baseUri);
        }

        private void RetrieveAndProcessUrl(Uri uri)
        {
            using(var client = new WebClient())
            {
                if(!_retrieved.ContainsKey(uri.ToString()))
                {
                    var content = client.DownloadString(uri.ToString());

                    // Process the content here
                    var wordList = _contentProcessor.Tokenise(content);

                    _retrieved.Add(uri.ToString(), uri);

                    var wordCounts = wordList.GroupBy(w => w)
                                             .Select(x => new { Word = x.Key, Count = x.Count() })
                                             .ToDictionary(x => x.Word, x => x.Count);

                    if(OnUrlRetrieved != null)
                        OnUrlRetrieved(this, new UrlRetrievedEventArgs(uri.ToString(), wordCounts));

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
