using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Index
{
    public interface IContentProcessor
    {
        string Cleanse(string content);
        List<string> Tokenise(string content);
    }
}
