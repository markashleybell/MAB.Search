using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;

namespace MAB.Search.Index
{
    public class SearchIndex : ISearchIndex
    {
        Dictionary<string, Dictionary<string, List<int>>> _index;
        List<Document> _documents;
        IContentProcessor _contentProcessor;

        private string _appPath;

        public int DocumentCount { get; private set; }

        public SearchIndex(IContentProcessor contentProcessor)
        {
            _index = new Dictionary<string, Dictionary<string, List<int>>>();
            _documents = new List<Document>();
            _contentProcessor = contentProcessor;

            _appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            DocumentCount = 0;

            if (File.Exists(_appPath + "\\index.bin"))
            {
                Stream stream = File.Open(_appPath + "\\index.bin", FileMode.Open);
                BinaryFormatter bFormatter = new BinaryFormatter();
                _index = (Dictionary<string, Dictionary<string, List<int>>>)bFormatter.Deserialize(stream);
                stream.Close();

                // Need a wrapper for the index dictionary to store meta data (e.g document count) which 
                // is not trivial to work out from the index itself when loaded
            }
        }

        public void AddDocument(Document document)
        {
            _documents.Add(document);
            DocumentCount ++;
        }

        public void AddDocuments(List<Document> documents)
        {
            foreach (var doc in documents)
                _documents.Add(doc);

            DocumentCount += documents.Count;
        }

        public void Update()
        {
            // Loop through the documents
            foreach (var doc in _documents)
            {
                // Obviously this tokeniser would need to be a bit more  
                // comprehensive if we were dealing with real text...
                var words = _contentProcessor.Tokenise(doc.Content);

                for (var i = 0; i < words.Count; i++)
                {
                    var word = words[i];

                    // If the index doesn't already contain an entry 
                    // for this word, add one
                    if (!_index.ContainsKey(word))
                        _index.Add(word, new Dictionary<string, List<int>>());

                    // If the entry for this word doesn't contain an entry 
                    // for this document's URL, add one
                    if (!_index[word].ContainsKey(doc.Url))
                        _index[word].Add(doc.Url, new List<int>());

                    // Add the index of this word in the document to 
                    // the list for this document/word
                    _index[word][doc.Url].Add(i);
                }
            }

            // Serialize the index to a binary file
            Stream stream = File.Open(_appPath + "\\index.bin", FileMode.Create);
            BinaryFormatter bFormatter = new BinaryFormatter();
            bFormatter.Serialize(stream, _index);
            stream.Close();
        }

        public List<Result> Query(string query)
        {
            // TODO: we need to deal with multiple spaces, case sensitivity etc
            var terms = query.Split(' ').ToList();

            var results = new Dictionary<string, int>();

            var postings = new Dictionary<string, Dictionary<string, List<int>>>();

            foreach (var term in terms)
            {
                // If the index doesn't contain this term, there are no matches
                if (!_index.ContainsKey(term))
                    return null;

                // Else, add the term to our temporary postings list
                postings.Add(term, _index[term]);

                // Add weight to the results depending on the number of 
                // occurrences of the terms in the document
                foreach (var doc in _index[term])
                {
                    if (!results.ContainsKey(doc.Key))
                        results.Add(doc.Key, 0);

                    results[doc.Key] += doc.Value.Count;
                }
            }

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

                results[doc] += 2;
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

                results[doc] += 10;
            }

            return results.Select(x => new Result { Url = x.Key, Relevance = x.Value }).OrderByDescending(x => x.Relevance).ToList();
        }
    }
}
