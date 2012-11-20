using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MAB.Search.Index
{
    public class ContentProcessor : IContentProcessor
    {
        private List<string> _stopWords;
        private Regex _stopWordsRegex;

        public ContentProcessor()
        {
            // Leave member variables null
        }

        public ContentProcessor(List<string> stopWords)
        {
            _stopWords = stopWords;
            // We have a list of separate stop words, so compile them all into a regular expression for later use
            _stopWordsRegex = new Regex("\\b(" + string.Join("|", _stopWords) + ")\\b", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        public string Cleanse(string content)
        {
            var opts = RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Multiline;

            content = Regex.Replace(content, "[\\r\\n]+", " ", opts); // Remove all line breaks
            content = Regex.Replace(content, "<script[^>]*>(.*?)</script>", " ", opts); // Remove inline script and script tags

            // Should we be preserving links when replacing tags and non-alpha chars? Depends on the algorithm
            content = Regex.Replace(content, "<\\!?/?[a-z][a-z0-9]*[^<>]*>", " ", opts); // Remove all other HTML tags
            content = Regex.Replace(content, "<!--.*?-->", " ", opts); // Remove HTML comments
            content = Regex.Replace(content, "&[a-z0-9]+;", " ", opts); // Remove HTML entities
            content = Regex.Replace(content, "[^a-z\\s]", "", opts); // Remove any non-alphanumeric characters

            content = Regex.Replace(content, "\\b[a-z0-9]{0,2}\\b", " ", opts); // Remove all words less than three letters long

            // If we have a list of stop words, remove them all here
            if (_stopWords != null)
                content = _stopWordsRegex.Replace(content, " ");

            content = Regex.Replace(content, "\\s{2,}", " ", opts); // Replace multiple spaces with single spaces

            return content;
        }

        public List<string> Tokenise(string content)
        {
            return Cleanse(content).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                   // We only cater for English at the moment so it's unlikely anyone is going to search for a 26-letter word
                                   .Where(s => s.Length < 26) 
                                   // Lowercase everything at this point
                                   .Select(s => s.ToLower())
                                   .ToList();
        }
    }
}
