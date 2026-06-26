using Microsoft.VisualBasic;

namespace InvertedIndexProgram;
/// <summary>
/// Prints the menu and handles menu input.
/// </summary>
public class ConsoleUI
{
    private readonly IInputReader _inputReader;
    private readonly QueryParser _queryParser;
    private readonly InvertedIndex _invertedIndex;
    public ConsoleUI(InvertedIndex invertedIndex, QueryParser queryParser, IInputReader inputReader)
    {
        _queryParser = queryParser;
        _invertedIndex = invertedIndex;
        _inputReader = inputReader;
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
        System.Console.WriteLine("=================");
        System.Console.WriteLine("Select: ");
        System.Console.WriteLine("1. Search");
        System.Console.WriteLine("2. Exit");
        System.Console.WriteLine("=================");
    }
    private void HandleInput(out bool shouldExit)
    {
        if (int.TryParse(_inputReader.ReadLine(), out int menuSelect))
        {
            switch (menuSelect)
            {
                case 1:
                    Console.Write("Search: ");
                    Console.WriteLine(GetResultMessage());
                    shouldExit = false;
                    break;
                case 2:
                    Console.WriteLine("GoodBye!");
                    shouldExit = true;
                    break;
                default:
                    Console.WriteLine("Invalid Number!");
                    shouldExit = false;
                    break;
            }
        }
        else
        {
            Console.WriteLine("Invalid Input!");
            shouldExit = false;
        }
    }
    private string GetResultMessage()
    {
        var results = _invertedIndex.GetSearchResult(_queryParser.ParseQuery());
        return results.Count == 0 ? "No results!" : string.Join(", ", results);
    }
}