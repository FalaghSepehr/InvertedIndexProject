namespace InvertedIndexProgram;

public interface ITextProcessor
{
    List<string> FilterTerms(List<string> terms);
    string Stem(string word);
}