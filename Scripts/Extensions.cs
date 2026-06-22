namespace InvertedIndex_Program;

public static class Extensions
{
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