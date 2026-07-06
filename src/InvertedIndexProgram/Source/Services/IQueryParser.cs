namespace InvertedIndexProgram;

/// <summary>
/// Parses a raw query string into a structured <see cref="QueryBundle"/>.
/// </summary>
public interface IQueryParser
{
    /// <summary>
    /// Reads and parses a query into categorized term lists.
    /// </summary>
    /// <returns>A bundle containing must-have, at-least-one, and must-not-have term lists.</returns>
    QueryBundle ParseQuery();
}