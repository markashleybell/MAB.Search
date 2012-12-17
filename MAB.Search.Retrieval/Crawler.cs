using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using MAB.Search.Indexing;

namespace MAB.Search.Retrieval
{
    public class Crawler : ICrawler
    {
        private List<string> _hosts;
        private int _limit = 20;
        private Queue<Uri> _crawlQueue;
        private Uri _baseUri;
        private ISearchIndex _index;

        public event EventHandler<UrlRetrievedEventArgs> OnUrlRetrieved;

        public Crawler(ISearchIndex index)
        {
            _hosts = new List<string>();
            _crawlQueue = new Queue<Uri>();
            _index = index;
        }

        public Crawler(ISearchIndex index, List<string> hosts)
        {
            _hosts = hosts;
            _crawlQueue = new Queue<Uri>();
            _index = index;
        }

        public Queue<Uri> CrawlQueue
        {
            get { return _crawlQueue; }
            set { _crawlQueue = value; }
        }

        public List<string> Hosts
        {
            get { return _hosts; }
            set { _hosts = value; }
        }

        public void Begin()
        {
            ProcessCrawlQueue();
        }

        private void ProcessCrawlQueue()
        {
            using(var client = new WebClient())
            {
                string content = null;

                if(_crawlQueue.Count > 0)
                {
                    var uri = _crawlQueue.Dequeue();

                    try
                    {
                        content = client.DownloadString(uri.ToString());
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error retrieving URL");
                    }

                    if(!string.IsNullOrWhiteSpace(content))
                    {
                        _index.AddDocument(new Document { 
                            Title = "TEST",
                            Url = uri.ToString(),
                            Content = content
                        });

                        if(OnUrlRetrieved != null)
                            OnUrlRetrieved(this, new UrlRetrievedEventArgs(uri.ToString(), _index.DocumentCount));

                        foreach(var url in MatchUrls(content, x => x.StartsWith("/") || x.StartsWith(uri.GetLeftPart(UriPartial.Authority))))
                        {
                            //Console.WriteLine(url);
                        }
                    }

                    ProcessCrawlQueue();
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
