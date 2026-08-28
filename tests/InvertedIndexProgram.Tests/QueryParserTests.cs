namespace InvertedIndexProgram.Tests;

public class QueryParserTests
{
  private readonly ITextProcessor _textProcessor;
  private readonly QueryParser _sut;

  public QueryParserTests()
  {
    _textProcessor = Substitute.For<ITextProcessor>();
    _sut = new(_textProcessor);
  }

  [Fact]
  public void ParseQuery_HandlesBothNormalQueriesAndExactPhraseQueries()
  {
    _textProcessor.GetRawTokens("cat +dog -bird").Returns(["cat", "+dog", "-bird"]);
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());

    var result = _sut.ParseQuery("\"Running\" cat +dog -bird");

    _textProcessor.Received().NormalizeTerms(Arg.Is<List<string>>(l => l.SequenceEqual(new List<string> { "cat" })));
    _textProcessor.Received().NormalizeTerms(Arg.Is<List<string>>(l => l.SequenceEqual(new List<string> { "dog" })));
    _textProcessor.Received().NormalizeTerms(Arg.Is<List<string>>(l => l.SequenceEqual(new List<string> { "bird" })));

    Assert.Equal(["Running", "cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal(["bird"], result.MustNotHave);
  }

  [Fact]
  public void ParseQuery_HandlesOnlyNormalQueries()
  {
    _textProcessor.GetRawTokens("cat +dog -bird").Returns(["cat", "+dog", "-bird"]);
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());

    var result = _sut.ParseQuery("cat +dog -bird");

    _textProcessor.Received().NormalizeTerms(Arg.Is<List<string>>(l => l.SequenceEqual(new List<string> { "cat" })));
    _textProcessor.Received().NormalizeTerms(Arg.Is<List<string>>(l => l.SequenceEqual(new List<string> { "dog" })));
    _textProcessor.Received().NormalizeTerms(Arg.Is<List<string>>(l => l.SequenceEqual(new List<string> { "bird" })));

    Assert.Equal(["cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal(["bird"], result.MustNotHave);
  }

  [Fact]
  public void ParseQuery_HandlesOnlyExactPhraseQueries()
  {
    _textProcessor.GetRawTokens("").Returns([]);

    var result = _sut.ParseQuery("\"Running\" +\"Cat\" -\"dog.2\"");

    _textProcessor.DidNotReceive().NormalizeTerms(Arg.Any<List<string>>());
    Assert.Equal(["Running"], result.MustHave);
    Assert.Equal(["Cat"], result.AtLeastOne);
    Assert.Equal(["dog.2"], result.MustNotHave);
  }

  [Fact]
  public void ParseQuery_HandlesTwoWordExactPhraseQueries()
  {
    _textProcessor.GetRawTokens("").Returns([]);
    
    var result = _sut.ParseQuery("\"Star Academy\" +\"I have\" -\"Bad Cat\"");

    Assert.Equal(["Star Academy"], result.MustHave);
    Assert.Equal(["I have"], result.AtLeastOne);
    Assert.Equal(["Bad Cat"], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesAllThree()
  {
    var result = _sut.Categorize(["cat", "+dog", "-Running"]);

    Assert.Equal(["cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal(["Running"], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesOnlyMustHaveTerms()
  {
    var result = _sut.Categorize(["cat", "dog"]);
    Assert.Equal(["cat", "dog"], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesOnlyAtLeastOneTerms()
  {
    var result = _sut.Categorize(["+cat", "+dog"]);
    Assert.Equal([], result.MustHave);
    Assert.Equal(["cat", "dog"], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesOnlyMustNotHaveTerms()
  {
    var result = _sut.Categorize(["-cat", "-dog"]);
    Assert.Equal([], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal(["cat", "dog"], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesEmptyInput()
  {
    var result = _sut.Categorize([]);
    Assert.Equal([], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  [Fact]
  public void Categorize_DoesNotNormalize()
  {
    var result = _sut.Categorize(["bird", "\"Running\"", "+dog.", "-cat1"]);

    _textProcessor.DidNotReceive().NormalizeTerms(Arg.Any<List<string>>());
    Assert.Equal(["bird", "\"Running\""], result.MustHave);
    Assert.Equal(["dog."], result.AtLeastOne);
    Assert.Equal(["cat1"], result.MustNotHave);
  }

  //edge case
  [Fact]
  public void Categorize_IgnoresPrefixOnlyItems()
  {
    var result = _sut.Categorize(["+", "-"]);

    Assert.Equal([], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  //edge case
  [Fact]
  public void Categorize_IgnoresPrefixOnlyAndKeepsOthers()
  {
    var result = _sut.Categorize(["+dog", "+", "cat", "-"]);
    Assert.Equal(["cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }
}