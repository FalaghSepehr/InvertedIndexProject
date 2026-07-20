using InvertedIndex.Core;

namespace InvertedIndex.Infrastructure;
/// <summary>
/// Parses a raw query string into a <see cref="QueryBundle"/> by categorizing terms
/// by prefix: + (at-least-one), - (must-not-have), and bare (must-have).
/// Each term is normalized using the injected <see cref="ITextProcessor"/>.
/// If the term is in double quotes it wont get normalized.
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
    var rawInputText = _inputReader.ReadLine();

    var queryTokens = _textProcessor.GetRawTokens(rawInputText);

    var categorizedTokens = Categorize(queryTokens);

    var result = NormalizeNonExactTokens(categorizedTokens);

    return result;
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
      MustHave = mustHaveTerms,
      AtLeastOne = atLeastOneTerms,
      MustNotHave = mustNotHaveTerms
    };
  }

  internal QueryBundle NormalizeNonExactTokens(QueryBundle bundle)
  {
    return new QueryBundle
    {
      MustHave = NormalizeCategory(bundle.MustHave),
      AtLeastOne = NormalizeCategory(bundle.AtLeastOne),
      MustNotHave = NormalizeCategory(bundle.MustNotHave)
    };
  }

  private List<string> NormalizeCategory(List<string> terms)
  {
    var exact = new List<string>();
    var nonExact = new List<string>();

    foreach (var term in terms)
    {
      if (term.StartsWith('"') && term.EndsWith('"') && term.Length > 1)
      {
        exact.Add(term.Substring(1, term.Length - 2)); // strip quotes
      }
      else
      {
        nonExact.Add(term);
      }
    }

    if (nonExact.Count > 0)
    {
      var normalized = _textProcessor.NormalizeTerms(nonExact);
      exact.AddRange(normalized);
    }
    return exact;
  }
}
