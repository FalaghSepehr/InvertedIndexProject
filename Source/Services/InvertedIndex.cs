namespace InvertedIndexProgram;

public class InvertedIndex
{
    private readonly Dictionary<string, HashSet<string>> _invertedIndexDic;
    public IReadOnlyDictionary<string, HashSet<string>> InvertedIndexDic => _invertedIndexDic;

    private InvertedIndex(Dictionary<string, HashSet<string>> invertedIndexDic)
    {
        _invertedIndexDic = invertedIndexDic;
    }
    public static InvertedIndex Build(string[] docPaths, ITextProcessor textProcessor)
    {
        var invertedIndexDic = new Dictionary<string, HashSet<string>>();

        foreach (string docFileDir in docPaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(docFileDir);
            var content = File.ReadAllText(docFileDir).ToLower().Trim();
            var terms = textProcessor.ExtractTerms(content);

            foreach (string term in terms)
            {
                if (!invertedIndexDic.TryGetValue(term, out var documents))
                {
                    documents = new HashSet<string>();
                    invertedIndexDic[term] = documents;
                }
                documents.Add(fileName);
            }
        }
        return new InvertedIndex(invertedIndexDic);
    }
    public List<string> GetSearchResult(QueryBundle queryBundle)
    {
        var mustHaveDocs = IntersectTermDocs(queryBundle.MustHave);
        var atLeastOneDocs = UnionTermDocs(queryBundle.AtLeastOne);
        var mustNotHaveDocs = UnionTermDocs(queryBundle.MustNotHave);

        var result = BuildResult(mustHaveDocs, atLeastOneDocs, mustNotHaveDocs);
 
        return result.OrderBy(v => v).ToList();
    }
    public void ExportTo(IOutputWriter writer)
    {
        foreach (var pair in InvertedIndexDic)
        {
            writer.WriteLine(FormatEntry(pair));
        }
    }

    private string FormatEntry(KeyValuePair<string, HashSet<string>> pair)
    {
        return $"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value.OrderBy(v => v))}";
    }

    private List<string> IntersectTermDocs(List<string> terms)
    {
        if (terms.Count == 0)
        {
            return new List<string>();
        }

        if (!_invertedIndexDic.TryGetValue(terms[0], out var firstDocs))
        {
            return new List<string>();
        }

        var resultSet = new HashSet<string>(firstDocs);

        for (int i = 1; i < terms.Count; i++)
        {
            if (!_invertedIndexDic.TryGetValue(terms[i], out var docs))
            {
                return new List<string>();
            }
            resultSet.IntersectWith(docs);
        }

        return resultSet.ToList();
    }
    private List<string> UnionTermDocs(List<string> terms)
    {
        var resultSet = new HashSet<string>();
        foreach (var term in terms)
        {
            if (_invertedIndexDic.TryGetValue(term, out var documents))
            {
                resultSet.UnionWith(documents);
            }
        }
        return resultSet.ToList();
    }
    private List<string> BuildResult(List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs)
    {
        var allDocs = _invertedIndexDic.Values.SelectMany(d => d).Distinct();

        List<string> positiveDocs;
        if (mustHaveDocs.Count > 0)
        {
            positiveDocs = mustHaveDocs.Intersect(atLeastOneDocs.Count > 0 ? atLeastOneDocs : allDocs).ToList();
        }
        else if (atLeastOneDocs.Count > 0)
        {
            positiveDocs = atLeastOneDocs;
        }
        else
        {
            positiveDocs = allDocs.ToList();
        }
        return positiveDocs.Except(mustNotHaveDocs).ToList();
    }
}