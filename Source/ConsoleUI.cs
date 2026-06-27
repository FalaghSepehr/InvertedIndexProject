using Microsoft.VisualBasic;

namespace InvertedIndexProgram;
/// <summary>
/// Prints the menu and handles menu input.
/// </summary>
public class ConsoleUI
{
    private readonly IInputReader _inputReader;
    private readonly IOutputWriter _outputWriter;
    private readonly QueryParser _queryParser;
    private readonly InvertedIndex _invertedIndex;
    public ConsoleUI(InvertedIndex invertedIndex, QueryParser queryParser, IInputReader inputReader, IOutputWriter outputWriter)
    {
        _queryParser = queryParser;
        _invertedIndex = invertedIndex;
        _inputReader = inputReader;
        _outputWriter = outputWriter;
    }
    /// <summary>
    /// Runs The Console UI. Requires an inverted index to function.
    /// </summary>
    /// <param name="invertedIndex">Index to Search in.</param>
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
    private void HandleInput(out bool shouldExit)
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
    private string GetResultMessage()
    {
        var results = _invertedIndex.GetSearchResult(_queryParser.ParseQuery());
        return results.Count == 0 ? "No results!" : string.Join(", ", results);
    }
}