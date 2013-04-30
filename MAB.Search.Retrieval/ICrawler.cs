using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Retrieval
{
    public interface ICrawler
    {
        event EventHandler<UrlRetrievedEventArgs> OnUrlRetrieved;
        event EventHandler<UrlQueuedEventArgs> OnUrlQueued;
        void Begin();
        Queue<Uri> CrawlQueue { get; set; }
    }
}
