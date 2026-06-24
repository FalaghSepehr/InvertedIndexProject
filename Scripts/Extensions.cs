namespace InvertedIndexProgram;
/// <summary>
/// Holds Extention Methods.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Does symbol, number and stop-word removal and stems words. Filters words with less than 2 characters.
    /// </summary>
    /// <param name="terms">Terms to apply filterring to.</param>
    /// <returns>Filtered list of terms</returns>
    public static List<string> FilterTerms(this List<string> terms)
    {
        return terms
            .SelectMany(t => AppUtility.TextProcessing.symbolsAndNumbers.Aggregate(t, (currentTerm, c) => currentTerm.Replace(c, ' '))
            .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(t => !AppUtility.TextProcessing.stopWords.Contains(t))
            .Where(t => t.Length > 2)
            .Select(AppUtility.TextProcessing.Stem).ToList();
    }
}