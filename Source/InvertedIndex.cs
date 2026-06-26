namespace InvertedIndexProgram;
/// <summary>
/// Builds an inverted index based off of a list of paths to documents.
/// Supports stemming, stop-word removal ,symbol and number removal.
/// Supports prefix-based search operators (+, -).
/// </summary>
public class InvertedIndex
{
    private readonly Dictionary<string, HashSet<string>> _invertedIndexDic;
    public Dictionary<string, HashSet<string>> InvertedIndexDic => _invertedIndexDic;

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
            List<string> terms = textProcessor.ExtractTerms(content);

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
    /// <summary>
    /// Searches through the index depending on a query bundle.
    /// </summary>
    /// <param name="queryBundle">
    /// A list containing three categorized term lists:
    /// <list type="bullet">
    /// <item><description>[0] Must-have terms — the word must be present in the document.</description></item>
    /// <item><description>[1] At-least-one terms — at least one of these words must be present.</description></item>
    /// <item><description>[2] Must-not-have terms — documents containing any of these are excluded.</description></item>
    /// </list>
    /// </param>
    /// <returns>The Resulted document names seperated by commas.</returns>
    public List<string> GetSearchResult(QueryBundle queryBundle)
    {
        var mustHaveDocs = IntersectTermDocs(queryBundle.MustHave);
        var atLeastOneDocs = UnionTermDocs(queryBundle.AtLeastOne);
        var mustNotHaveDocs = UnionTermDocs(queryBundle.MustNotHave);

        var result = BuildResult(mustHaveDocs, atLeastOneDocs, mustNotHaveDocs);
 
        return result.OrderBy(v => v).ToList();
    }
    private List<string> IntersectTermDocs(List<string> terms)
    {
        var result = new List<string>();

        bool first = true;
        foreach (var term in terms)
        {
            if (_invertedIndexDic.TryGetValue(term, out var documents))
            {
                if (first)
                {
                    result = new List<string>(documents);
                    first = false;
                }
                else
                {
                    result = result.Intersect(documents).ToList();
                }
            }
            else
            {
                return new List<string>();
            }
        }
        return result;
    }
    private List<string> UnionTermDocs(List<string> terms)
    {
        var result = new List<string>();
        foreach (var term in terms)
        {
            if (_invertedIndexDic.TryGetValue(term, out var documents))
            {
                result.AddRange(documents);
            }
        }
        return result.Distinct().ToList();
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