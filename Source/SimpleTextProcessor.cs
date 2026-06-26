using PorterStemmer.Stemmers;

namespace InvertedIndexProgram;

public class SimpleTextProcessor : ITextProcessor
{
    private readonly char[] _symbolsAndNumbers;
    private readonly HashSet<string> _stopWords;

    public SimpleTextProcessor (char[] symbolsAndNumbers, HashSet<string> stopWords)
    {
        _symbolsAndNumbers = symbolsAndNumbers;
        _stopWords = stopWords;
    }
    public IEnumerable<string> Tokenize(string text)
    {
        return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
    }
    /// <summary>
    /// Does symbol, number and stop-word removal and stems words. Filters words with less than 2 characters.
    /// </summary>
    /// <param name="terms">Terms to apply filterring to.</param>
    /// <returns>Filtered list of terms</returns>
    public IEnumerable<string> FilterTerms(IEnumerable<string> terms)
    {
        return terms
            .SelectMany(t => _symbolsAndNumbers.Aggregate(t, (currentTerm, c) => currentTerm.Replace(c, ' '))
            .Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(t => !_stopWords.Contains(t))
            .Where(t => t.Length > 2)
            .Select(Stem);
    }
    private static readonly EnglishStemmer Stemmer = new();
    /// <summary>
    /// Stems an English word to its root form using the Porter algorithm
    /// (via the PorterStemmer NuGet package).
    /// </summary>
    /// <param name="word">The word to stem.</param>
    /// <returns>The stemmed word (e.g., "running" → "run").</returns>
    public string Stem(string word) => Stemmer.GetStem(word);
}