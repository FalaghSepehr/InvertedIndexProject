namespace InvertedIndexProgram;
/// <summary>
/// Builds and queries an inverted index from text documents.
/// Accepts an <see cref="ITextProcessor"/> for term extraction, enabling different text processing strategies.
/// Supports prefix-based search operators (+, -) for must-have, at-least-one, and must-not-have terms.
/// </summary>
public class InvertedIndex : ISearchService
{
    private readonly Dictionary<string, HashSet<string>> _invertedIndexDic;
    public IReadOnlyDictionary<string, HashSet<string>> InvertedIndexDic => _invertedIndexDic;

    private InvertedIndex(Dictionary<string, HashSet<string>> invertedIndexDic)
    {
        _invertedIndexDic = invertedIndexDic;
    }
    /// <summary>
    /// Builds an inverted index from the specified text documents using the given text processor.
    /// </summary>
    /// <param name="docPaths">Array of file paths to text documents.</param>
    /// <param name="textProcessor">The text processor for tokenizing and normalizing terms.</param>
    /// <returns>A fully built InvertedIndex ready for searching.</returns>
    public static InvertedIndex Build(string[] docPaths, ITextProcessor textProcessor)
    {
        var invertedIndexDic = new Dictionary<string, HashSet<string>>();

        foreach (string docFileDir in docPaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(docFileDir);
            var content = File.ReadAllText(docFileDir);
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
    public List<string> Search(QueryBundle queryBundle)
    {
        var hasMustHaveTerms = queryBundle.MustHave.Count > 0;
        var mustHaveDocs = IntersectTermDocs(queryBundle.MustHave);
        var atLeastOneDocs = UnionTermDocs(queryBundle.AtLeastOne);
        var mustNotHaveDocs = UnionTermDocs(queryBundle.MustNotHave);

        var result = BuildResult(hasMustHaveTerms, mustHaveDocs, atLeastOneDocs, mustNotHaveDocs);
 
        return result.OrderBy(v => v).ToList();
    }
    /// <summary>
    /// Exports the entire index to the specified output writer for debugging or external use.
    /// </summary>
    /// <param name="writer">The output writer to receive the formatted index.</param>
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
    private List<string> BuildResult(bool hasMustHaveTerms, List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs)
    {
        var allDocs = _invertedIndexDic.Values.SelectMany(d => d).Distinct();

        List<string> positiveDocs;
        if (hasMustHaveTerms)
        {
            positiveDocs = mustHaveDocs.Count == 0 ? [] : mustHaveDocs.Intersect(atLeastOneDocs.Count > 0 ? atLeastOneDocs : allDocs).ToList();
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