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
    public List<string> ExtractTerms(string text)
    {
        return NormalizeTerms(PrepareTokens(text));
    }
    public List<string> PrepareTokens(string rawText)
    {
        return Tokenize(rawText.Trim().ToLower());
    }
    public List<string> NormalizeTerms(List<string> terms)
    {
        return terms
            .Select(CleanSymbolsAndNumbers)
            .SelectMany(t => t.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Where(IsIndexable)
            .Select(Stem)
            .ToList();
    }
    private List<string> Tokenize(string text)
    {
        return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).ToList();
    }
    private string CleanSymbolsAndNumbers(string term)
    {
        return _symbolsAndNumbers.Aggregate(term, (current, c) => current.Replace(c, ' '));
    }
    private bool IsIndexable(string term)
    {
        return !_stopWords.Contains(term) && term.Length > 2;
    }
    private static readonly EnglishStemmer Stemmer = new();
    private string Stem(string word) => Stemmer.GetStem(word);
}