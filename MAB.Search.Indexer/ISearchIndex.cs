using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.Search.Index
{
    public interface ISearchIndex
    {
        void AddDocument(Document document);
        void AddDocuments(List<Document> documents);
        void Update();
        int DocumentCount { get; }
        List<Result> Query(string query);
    }
}
