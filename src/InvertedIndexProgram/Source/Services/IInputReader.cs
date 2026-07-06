namespace InvertedIndexProgram;
/// <summary>
/// Reads a line of input from an input source.
/// </summary>
public interface IInputReader
{
    /// <summary>
    /// Takes one line as input.
    /// </summary>
    /// <returns></returns>
    string ReadLine();
}