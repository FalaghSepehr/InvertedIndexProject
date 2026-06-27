namespace InvertedIndexProgram;

public class ConsoleInputReader : IInputReader
{
    public string ReadLine() => Console.ReadLine() ?? string.Empty;
}