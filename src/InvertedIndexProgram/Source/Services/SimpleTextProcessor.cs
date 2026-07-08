using PorterStemmer.Stemmers;

namespace InvertedIndexProgram;
/// <summary>
/// Provides basic text processing by cleaning symbols and numbers, removing stop words,
/// and applying Porter stemming. Implements <see cref="ITextProcessor"/>.
/// </summary>
public class SimpleTextProcessor : ITextProcessor
{
  private readonly char[] _symbols;
  private static readonly char[] _numbers = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
  private readonly char[] _allCharsToRemove;

  private readonly HashSet<string> _stopWords;

  public SimpleTextProcessor(char[] symbols, HashSet<string> stopWords)
  {
    _symbols = symbols;
    _stopWords = stopWords;
    _allCharsToRemove = new char[_symbols.Length + _numbers.Length];
    _symbols.CopyTo(_allCharsToRemove, 0);
    _numbers.CopyTo(_allCharsToRemove, _symbols.Length);
  }
  public List<string> ExtractTerms(string rawText)
  {
    return NormalizeTerms(PrepareTokens(rawText));
  }
  public List<string> PrepareTokens(string rawText)
  {
    return Tokenize(rawText.Trim().ToLower());
  }
  public List<string> NormalizeTerms(List<string> terms)
  {
    return terms
        .Select(CleanSymbolsAndNumbers)
        .SelectMany(SplitOnSpaces)
        .Where(IsIndexable)
        .Select(Stem)
        .ToList();
  }
  internal List<string> Tokenize(string text)
  {
    return text.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).ToList();
  }
  internal string CleanSymbolsAndNumbers(string term)
  {
    return _allCharsToRemove.Aggregate(term, (current, c) => current.Replace(c, ' ')).Trim();
  }
  internal List<string> SplitOnSpaces(string term)
  {
    return term.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
  }
  internal bool IsIndexable(string term)
  {
    return !_stopWords.Contains(term) && term.Length > 2;
  }
  private static readonly EnglishStemmer Stemmer = new();
  internal string Stem(string word) => Stemmer.GetStem(word);
}