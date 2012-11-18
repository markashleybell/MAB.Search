using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Spider
{
    public interface ISpider
    {
        event EventHandler<UrlRetrievedEventArgs> OnUrlRetrieved;
        void Begin();
    }
}
