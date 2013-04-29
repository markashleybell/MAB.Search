using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using MAB.Search.Indexing;
using MAB.Search.Retrieval;

namespace MAB.Search.TestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var stopWords = new List<string> { "cannot", "into", "our", "thus", "about", "ours", "above", "could", "ourselves", "together", "across", "down", "its", "out", "too", "after", "during", "itself", "over", "toward", "afterwards", "each", "last", "own", "towards", "again", "latter", "per", "under", "against", "either", "latterly", "perhaps", "until", "all", "else", "least", "rather", "almost", "elsewhere", "less", "same", "upon", "alone", "enough", "ltd", "seem", "along", "etc", "many", "seemed", "very", "already", "even", "may", "seeming", "via", "also", "ever", "seems", "was", "although", "every", "meanwhile", "several", "always", "everyone", "might", "she", "well", "among", "everything", "more", "should", "were", "amongst", "everywhere", "moreover", "since", "what", "except", "most", "whatever", "and", "few", "mostly", "some", "when", "another", "first", "much", "somehow", "whence", "any", "for", "must", "someone", "whenever", "anyhow", "former", "something", "where", "anyone", "formerly", "myself", "sometime", "whereafter", "anything", "from", "namely", "sometimes", "whereas", "anywhere", "further", "neither", "somewhere", "whereby", "are", "had", "never", "still", "wherein", "around", "has", "nevertheless", "such", "whereupon", "have", "next", "than", "wherever", "that", "whether", "hence", "nobody", "the", "whither", "became", "her", "none", "their", "which", "because", "here", "noone", "them", "while", "become", "hereafter", "nor", "themselves", "who", "becomes", "hereby", "not", "then", "whoever", "becoming", "herein", "nothing", "thence", "whole", "been", "hereupon", "now", "there", "whom", "before", "hers", "nowhere", "thereafter", "whose", "beforehand", "herself", "thereby", "why", "behind", "him", "off", "therefore", "will", "being", "himself", "often", "therein", "with", "below", "his", "thereupon", "within", "beside", "how", "once", "these", "without", "besides", "however", "one", "they", "would", "between", "only", "this", "yet", "beyond", "onto", "those", "you", "both", "though", "your", "but", "other", "through", "yours", "inc", "others", "throughout", "yourself", "can", "indeed", "otherwise", "thru", "yourselves" };

            IContentProcessor contentProcessor = new HtmlContentProcessor(stopWords);

            ISearchIndex index = new SearchIndex(contentProcessor);

            var indexFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\index.bin";

            //if (!File.Exists(indexFile))
            //{
                var segments = new List<Uri> { 
                    new Uri("http://en.wikipedia.org/wiki/Battle_of_Bosworth_Field"),
                    new Uri("http://en.wikipedia.org/wiki/Plymouth"),
                    new Uri("http://en.wikipedia.org/wiki/Tamar_Bridge"),
                    new Uri("http://en.wikipedia.org/wiki/Saltash"), 
                    new Uri("http://en.wikipedia.org/wiki/Plymouth,_Massachusetts"),
                    new Uri("http://en.wikipedia.org/wiki/Pilgrim_Fathers"),
                    new Uri("http://en.wikipedia.org/wiki/Francis_Drake"), 
                    new Uri("http://en.wikipedia.org/wiki/HMNB_Devonport"), 
                    new Uri("http://en.wikipedia.org/wiki/River_Tamar"), 
                    new Uri("http://en.wikipedia.org/wiki/Royal_Albert_Bridge"),
                    new Uri("http://en.wikipedia.org/wiki/Devonport,_Devon"),
                    new Uri("http://en.wikipedia.org/wiki/Royal_Albert_Bridge"),
                    new Uri("http://en.wikipedia.org/wiki/English_Civil_War"),
                    new Uri("http://en.wikipedia.org/wiki/River_Plym"),
                    new Uri("http://en.wikipedia.org/wiki/Plympton"),
                    new Uri("http://en.wikipedia.org/wiki/Royal_Albert_Bridge"),
                    new Uri("http://en.wikipedia.org/wiki/Plymouth_Colony"),
                    new Uri("http://en.wikipedia.org/wiki/Union_Street,_Plymouth"),
                    new Uri("http://en.wikipedia.org/wiki/Plymstock"),
                    new Uri("http://en.wikipedia.org/wiki/Dartmoor"),
                    new Uri("http://en.wikipedia.org/wiki/University_of_Plymouth")                    
                };

                ICrawler crawler = new Crawler(index);

                segments.ForEach(x => crawler.CrawlQueue.Enqueue(x));

                crawler.OnUrlRetrieved += OnUrlRetrieved;

                crawler.Begin();
            //}

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
