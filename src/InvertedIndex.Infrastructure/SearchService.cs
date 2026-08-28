using InvertedIndex.Core;

namespace InvertedIndex.Infrastructure;

public class SearchService : ISearchService
{
  private readonly Searcher _searcher;
  private readonly IReadOnlyDictionary<string, HashSet<string>> _index;

  public SearchService(Searcher searcher, IReadOnlyDictionary<string, HashSet<string>> index)
  {
    _searcher = searcher;
    _index = index;
  }

  public List<string> Search(QueryBundle query)
  {
    return _searcher.Search(query, _index);
  }
}