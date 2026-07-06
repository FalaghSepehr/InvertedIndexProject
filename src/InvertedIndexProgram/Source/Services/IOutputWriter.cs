namespace InvertedIndexProgram;
/// <summary>
/// Writes a meesage to an output destination.
/// </summary>
public interface IOutputWriter
{
    /// <summary>
    /// Writes the specified message followed by a line terminator to the output.
    /// </summary>
    /// <param name="message">The message to write.</param>
    void WriteLine(string message);
}