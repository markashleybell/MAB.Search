using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Spider
{
    public interface IContentProcessor
    {
        string Cleanse(string content);
        List<string> Tokenise(string content);
    }
}
