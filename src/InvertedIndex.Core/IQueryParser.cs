namespace InvertedIndex.Core;

/// <summary>
/// Parses a query string into a structured <see cref="QueryBundle"/>.
/// Implementations must handle + (at-least-one), - (must-not-have), bare words (must-have),
/// and "quoted phrases" (exact matching, not normalized).
/// </summary>
public interface IQueryParser
{
  /// <summary>
  /// Parses the provided query text into categorized term lists.
  /// </summary>
  /// <param name="queryText">The raw query string to parse.</param>
  /// <returns>A bundle containing categorized must-have, at-least-one, and must-not-have terms.</returns>
  QueryBundle ParseQuery(string queryText);
}