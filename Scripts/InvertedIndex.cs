public class InvertedIndex
{
    public Dictionary<string, List<string>> indexDic { get; set; } = new();
    public InvertedIndex(string[] fileDirectories)
    {
        foreach (string docFileDir in fileDirectories)
        {
            string fileName = Path.GetFileNameWithoutExtension(docFileDir);
            string content = File.ReadAllText(docFileDir).ToLower().Trim();
            List<string> terms = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().FilterTerms();

            foreach (string term in terms)
            {
                if (!indexDic.ContainsKey(term))
                {
                    indexDic[term] = new List<string>();
                }
                if (!indexDic[term].Contains(fileName))
                {
                    indexDic[term].Add(fileName);
                }
            }
        }
        foreach (var term in indexDic.Keys)
        {
            indexDic[term].Sort();
        }
    }
    public string Search(List<List<string>> queryBundle)
    {
        return GetSearchResult(queryBundle, indexDic);
    }
    private static string GetSearchResult(List<List<string>> queryBundle, Dictionary<string, List<string>> invertedIndex)
    {
        var mustHaveTerms = queryBundle[0];
        var atLeastOneTerms = queryBundle[1];
        var mustNotHaveTerms = queryBundle[2];

        var mustHaveDocs = IntersectTermDocs(mustHaveTerms, invertedIndex);
        var atLeastOneDocs = UnionTermDocs(atLeastOneTerms, invertedIndex);
        var mustNotHaveDocs = UnionTermDocs(mustNotHaveTerms, invertedIndex);

        var result = BuildResult(mustNotHaveTerms, mustHaveDocs, atLeastOneDocs, mustNotHaveDocs, invertedIndex);
        if (result.Count == 0)
        {
            return "No results!";
        }
        return string.Join(", ", result);
    }
    private static List<string> IntersectTermDocs(List<string> terms, Dictionary<string, List<string>> invertedIndex)
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
                result.Clear();
            }
        }
        return result;
    }
    private static List<string> UnionTermDocs(List<string> terms, Dictionary<string, List<string>> invertedIndex)
    {
        var result = new List<string>();
        foreach (var term in terms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
                result.AddRange(documents);
        }
        return result.Distinct().ToList();
    }
    private static List<string> BuildResult(List<string> mustNotHaveTerms, List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs, Dictionary<string, List<string>> invertedIndex)
    {
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