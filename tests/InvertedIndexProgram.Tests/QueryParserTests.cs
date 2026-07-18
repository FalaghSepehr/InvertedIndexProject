namespace InvertedIndexProgram.Tests;

public class QueryParserTests
{
  private readonly ITextProcessor _textProcessor;
  private readonly IInputReader _inputReader;
  private readonly QueryParser _sut;

  public QueryParserTests()
  {
    _textProcessor = Substitute.For<ITextProcessor>();
    _inputReader = Substitute.For<IInputReader>();
    _sut = new(_textProcessor, _inputReader);
  }

  [Fact]
  public void ParseQuery_ReadsInputAndReturnsCategorizedBundle()
  {
    _inputReader.ReadLine().Returns("cat +dog -bird");
    _textProcessor.PrepareTokens("cat +dog -bird").Returns(["cat", "+dog", "-bird"]);
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());

    var result = _sut.ParseQuery();

    Assert.Equal(["cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal(["bird"], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesAllThree()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());

    var result = _sut.Categorize(["cat", "+dog", "-bird"]);

    Assert.Equal(["cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal(["bird"], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesOnlyBareTerms()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());
    var result = _sut.Categorize(["cat", "dog"]);
    Assert.Equal(["cat", "dog"], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesOnlyAtLeastOneTerms()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());
    var result = _sut.Categorize(["+cat", "+dog"]);
    Assert.Equal([], result.MustHave);
    Assert.Equal(["cat", "dog"], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesOnlyMustNotHaveTerms()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());
    var result = _sut.Categorize(["-cat", "-dog"]);
    Assert.Equal([], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal(["cat", "dog"], result.MustNotHave);
  }

  [Fact]
  public void Categorize_HandlesEmptyInput()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());
    var result = _sut.Categorize([]);
    Assert.Equal([], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  //edge case
  [Fact]
  public void Categorize_IgnoresPrefixOnlyItems()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());

    var result = _sut.Categorize(["+", "-"]);

    Assert.Equal([], result.MustHave);
    Assert.Equal([], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }

  //edge case
  [Fact]
  public void Categorize_IgnoresPrefixOnlyAndKeepsOthers()
  {
    _textProcessor.NormalizeTerms(Arg.Any<List<string>>()).Returns(x => x.Arg<List<string>>());
    var result = _sut.Categorize(["+dog", "+", "cat", "-"]);
    Assert.Equal(["cat"], result.MustHave);
    Assert.Equal(["dog"], result.AtLeastOne);
    Assert.Equal([], result.MustNotHave);
  }
}