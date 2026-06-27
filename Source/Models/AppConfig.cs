namespace InvertedIndexProgram;
public record AppConfig
{
    public string DocumentsDir { get; init; }
    public string OutputPath { get; init; }
    public char[] SymbolsAndNumbers { get; init; }
    public HashSet<string> StopWords { get; init; }
}