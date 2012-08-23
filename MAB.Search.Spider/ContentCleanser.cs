using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MAB.Search.Spider
{
    public class ContentCleanser : IContentCleanser
    {
        public List<string> GetWords(string content)
        {
            var cleansed = Cleanse(content);

            var words = cleansed.Split(' ');

            return words.ToList();
        }

        private string Cleanse(string content)
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
            content = Regex.Replace(content, "\\s{2,}", " ", opts); // Replace multiple spaces with single spaces

            return content;
        }
    }
}
