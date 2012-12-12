using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MAB.Search.Index
{
    public class SearchIndex : ISearchIndex
    {
        IContentProcessor _contentProcessor;

        private string _appPath;

        public int DocumentCount { get; private set; }

        public SearchIndex(IContentProcessor contentProcessor)
        {
            _contentProcessor = contentProcessor;

            _appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            DocumentCount = 0;
        }

        private Dictionary<string, Dictionary<string, List<int>>> LoadIndex()
        {
            var index = new Dictionary<string, Dictionary<string, List<int>>>();

            //if (File.Exists(_appPath + "\\index.bin.old"))
            //{
            //    using(var stream = File.Open(_appPath + "\\index.bin.old", FileMode.Open))
            //    {
            //        BinaryFormatter bFormatter = new BinaryFormatter();
            //        index = (Dictionary<string, Dictionary<string, List<int>>>)bFormatter.Deserialize(stream);
            //    }

            //    // Need a wrapper for the index dictionary to store meta data (e.g document count) which 
            //    // is not trivial to work out from the index itself when loaded
            //}

            //var docs = input.Split(']');

            if (File.Exists(_appPath + "\\index.bin"))
            {
                var indexRaw = File.ReadAllText(_appPath + "\\index.bin");

                index = (from word in indexRaw.Split('~') // Split the whole thing on tilde chars, this gives us an element for each word
	                         let documents = word.Split('^') // Split the current word element on ^, this gives us an array where the first
	                         select new {
				 	            k = documents[0],
					            v = (from posting in documents.Skip(1)
						             let p = posting.Split(' ')
						             select new {
							             k1 = p[0],
							             v1 = p[1].Split('|').Select(x => Convert.ToInt32(x)).ToList()
						             }).ToDictionary(x => x.k1, x => x.v1)	
				            }).ToDictionary(x => x.k, x => x.v);
            }

            return index;
        }

        private void SaveIndex(Dictionary<string, Dictionary<string, List<int>>> index)
        {
            // Serialize the index to a binary file
            //using(var stream = File.Open(_appPath + "\\index.bin", FileMode.Create))
            //{
            //    BinaryFormatter bFormatter = new BinaryFormatter();
            //    bFormatter.Serialize(stream, index);
            //}

            // var output = new StringBuilder();


            var output = string.Join("~", (from word in index
                                           select word.Key + "^" + 
                                                string.Join("^", (from document in word.Value
                                                                  select document.Key + " " +
                                                                        string.Join("|", (from posting in document.Value
                                                                                          select posting).ToArray()) 
                                                                  ).ToArray())
                                           ).ToArray());

            //foreach(var document in index)
            //{
            //    output.Append(document.Key + "[");

            //    foreach(var word in document.Value)
            //    {
            //        output.Append(word.Key + "{" + string.Join("|", word.Value.ToArray()) + "}");
            //    }

            //    output.Append("]");
            //}

            File.WriteAllText(_appPath + "\\index.bin", output.ToString());
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
