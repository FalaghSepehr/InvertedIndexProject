using InvertedIndex.Core;

namespace InvertedIndex.Infrastructure;
/// <summary>
/// Parses a raw query string into a <see cref="QueryBundle"/> by categorizing terms
/// by prefix: + (at-least-one), - (must-not-have), and bare (must-have).
/// Each category is normalized using the injected <see cref="ITextProcessor"/>.
/// </summary>
public class QueryParser : IQueryParser
{
  private readonly IInputReader _inputReader;
  private readonly ITextProcessor _textProcessor;

  public QueryParser(ITextProcessor textProcessor, IInputReader inputReader)
  {
    _inputReader = inputReader;
    _textProcessor = textProcessor;
  }
  public QueryBundle ParseQuery()
  {
    var queryTokens = _textProcessor.PrepareTokens(_inputReader.ReadLine());
    return Categorize(queryTokens);
  }
  internal QueryBundle Categorize(List<string> tokens)
  {
    var mustHaveTerms = new List<string>();
    var atLeastOneTerms = new List<string>();
    var mustNotHaveTerms = new List<string>();

    foreach (string item in tokens)
    {
      switch (item[0])
      {
        case '+':
          if (item.Length > 1)
          {
            atLeastOneTerms.Add(item.Substring(1));
          }
          break;
        case '-':
          if (item.Length > 1)
          {
            mustNotHaveTerms.Add(item.Substring(1));
          }
          break;
        default:
          mustHaveTerms.Add(item);
          break;
      }
    }

    return new QueryBundle
    {
      MustHave = _textProcessor.NormalizeTerms(mustHaveTerms),
      AtLeastOne = _textProcessor.NormalizeTerms(atLeastOneTerms),
      MustNotHave = _textProcessor.NormalizeTerms(mustNotHaveTerms)
    };
  }
}
