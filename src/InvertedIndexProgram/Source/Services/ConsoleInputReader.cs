namespace InvertedIndexProgram;
/// <summary>
/// Reads input from console.
/// Returns an empty string if the input is null.
/// </summary>
public class ConsoleInputReader : IInputReader
{
    public string ReadLine() => Console.ReadLine() ?? string.Empty;
}