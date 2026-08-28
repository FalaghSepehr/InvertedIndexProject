using InvertedIndex.Core;

namespace InvertedIndex.Infrastructure;

public class Searcher
{
  public List<string> Search(QueryBundle queryBundle, IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    var hasMustHaveTerms = queryBundle.MustHave.Count > 0;
    var hasAtLeastOneTerms = queryBundle.AtLeastOne.Count > 0;
    var hasMustNotHaveTerms = queryBundle.MustNotHave.Count > 0;
    var mustHaveDocs = IntersectTermDocs(queryBundle.MustHave, invertedIndexDic);
    var atLeastOneDocs = UnionTermDocs(queryBundle.AtLeastOne, invertedIndexDic);
    var mustNotHaveDocs = UnionTermDocs(queryBundle.MustNotHave, invertedIndexDic);

    var context = new SearchContext
    (
      queryBundle.MustHave.Count > 0,
      queryBundle.AtLeastOne.Count > 0,
      queryBundle.MustNotHave.Count > 0,
      mustHaveDocs,
      atLeastOneDocs,
      mustNotHaveDocs
    );

    var result = BuildResult(context, invertedIndexDic);

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
  internal List<string> BuildResult(SearchContext context, IReadOnlyDictionary<string, HashSet<string>> invertedIndexDic)
  {
    if (!context.HasMustHaveTerms && !context.HasAtLeastOneTerms && !context.HasMustNotHaveTerms)
    {
      return new List<string>();
    }

    var allDocs = invertedIndexDic.Values.SelectMany(d => d).Distinct();
    List<string> positiveDocs;

    if (context.HasMustHaveTerms)
    {
      if (context.MustHaveDocs.Count == 0)
      {
        return [];
      }
      if (context.HasAtLeastOneTerms)
      {
        positiveDocs = context.AtLeastOneDocs.Count == 0 ? [] : context.MustHaveDocs.Intersect(context.AtLeastOneDocs).ToList();
      }
      else
      {
        positiveDocs = context.MustHaveDocs;
      }
    }
    else if (context.HasAtLeastOneTerms)
    {
      positiveDocs = context.AtLeastOneDocs;
    }
    else
    {
      positiveDocs = allDocs.ToList();
    }
    return positiveDocs.Except(context.MustNotHaveDocs).ToList();
  }
}
internal record SearchContext
(
  bool HasMustHaveTerms,
  bool HasAtLeastOneTerms,
  bool HasMustNotHaveTerms,
  List<string> MustHaveDocs,
  List<string> AtLeastOneDocs,
  List<string> MustNotHaveDocs
);