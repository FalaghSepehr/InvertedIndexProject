namespace InvertedIndexProgram;

public class Searcher : ISearchService
{
  private IReadOnlyDictionary<string, HashSet<string>> _invertedIndexDic;

  public Searcher(IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    _invertedIndexDic = invertedIndexDic;
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
  internal List<string> IntersectTermDocs(List<string> terms)
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
  internal List<string> UnionTermDocs(List<string> terms)
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
  internal List<string> BuildResult(bool hasMustHaveTerms, List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs)
  {
    if (!hasMustHaveTerms && atLeastOneDocs.Count == 0 && mustNotHaveDocs.Count == 0)
    {
      return new List<string>();
    }

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