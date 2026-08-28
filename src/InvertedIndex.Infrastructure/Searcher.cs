using InvertedIndex.Core;

namespace InvertedIndex.Infrastructure;

public class Searcher : ISearchService
{
  public List<string> Search(QueryBundle queryBundle, IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    var hasMustHaveTerms = queryBundle.MustHave.Count > 0;
    var hasAtLeastOneTerms = queryBundle.AtLeastOne.Count > 0;
    var hasMustNotHaveTerms = queryBundle.MustNotHave.Count > 0;
    var mustHaveDocs = IntersectTermDocs(queryBundle.MustHave, invertedIndexDic);
    var atLeastOneDocs = UnionTermDocs(queryBundle.AtLeastOne, invertedIndexDic);
    var mustNotHaveDocs = UnionTermDocs(queryBundle.MustNotHave, invertedIndexDic);

    var result = BuildResult(hasMustHaveTerms, hasAtLeastOneTerms, hasMustNotHaveTerms, mustHaveDocs, atLeastOneDocs, mustNotHaveDocs, invertedIndexDic);

    return result.OrderBy(v => v).ToList();
  }
  internal List<string> IntersectTermDocs(List<string> terms, IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    if (terms.Count == 0)
    {
      return new List<string>();
    }

    if (!invertedIndexDic.TryGetValue(terms[0], out var firstDocs))
    {
      return new List<string>();
    }

    var resultSet = new HashSet<string>(firstDocs);

    for (int i = 1; i < terms.Count; i++)
    {
      if (!invertedIndexDic.TryGetValue(terms[i], out var docs))
      {
        return new List<string>();
      }
      resultSet.IntersectWith(docs);
    }

    return resultSet.ToList();
  }
  internal List<string> UnionTermDocs(List<string> terms, IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    var resultSet = new HashSet<string>();
    foreach (var term in terms)
    {
      if (invertedIndexDic.TryGetValue(term, out var documents))
      {
        resultSet.UnionWith(documents);
      }
    }
    return resultSet.ToList();
  }
  internal List<string> BuildResult(bool hasMustHaveTerms, bool hasAtLeastOneTerms, bool hasMustNotHaveTerms, List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs, IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    if (!hasMustHaveTerms && !hasAtLeastOneTerms && !hasMustNotHaveTerms)
    {
      return new List<string>();
    }
    
    var allDocs = invertedIndexDic.Values.SelectMany(d => d).Distinct();
    List<string> positiveDocs;

    if (hasMustHaveTerms)
    {
      if (mustHaveDocs.Count == 0)
      {
        return [];
      }
      if (hasAtLeastOneTerms)
      {
        positiveDocs = atLeastOneDocs.Count == 0 ? [] : mustHaveDocs.Intersect(atLeastOneDocs).ToList();
      }
      else
      {
        positiveDocs = mustHaveDocs;
      }
    }
    else if (hasAtLeastOneTerms)
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