using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Indexing
{
    /// <summary>
    /// Defines a class responsible for cleaning and tokenising content
    /// </summary>
    public interface IContentProcessor
    {
        string Cleanse(string content);
        List<string> Tokenise(string content);
    }
}
