﻿using System;
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

        public event EventHandler<UrlRetrievedEventArgs> OnUrlRetrieved;
        
        public Spider()
        {
            _hosts = new List<string>();
            _retrieved = new Dictionary<string, Uri>();
        }

        public Spider(List<string> hosts)
        {
            _hosts = hosts;
            _retrieved = new Dictionary<string, Uri>();
        }

        public List<string> Hosts
        {
            get { return _hosts; }
            set { _hosts = value; }
        }

        public void Begin(string url)
        {
            RetrieveAndProcessUrl(new Uri(url));
        }

        private void RetrieveAndProcessUrl(Uri uri)
        {
            using(var client = new WebClient())
            {
                if(!_retrieved.ContainsKey(uri.ToString()))
                {
                    var content = client.DownloadString(uri.ToString());

                    _retrieved.Add(uri.ToString(), uri);

                    if(OnUrlRetrieved != null)
                        OnUrlRetrieved(this, new UrlRetrievedEventArgs(uri.ToString()));

                    foreach(var url in MatchUrls(content, x => x.StartsWith("/") || x.StartsWith(uri.GetLeftPart(UriPartial.Authority))))
                    {
                        Console.WriteLine(url);
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
