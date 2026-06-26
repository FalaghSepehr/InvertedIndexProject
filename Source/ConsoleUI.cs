using Microsoft.VisualBasic;

namespace InvertedIndexProgram;
/// <summary>
/// Prints the menu and handles menu input.
/// </summary>
public class ConsoleUI
{
    private readonly QueryParser _queryParser;
    private readonly InvertedIndex _invertedIndex;
    public ConsoleUI(InvertedIndex invertedIndex, QueryParser queryParser)
    {
        _queryParser = queryParser;
        _invertedIndex = invertedIndex;
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
            HandleInput(_invertedIndex, out var shouldExit);
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
    private void HandleInput(InvertedIndex invertedIndex, out bool shouldExit)
    {
        if (int.TryParse(Console.ReadLine(), out int menuSelect))
        {
            switch (menuSelect)
            {
                case 1:
                    Console.Write("Search: ");
                    Console.WriteLine(invertedIndex.GetSearchResult(_queryParser.ParseQuery()));
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
            System.Console.WriteLine("Invalid Input!");
            shouldExit = false;
        }
    }
}