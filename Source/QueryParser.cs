namespace InvertedIndexProgram;
/// <summary>
/// Handles search query input.
/// Supports stemming, stop-word removal ,symbol and number removal.
/// Supports prefix-based search operators (+, -).
/// </summary>
public class QueryParser
{
    private readonly IInputReader _inputReader;
    private readonly ITextProcessor _textProcessor;
    
    public QueryParser(ITextProcessor textProcessor, IInputReader inputReader)
    {
        _inputReader = inputReader;
        _textProcessor = textProcessor;
    }
    /// <summary>
    /// Reads a query from the console and parses it into categorized term lists.
    /// Words prefixed with '+' are at-least-one terms, '-' are must-not-have terms,
    /// and bare words are must-have terms. Each list is then filtered through
    /// stemming, stop word removal, and symbol and number cleanup.
    /// </summary>
    /// <returns>
    /// A query bundle list containing three filtered term lists:
    /// <list type="bullet">
    /// <item><description>[0] Must-have terms — all must be present in a matching document.</description></item>
    /// <item><description>[1] At-least-one terms — at least one must be present.</description></item>
    /// <item><description>[2] Must-not-have terms — documents containing any of these are excluded.</description></item>
    /// </list>
    /// </returns>
    public QueryBundle ParseQuery()
    {
        var queryArray = GetQuerryArray();
        var mustHaveTerms = new List<string>();
        var atLeastOneTerms = new List<string>();
        var mustNotHaveTerms = new List<string>();

        foreach (string item in queryArray)
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
            MustHave = _textProcessor.NormalizeTerms(mustHaveTerms).ToList(),
            AtLeastOne = _textProcessor.NormalizeTerms(atLeastOneTerms).ToList(),
            MustNotHave = _textProcessor.NormalizeTerms(mustNotHaveTerms).ToList()
        };
    }
    private string[] GetQuerryArray()
    {
        string query = _inputReader.ReadLine().Trim().ToLower();
        return query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
public record QueryBundle
{
    public List<string> MustHave { get; init; }
    public List<string> AtLeastOne { get; init; }
    public List<string> MustNotHave { get; init; }
}