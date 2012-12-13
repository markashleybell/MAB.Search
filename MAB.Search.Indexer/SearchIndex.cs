using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MAB.Search.Index
{
    public class SearchIndex : ISearchIndex
    {
        IContentProcessor _contentProcessor;
        private string _appPath;

        private const char FILE_SEPARATOR = ((char)28); 
        private const char GROUP_SEPARATOR = ((char)29); 
        private const char RECORD_SEPARATOR = ((char)30); 
        private const char UNIT_SEPARATOR = ((char)31);

        public int DocumentCount { get; private set; }

        public SearchIndex(IContentProcessor contentProcessor)
        {
            _contentProcessor = contentProcessor;
            _appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            DocumentCount = 0;
        }

        private Dictionary<string, Dictionary<string, List<int>>> LoadIndex()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var index = new Dictionary<string, Dictionary<string, List<int>>>();

            if (File.Exists(_appPath + "\\index.bin"))
            {
                var indexRaw = File.ReadAllText(_appPath + "\\index.bin");

                index = (from word in indexRaw.Split(GROUP_SEPARATOR) // Returns array with element for each word
                         let documents = word.Split(RECORD_SEPARATOR) // Returns array where element 0 is the word and the others are url/postings structures
                         select new {
                             k = documents[0], // Key is the word
                             v = (from posting in documents.Skip(1) // Skip the first element (which was the word)
                                  let p = posting.Split(' ') // Returns array where 0 is document URL and 1 is pipe-separated postings list
                                  select new {
                                      k1 = p[0], // Key is the document URL
                                      v1 = p[1].Split('|').Select(x => Convert.ToInt32(x)).ToList() // Split the postings into a List<int>
                                  }).ToDictionary(x => x.k1, x => x.v1)	// Return a Dictionary<string, List<int>>
                         }).ToDictionary(x => x.k, x => x.v); // Dictionary<string, Dictionary<string, List<int>>>
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            File.AppendAllText(_appPath + "\\timings.log", "Load Index: " + ts.ToString() + Environment.NewLine);

            return index;
        }

        private void SaveIndex(Dictionary<string, Dictionary<string, List<int>>> index)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // Convert these to strings for use with string.Join
            var gs = GROUP_SEPARATOR.ToString();
            var rs = RECORD_SEPARATOR.ToString();

            // Forgive me, this was tricky to indent readably...
            var output = string.Join(gs, (from word in index // For each word in the index
                                          select word.Key + rs + // Concatenate the word with a record separator char then the list of url/posting structures
                                          string.Join(rs, (from document in word.Value // For each url/posting structure in this word's value
                                                           select document.Key + " " + // Concatenate the document URL with a space, then a pipe-separated list of postings
                                                           string.Join("|", (from posting in document.Value select posting).ToArray()) // Join the postings for this doc with a pipe
                                                           ).ToArray())
                                          ).ToArray());

            // Index Format:
            // ship{RECORD_SEPARATOR}http://en.wikipedia.org/wiki/Saltash 480|494|670{RECORD_SEPARATOR}http://en.wikipedia.org/wiki/Plymouth,_Massachusetts 127|444|458|479|2510{GROUP_SEPARATOR}...

            File.WriteAllText(_appPath + "\\index.bin", output.ToString());

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            File.AppendAllText(_appPath + "\\timings.log", "Save Index: " + ts.ToString() + Environment.NewLine);
        }

        public void AddDocument(Document document)
        {
            var index = LoadIndex();

            Update(document, index);

            SaveIndex(index);

            DocumentCount ++;
        }

        public void AddDocuments(List<Document> documents)
        {
            var index = LoadIndex();

            // Loop through the documents
            foreach (var document in documents)
            {
                Update(document, index);
            }

            SaveIndex(index);

            DocumentCount += documents.Count;
        }

        private void Update(Document document, Dictionary<string, Dictionary<string, List<int>>> index)
        {
            var words = _contentProcessor.Tokenise(document.Content);

            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];

                // If the index doesn't already contain an entry 
                // for this word, add one
                if (!index.ContainsKey(word))
                    index.Add(word, new Dictionary<string, List<int>>());

                // If the entry for this word doesn't contain an entry 
                // for this document's URL, add one
                if (!index[word].ContainsKey(document.Url))
                    index[word].Add(document.Url, new List<int>());

                // Add the index of this word in the document to 
                // the list for this document/word
                index[word][document.Url].Add(i);
            }
            
        }

        public List<Result> Query(string query)
        {
            var index = LoadIndex();

            // Tidy up the query as much as possible
            var terms = Regex.Replace(query.Trim(), @" {2,}", " ").ToLower().Split(' ').ToList();

            var results = new Dictionary<string, decimal>();

            var postings = new Dictionary<string, Dictionary<string, List<int>>>();

            foreach (var term in terms)
            {
                // If the index doesn't contain this term, there are no matches
                if (!index.ContainsKey(term))
                    return null;

                // Else, add the term to our temporary postings list
                postings.Add(term, index[term]);

                // Add weight to the results depending on the number of 
                // occurrences of the terms in the document
                foreach (var doc in index[term])
                {
                    if (!results.ContainsKey(doc.Key))
                        results.Add(doc.Key, 0);

                    results[doc.Key] += doc.Value.Count;
                }
            }

            index = null;

            // If it was a single search term, we don't need to bother
            // checking for multiple and positional matches
            if (terms.Count > 1)
            {
                // Just get the documents (keys) from the postings list 
                // So docs will be a list of lists containing all the 
                // documents each term is found in
                var docs = (from term in postings
                            select term.Value.Keys).ToList();

                // Initialise the list with the list of docs which match the 
                // first term so we can produce an intersection with it on 
                // the first iteration of the loop
                var docsWhichContainAllTerms = new List<string>();
                docsWhichContainAllTerms.AddRange(docs.First());

                // Loop through the lists which match the remaining terms
                // Skip the first list
                foreach (var doc in docs.Skip(1))
                {
                    // Get the intersection of the docs which contain this term 
                    // with the docs which match all the previous terms
                    docsWhichContainAllTerms = docsWhichContainAllTerms.Intersect(doc).ToList();
                }

                foreach (var doc in docsWhichContainAllTerms)
                {
                    // Add weight to each document which contains 
                    // ALL of the search terms
                    if (!results.ContainsKey(doc))
                        results.Add(doc, 0);

                    results[doc] += 1;
                }

                var docsWhichMatch = new List<string>();

                // Loop through each document which contains all the terms
                foreach (var doc in docsWhichContainAllTerms)
                {
                    // Get a list of the term positions for this document
                    var termPositions = (from p in postings
                                         select p.Value[doc]).ToList();

                    var intersection = new List<int>();
                    intersection.AddRange(termPositions.First());

                    for (var i = 1; i < termPositions.Count; i++)
                    {
                        var subtracted = termPositions[i].Select(x => (x - i)).ToList();

                        intersection = intersection.Intersect(subtracted).ToList();
                    }

                    if (intersection.Count > 0)
                        docsWhichMatch.Add(doc);
                }

                foreach (var doc in docsWhichMatch)
                {
                    // Add 'weight' to each document which contains 
                    // the exact phrase searched for
                    if (!results.ContainsKey(doc))
                        results.Add(doc, 0);

                    results[doc] += 100;
                }
            }

            return results.Select(x => new Result { Url = x.Key, Relevance = x.Value })
                          .OrderByDescending(x => x.Relevance)
                          .ToList();
        }
    }
}
