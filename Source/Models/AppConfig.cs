namespace InvertedIndexProgram;
/// <summary>
/// Holds configuration settings loaded from appsettings.json at startup.
/// </summary>
public record AppConfig
{
    /// <summary>Path to the documents folder.</summary>
    public string DocumentsDir { get; init; }

    /// <summary>Path where the exported index file is written.</summary>
    public string OutputPath { get; init; }

    /// <summary>Characters to remove during text processing.</summary>
    public char[] SymbolsAndNumbers { get; init; }
    
    /// <summary>Words excluded from indexing and search.</summary>
    public HashSet<string> StopWords { get; init; }
}