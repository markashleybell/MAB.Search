using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MAB.Search.Index;
using System.IO;
using System.Reflection;

namespace MAB.Search.Spider.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopWords = new List<string> { "cannot", "into", "our", "thus", "about", "ours", "above", "could", "ourselves", "together", "across", "down", "its", "out", "too", "after", "during", "itself", "over", "toward", "afterwards", "each", "last", "own", "towards", "again", "latter", "per", "under", "against", "either", "latterly", "perhaps", "until", "all", "else", "least", "rather", "almost", "elsewhere", "less", "same", "upon", "alone", "enough", "ltd", "seem", "along", "etc", "many", "seemed", "very", "already", "even", "may", "seeming", "via", "also", "ever", "seems", "was", "although", "every", "meanwhile", "several", "always", "everyone", "might", "she", "well", "among", "everything", "more", "should", "were", "amongst", "everywhere", "moreover", "since", "what", "except", "most", "whatever", "and", "few", "mostly", "some", "when", "another", "first", "much", "somehow", "whence", "any", "for", "must", "someone", "whenever", "anyhow", "former", "something", "where", "anyone", "formerly", "myself", "sometime", "whereafter", "anything", "from", "namely", "sometimes", "whereas", "anywhere", "further", "neither", "somewhere", "whereby", "are", "had", "never", "still", "wherein", "around", "has", "nevertheless", "such", "whereupon", "have", "next", "than", "wherever", "that", "whether", "hence", "nobody", "the", "whither", "became", "her", "none", "their", "which", "because", "here", "noone", "them", "while", "become", "hereafter", "nor", "themselves", "who", "becomes", "hereby", "not", "then", "whoever", "becoming", "herein", "nothing", "thence", "whole", "been", "hereupon", "now", "there", "whom", "before", "hers", "nowhere", "thereafter", "whose", "beforehand", "herself", "thereby", "why", "behind", "him", "off", "therefore", "will", "being", "himself", "often", "therein", "with", "below", "his", "thereupon", "within", "beside", "how", "once", "these", "without", "besides", "however", "one", "they", "would", "between", "only", "this", "yet", "beyond", "onto", "those", "you", "both", "though", "your", "but", "other", "through", "yours", "inc", "others", "throughout", "yourself", "can", "indeed", "otherwise", "thru", "yourselves" };

            IContentProcessor contentProcessor = new ContentProcessor(stopWords);

            ISearchIndex index = new SearchIndex(contentProcessor);

            var indexFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.bin";

            if (!File.Exists(indexFile))
            {
                var urls = new List<string> { 
                    "http://en.wikipedia.org/wiki/Battle_of_Bosworth_Field",
                    "http://en.wikipedia.org/wiki/Plymouth",
                    "http://en.wikipedia.org/wiki/Tamar_Bridge"
                };

                ISpider spider = new Spider(index, urls);

                spider.OnUrlRetrieved += OnUrlRetrieved;

                spider.Begin();

                index.Update();
            }

            while (true) 
            {
                Console.WriteLine("Query:"); // Prompt
                string q = Console.ReadLine(); // Get string from user
                if (q == "quit") // Check string
                    break;

                var results = index.Query(q);

                if (results == null)
                {
                    Console.WriteLine("No results");
                }
                else
                {
                    foreach (var result in results)
                    {
                        Console.WriteLine(result.Url + " (Relevance " + result.Relevance + ")");
                    }
                }
            }
        }

        private static void OnUrlRetrieved(object sender, UrlRetrievedEventArgs e)
        {
            Console.WriteLine(e.Url);
        }
    }
}
