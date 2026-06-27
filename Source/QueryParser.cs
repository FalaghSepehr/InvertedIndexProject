namespace InvertedIndexProgram;

public class QueryParser
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