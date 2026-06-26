namespace InvertedIndexProgram;

public interface ITextProcessor
{
    List<string> ExtractTerms(string text);
    IEnumerable<string> FilterTerms(IEnumerable<string> terms);
}