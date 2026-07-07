global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
using Microsoft.Extensions.Configuration;

namespace InvertedIndexProgram;

class Program
{
  private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
  public static void Main(string[] args)
  {
    var config = LoadConfig();

    IOutputWriter consoleOutputWriter = new ConsoleOutputWriter();
    IOutputWriter fileOutputWriter = new FileOutputWriter(config.OutputPath);
    IInputReader consoleInputReader = new ConsoleInputReader();
    ITextProcessor simpleTextProcessor = new SimpleTextProcessor(config.SymbolsAndNumbers, config.StopWords);

    var invertedIndex = InvertedIndex.Build(GetDocumentPathsArray(config.DocumentsDir), simpleTextProcessor);
    ISearchService searhcer = new Searcher(invertedIndex.InvertedIndexDic);
    IQueryParser queryParser = new QueryParser(simpleTextProcessor, consoleInputReader);

    var consoleUI = new ConsoleUI(searhcer, queryParser, consoleInputReader, consoleOutputWriter);

    invertedIndex.ExportTo(fileOutputWriter);
    consoleOutputWriter.WriteLine($"Index written to {config.OutputPath}");

    consoleUI.Run();
  }
  /// <summary>
  /// Gets seperate documnet paths stored in the given directory.
  /// </summary>
  /// <param name="docsDir">The folder in which the documents are stored</param>
  /// <returns>Array of document paths</returns>
  /// <exception cref="InvalidOperationException"></exception>
  internal static string[] GetDocumentPathsArray(string docsDir)
  {
    if (!Directory.Exists(docsDir))
    {
      throw new InvalidOperationException($"Documents directory not found at: {docsDir}");
    }
    return Directory.GetFiles(docsDir);
  }
  internal static char[] LoadSymbols(string path)
  {
    if (!File.Exists(path))
    {
      throw new InvalidOperationException($"Required file not found: {path}");
    }
    return File.ReadAllText(path).Where(c => !char.IsWhiteSpace(c)).ToArray();
  }
  internal static HashSet<string> LoadStopWords(string path)
  {
    if (!File.Exists(path))
    {
      throw new InvalidOperationException($"Required file not found: {path}");
    }
    return new HashSet<string>(File.ReadAllText(path).Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
  }
  /// <summary>
  /// Provides the user's config loaded from appsettings.json
  /// </summary>
  /// <returns></returns>
  internal static AppConfig LoadConfig()
  {
    var configuration = new ConfigurationBuilder()
        .SetBasePath(BaseDir)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var appSettings = configuration.GetSection("AppSettings");

    return new AppConfig
    {
      DocumentsDir = Path.Combine(BaseDir, appSettings["DocumentsPath"]),
      OutputPath = Path.Combine(BaseDir, appSettings["OutputPath"]),
      SymbolsAndNumbers = LoadSymbols(Path.Combine(BaseDir, appSettings["SymbolsPath"])),
      StopWords = LoadStopWords(Path.Combine(BaseDir, appSettings["StopWordsPath"]))
    };
  }
}
