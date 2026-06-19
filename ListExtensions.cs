public static class ListExtensions
{
    public static List<string> FilterTerms(this List<string> terms)
    {
        return terms
            .SelectMany(t => AppUtility.symbolsAndNumbers.Aggregate(t, (currentTerm, c) => currentTerm.Replace(c, ' '))
            .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(t => !AppUtility.stopWords.Contains(t))
            .Where(t => t.Length > 2)
            .Select(AppUtility.Stem).ToList();
    }
}