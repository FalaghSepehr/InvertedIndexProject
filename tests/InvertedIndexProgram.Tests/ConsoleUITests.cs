namespace InvertedIndexProgram.Tests;

public class ConsoleUITests
{
  private readonly ISearchService _searchService;
  private readonly IQueryParser _queryParser;
  private readonly IInputReader _inputReader;
  private readonly IOutputWriter _outputWriter;
  
  private readonly ConsoleUI _sut;

  public ConsoleUITests()
  {
    _searchService = Substitute.For<ISearchService>();
    _queryParser = Substitute.For<IQueryParser>();
    _inputReader = Substitute.For<IInputReader>();
    _outputWriter = Substitute.For<IOutputWriter>();

    _sut = new ConsoleUI(_searchService, _queryParser, _inputReader, _outputWriter);
  }

  [Fact]
  public void Run_ExitsLoop_when_menuSelects_2()
  {
    _inputReader.ReadLine().Returns("2");

    _sut.Run(false);

    _outputWriter.Received().WriteLine("Goodbye!");
    _searchService.DidNotReceive().Search(Arg.Any<QueryBundle>());
  }

  [Fact]
  public void Run_LoopsOnInvalidInputThenExits()
  {
    _inputReader.ReadLine().Returns("3", "2");

    _sut.Run(false);

    _outputWriter.Received().WriteLine("Invalid Number!");
    _outputWriter.Received().WriteLine("Goodbye!");
  }

  // edge case
  [Fact]
  public void Run_ShowsEmptyMessageAndExits_when_index_empty()
  {
    _sut.Run(true);

    _outputWriter.Received().WriteLine("\n=================\nIndex is empty!\nGoodbye!\n=================");
    _searchService.DidNotReceive().Search(Arg.Any<QueryBundle>());
  }

  [Fact]
  public void ShowMenu_ShowsTwoOptions_when_index_not_empty()
  {
    _sut.ShowMenu(false);

    _outputWriter.Received().WriteLine("\n=================\nMenu\n1. Search\n2. Exit\n=================");
  }

  // edge case
  [Fact]
  public void ShowMenu_ShowsEmptyMessage_when_index_empty()
  {
    _sut.ShowMenu(true);

    _outputWriter.Received().WriteLine("\n=================\nIndex is empty!\nGoodbye!\n=================");
  }

  [Fact]
  public void HandleInput_AcceptsMenuSelectionWithLeadingZeroAndSpaces()
  {
    _inputReader.ReadLine().Returns(" 01 ");
    _searchService.Search(Arg.Any<QueryBundle>()).Returns([]);

    _sut.HandleInput(out var shouldExit);

    _outputWriter.DidNotReceive().WriteLine("Invalid Input!");
    _outputWriter.DidNotReceive().WriteLine("Invalid Number!");
    Assert.False(shouldExit);
  }

  [Fact]
  public void HandleInput_Searches_WithCorrectMessage_when_menuSelect_1()
  {
    _inputReader.ReadLine().Returns("1");
    _searchService.Search(Arg.Any<QueryBundle>()).Returns([]);

    _sut.HandleInput(out bool shouldExit);

    _outputWriter.Received().WriteLine("Search: ");
    _searchService.Received().Search(Arg.Any<QueryBundle>());
    Assert.False(shouldExit);
  }

  [Fact]
  public void HandleInput_Exits_WithCorrectMessage_when_menuSelect_2()
  {
    _inputReader.ReadLine().Returns("2");

    _sut.HandleInput(out bool shouldExit);

    _outputWriter.Received().WriteLine("Goodbye!");
    _searchService.DidNotReceive().Search(Arg.Any<QueryBundle>());
    Assert.True(shouldExit);
  }

  //edge case
  [Theory]
  [InlineData("3")]
  [InlineData("4")]
  [InlineData("52")]
  public void HandleInput_LoopsBack_WithCorrectMessage_when_menuSelect_invalid_number(string input)
  {
    _inputReader.ReadLine().Returns(input);

    _sut.HandleInput(out bool shouldExit);

    _outputWriter.Received().WriteLine("Invalid Number!");
    _searchService.DidNotReceive().Search(Arg.Any<QueryBundle>());
    Assert.False(shouldExit);
  }

  //edge case
  [Theory]
  [InlineData("")]
  [InlineData("b")]
  [InlineData("1 2")]
  public void HandleInput_LoopsBack_WithCorrectMessage_when_menuSelect_not_number(string input)
  {
    _inputReader.ReadLine().Returns(input);

    _sut.HandleInput(out bool shouldExit);

    _outputWriter.Received().WriteLine("Invalid Input!");
    _searchService.DidNotReceive().Search(Arg.Any<QueryBundle>());
    Assert.False(shouldExit);
  }

  [Fact]
  public void GetResultMessage_ReturnsCommaSeparatedDocumentNames()
  {
    _queryParser.ParseQuery().Returns(new QueryBundle());
    _searchService.Search(Arg.Any<QueryBundle>()).Returns(["doc1", "doc2"]);

    var result = _sut.GetResultMessage();

    Assert.Equal("doc1, doc2", result);
  }

  [Fact]
  public void GetResultMessage_ReturnsNoResultsMessage_when_results_empty()
  {
    _queryParser.ParseQuery().Returns(new QueryBundle());
    _searchService.Search(Arg.Any<QueryBundle>()).Returns([]);

    var result = _sut.GetResultMessage();

    Assert.Equal("No results!", result);
  }

}