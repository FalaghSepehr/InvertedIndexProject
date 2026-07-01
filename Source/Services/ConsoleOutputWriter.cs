namespace InvertedIndexProgram;
/// <summary>
/// Writes output messages to the console.
/// </summary>
public class ConsoleOutputWriter : IOutputWriter
{
    public void WriteLine(string message) => Console.WriteLine(message);
}