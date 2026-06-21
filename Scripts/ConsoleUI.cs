public static class ConsoleUI
{
    public static void Run(InvertedIndex invertedIndex)
    {
        do
        {
            ShowMenu();
            HandleInput(invertedIndex, out var shouldExit);
            if (shouldExit)
            {
                break;
            }
        } while (true);
    }
    private static void ShowMenu()
    {
        System.Console.WriteLine("=================");
        System.Console.WriteLine("Select: ");
        System.Console.WriteLine("1. Search");
        System.Console.WriteLine("2. Exit");
        System.Console.WriteLine("=================");
    }
    private static void HandleInput(InvertedIndex invertedIndex, out bool shouldExit)
    {
        if (int.TryParse(Console.ReadLine(), out int menuSelect))
        {
            switch (menuSelect)
            {
                case 1:
                    Console.Write("Search: ");
                    Console.WriteLine(invertedIndex.Search(QueryParser.ParseQuery()));
                    shouldExit = false;
                    break;
                case 2:
                    System.Console.WriteLine("GoodBye!");
                    shouldExit = true;
                    break;
                default:
                    System.Console.WriteLine("Invalid Number!");
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