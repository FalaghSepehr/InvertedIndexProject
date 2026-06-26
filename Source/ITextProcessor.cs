namespace InvertedIndexProgram;

public interface ITextProcessor
{
    IEnumerable<string> Tokenize(string text);
    IEnumerable<string> FilterTerms(IEnumerable<string> terms);
    string Stem(string word);
}