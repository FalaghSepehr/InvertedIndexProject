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
            List<string> terms = textProcessor.FilterTerms(content.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).ToList());

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
    public string GetSearchResult(List<List<string>> queryBundle)
    {
        var mustHaveTerms = queryBundle[0];
        var atLeastOneTerms = queryBundle[1];
        var mustNotHaveTerms = queryBundle[2];

        var mustHaveDocs = IntersectTermDocs(mustHaveTerms, _invertedIndexDic);
        var atLeastOneDocs = UnionTermDocs(atLeastOneTerms, _invertedIndexDic);
        var mustNotHaveDocs = UnionTermDocs(mustNotHaveTerms, _invertedIndexDic);

        var result = BuildResult(mustNotHaveTerms, mustHaveDocs, atLeastOneDocs, mustNotHaveDocs, _invertedIndexDic);
        if (result.Count == 0)
        {
            return "No results!";
        }
        return string.Join(", ", result.OrderBy(v => v));
    }
    private static List<string> IntersectTermDocs(List<string> terms, Dictionary<string, HashSet<string>> invertedIndex)
    {
        var result = new List<string>();

        bool first = true;
        foreach (var term in terms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
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
    private static List<string> UnionTermDocs(List<string> terms, Dictionary<string, HashSet<string>> invertedIndex)
    {
        var result = new List<string>();
        foreach (var term in terms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
                result.AddRange(documents);
        }
        return result.Distinct().ToList();
    }
    private static List<string> BuildResult(List<string> mustNotHaveTerms, List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs, Dictionary<string, HashSet<string>> invertedIndex)
    {
        // Combines must-have (intersection) and at-least-one (union) results,
        // then excludes must-not-have documents. Special cases:
        // - No matches found for any required terms → no results (unless only exclusion terms exist)
        // - Only exclusion terms specified → start with all documents then exclude

        var result = new List<string>();
        if (mustHaveDocs.Count == 0 && atLeastOneDocs.Count == 0 && mustNotHaveDocs.Count == 0)
        {
            if (mustNotHaveTerms.Count != 0)
            {
                result = invertedIndex.Values.SelectMany(list => list).Distinct().ToList();
            }
        }
        else if (mustHaveDocs.Count == 0 && atLeastOneDocs.Count == 0 && mustNotHaveDocs.Count != 0)
        {
            result = invertedIndex.Values.SelectMany(list => list).Distinct().Except(mustNotHaveDocs).ToList();
        }
        else if (mustHaveDocs.Count != 0 && atLeastOneDocs.Count != 0)
        {
            result = mustHaveDocs.Intersect(atLeastOneDocs).Except(mustNotHaveDocs).ToList();
        }
        else if (mustHaveDocs.Count == 0 || atLeastOneDocs.Count == 0)
        {
            result = mustHaveDocs.Union(atLeastOneDocs).Except(mustNotHaveDocs).ToList();
        }
        return result;
    }
}