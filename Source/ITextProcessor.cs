namespace InvertedIndexProgram;

public interface ITextProcessor
{
    List<string> ExtractTerms(string text);
    IEnumerable<string> NormalizeTerms(IEnumerable<string> terms);
}