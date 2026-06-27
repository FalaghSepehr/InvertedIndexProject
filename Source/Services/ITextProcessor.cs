namespace InvertedIndexProgram;

public interface ITextProcessor
{
    List<string> ExtractTerms(string text);
    List<string> NormalizeTerms(List<string> terms);
    List<string> Tokenize(string text);
    List<string> PrepareTokens(string text);
}