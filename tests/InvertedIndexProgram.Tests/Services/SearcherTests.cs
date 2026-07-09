namespace InvertedIndexProgram.Tests;

public class SearcherTests
{
  private static readonly IReadOnlyDictionary<string, HashSet<string>> _invertedIndexDic = new Dictionary<string, HashSet<string>>
  {
    ["cat"] = ["doc1", "doc2"],
    ["dog"] = ["doc2", "doc3"],
    ["bird"] = ["doc1", "doc3"]
  };
  private static readonly Searcher _searcher = new(_invertedIndexDic);

  public class Search
  {
    [Fact]
    public void ReturnsResults_when_all_categories_active()
    {
      var query = new QueryBundle
      {
        MustHave = ["cat"],
        AtLeastOne = ["dog"],
        MustNotHave = ["bird"]
      };
      var result = _searcher.Search(query);
      Assert.Equal(["doc2"], result);
    }

    [Fact]
    public void ReturnsResults_when_only_must_have()
    {
      var query = new QueryBundle { MustHave = ["cat", "dog"] };
      var result = _searcher.Search(query);
      Assert.Equal(["doc2"], result);
    }

    [Fact]
    public void ReturnsResults_when_only_at_least_one()
    {
      var query = new QueryBundle { AtLeastOne = ["cat", "bird"] };
      var result = _searcher.Search(query);
      Assert.Equal(["doc1", "doc2", "doc3"], result);
    }

    [Fact]
    public void ReturnsResults_when_only_must_not_have()
    {
      var query = new QueryBundle { MustNotHave = ["bird"] };
      var result = _searcher.Search(query);
      Assert.Equal(["doc2"], result);
    }

    [Fact]
    public void ReturnsAllDocs_when_must_not_have_terms_not_found()
    {
      var query = new QueryBundle { MustNotHave = ["cow"] };
      var result = _searcher.Search(query);
      Assert.Equal(["doc1", "doc2", "doc3"], result);
    }
  }
  
  public class IntersectTermDocs
  {
    [Fact]
    public void ReturnsEmptyDocsList_if_terms_empty()
    {
      var mustHaveTerms = new List<string>();
      var result = _searcher.IntersectTermDocs(mustHaveTerms);
      Assert.Equal([], result);
    }

    [Fact]
    public void ReturnsEmptyDocsList_if_first_term_not_found()
    {
      var mustHaveTerms = new List<string>(["cow", "bird"]);
      var result = _searcher.IntersectTermDocs(mustHaveTerms);
      Assert.Equal([], result);
    }

    [Fact]
    public void ReturnsEmptyDocsList_if_term_not_found_midway()
    {
      var mustHaveTerms = new List<string>(["bird", "cat", "cow"]);
      var result = _searcher.IntersectTermDocs(mustHaveTerms);
      Assert.Equal([], result);
    }
    
    [Fact]
    public void ReturnsDocsForSingleTerm()
    {
      var mustHaveTerms = new List<string>(["cat"]);
      var result = _searcher.IntersectTermDocs(mustHaveTerms);
      Assert.Equal(["doc1", "doc2"], result);
    }

    [Fact]
    public void ReturnsIntersectionOfMultipleTerms()
    {
      var mustHaveTerms = new List<string>(["cat", "dog"]);
      var result = _searcher.IntersectTermDocs(mustHaveTerms);
      Assert.Equal(["doc2"], result);
    }
  }

  public class UnionTermDocs
  {
    [Fact]
    public void ReturnsEmptyDocsList_if_terms_empty()
    {
      var terms = new List<string>();
      var result = _searcher.UnionTermDocs(terms);
      Assert.Equal([], result);
    }

    [Fact]
    public void ReturnsEmptyDocsList_if_no_terms_found()
    {
      var terms = new List<string>(["cow", "sheep"]);
      var result = _searcher.UnionTermDocs(terms);
      Assert.Equal([], result);
    }

    [Fact]
    public void ReturnsUnionOfFoundTermDocs()
    {
      var terms = new List<string>(["cat", "dog", "cow"]);
      var result = _searcher.UnionTermDocs(terms);
      Assert.Equal(["doc1", "doc2", "doc3"], result);
    }
  }

  public class BuildResult
  {
    [Fact]
    public void CombinesAllThreeCategories()
    {
      var result = _searcher.BuildResult(true, true, true, ["doc1", "doc2"], ["doc2", "doc3"], ["doc3"]);
      Assert.Equal(["doc2"], result);
    }

    [Fact]
    public void ReturnsMustHaveDocs_when_only_must_have()
    {
      var result = _searcher.BuildResult(true, false, false, ["doc1", "doc2"], [], []);
      Assert.Equal(["doc1", "doc2"], result);
    }

    [Fact]
    public void ReturnsAtLeastOneDocs_when_only_at_least_one()
    {
      var result = _searcher.BuildResult(false, true, false, [], ["doc1", "doc2"], []);
      Assert.Equal(["doc1", "doc2"], result);
    }

    [Fact]
    public void ReturnsAllDocsExceptExcluded_when_only_must_not_have()
    {
      var result = _searcher.BuildResult(false, false, true, [], [], ["doc1"]);
      Assert.Equal(["doc2", "doc3"], result);
    }

    [Fact]
    public void IntersectsMustHaveAndAtLeastOne()
    {
      var result = _searcher.BuildResult(true, true, false, ["doc1", "doc2"], ["doc2", "doc3"], []);
      Assert.Equal(["doc2"], result);
    }

    [Fact]
    public void ReturnsMustHaveMinusExclusions()
    {
      var result = _searcher.BuildResult(true, false, true, ["doc1", "doc2"], [], ["doc2"]);
      Assert.Equal(["doc1"], result);
    }

    // edge case
    [Fact]
    public void ReturnsEmpty_when_atLeastOneTerms_not_found()
    {
      var result = _searcher.BuildResult(true, true, false, ["doc1", "doc2"], [], []);
      Assert.Equal([], result);
    }

    // edge case
    [Fact]
    public void ReturnsEmpty_when_mustHaveTerms_not_found()
    {
      var result = _searcher.BuildResult(true, false, false, [], [], []);
      Assert.Equal([], result);
    }

    // edge case
    [Fact]
    public void ReturnsEmpty_when_no_terms_provided()
    {
      var result = _searcher.BuildResult(false, false, false, [], [], []);
      Assert.Equal([], result);
    }

    // edge case, maybe redundant
    [Fact]
    public void ReturnsIntersection_when_exclusion_terms_not_found()
    {
      // "foundTerm +foundTerm -notFoundTerm"
      var result = _searcher.BuildResult(true, true, true, ["doc1", "doc2"], ["doc2", "doc3"], []);
      Assert.Equal(["doc2"], result);
    }

    // edge case
    [Fact]
    public void ReturnsAllDocs_when_mustNotHaveTerms_not_found()
    {
      var result = _searcher.BuildResult(false, false, true, [], [], []);
      Assert.Equal(["doc1", "doc2", "doc3"], result);
    }
  }
}