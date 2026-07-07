namespace InvertedIndexProgram;
/// <summary>
/// Console-based user interface that displays a menu and handles user input
/// for searching the inverted index. Communicates results via an
/// <see cref="IOutputWriter"/> and reads input via an <see cref="IInputReader"/>.
/// </summary>
public class ConsoleUI
{
  private readonly IInputReader _inputReader;
  private readonly IOutputWriter _outputWriter;
  private readonly IQueryParser _queryParser;
  private readonly ISearchService _searcher;
  public ConsoleUI(ISearchService searcher, IQueryParser queryParser, IInputReader inputReader, IOutputWriter outputWriter)
  {
    _queryParser = queryParser;
    _searcher = searcher;
    _inputReader = inputReader;
    _outputWriter = outputWriter;
  }
  /// <summary>
  /// Starts the menu loop and processes user input until exit is requested.
  /// </summary>
  public void Run()
  {
    do
    {
      ShowMenu();
      HandleInput(out var shouldExit);
      if (shouldExit)
      {
        break;
      }
    } while (true);
  }
  private void ShowMenu()
  {
    _outputWriter.WriteLine("\n=================\nMenu\n1. Search\n2. Exit\n=================");
  }
  internal void HandleInput(out bool shouldExit)
  {
    if (int.TryParse(_inputReader.ReadLine(), out int menuSelect))
    {
      switch (menuSelect)
      {
        case 1:
          _outputWriter.WriteLine("Search: ");
          _outputWriter.WriteLine(GetResultMessage());
          shouldExit = false;
          break;
        case 2:
          _outputWriter.WriteLine("GoodBye!");
          shouldExit = true;
          break;
        default:
          _outputWriter.WriteLine("Invalid Number!");
          shouldExit = false;
          break;
      }
    }
    else
    {
      _outputWriter.WriteLine("Invalid Input!");
      shouldExit = false;
    }
  }
  internal string GetResultMessage()
  {
    var results = _searcher.Search(_queryParser.ParseQuery());
    return results.Count == 0 ? "No results!" : string.Join(", ", results);
  }
}