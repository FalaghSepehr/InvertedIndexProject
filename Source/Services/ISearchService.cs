namespace InvertedIndexProgram;
/// <summary>
/// Searches an inverted index using a structured query and returns matching document names.
/// </summary>
public interface ISearchService
 {
    /// <summary>
    /// Executes the query and returns the list of document names that match.
    /// </summary>
    /// <param name="queryBundle">The parsed query bundle containing must-have, at-least-one, and must-not-have terms.</param>
    /// <returns>A list of matching document names, or an empty list if none match.</returns>
    List<string> Search(QueryBundle queryBundle);
 }