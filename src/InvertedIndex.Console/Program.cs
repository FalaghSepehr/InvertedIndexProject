using InvertedIndex.Core;
using InvertedIndex.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InvertedIndex.Console;

class Program
{
  private static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
  public static void Main(string[] args)
  {
    var config = LoadConfig();

    var services = new ServiceCollection();

    services.AddSingleton(config);
    services.AddSingleton<IOutputWriter, ConsoleOutputWriter>();
    services.AddSingleton<IInputReader, ConsoleInputReader>();
    services.AddSingleton<ITextProcessor>(new SimpleTextProcessor(config.SymbolsAndNumbers, config.StopWords));
    services.AddSingleton(sp =>
    {
      var textProcessor = sp.GetRequiredService<ITextProcessor>();
      return InvertedIndexBuilder.Build(GetDocumentPathsArray(config.DocumentsDir), textProcessor);
    });
    services.AddSingleton<Searcher>();
    services.AddSingleton<ISearchService>(sp =>
    {
      var invertedIndex = sp.GetRequiredService<InvertedIndexBuilder>();
      var searcher = sp.GetRequiredService<Searcher>();
      return new SearchService(searcher, invertedIndex.InvertedIndexDic);
    });
    services.AddSingleton<IQueryParser, QueryParser>();
    services.AddSingleton<ConsoleUI>();

    var provider = services.BuildServiceProvider();

    var invertedIndex = provider.GetRequiredService<InvertedIndexBuilder>();
    var consoleUI = provider.GetRequiredService<ConsoleUI>();

    var fileWriter = new FileOutputWriter(config.OutputPath);

    invertedIndex.ExportTo(fileWriter);

    var consoleWriter = provider.GetRequiredService<IOutputWriter>();
    consoleWriter.WriteLine($"Index written to {config.OutputPath}");

    consoleUI.Run(invertedIndex.IsEmpty);
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
    var files = Directory.GetFiles(docsDir);
    if (files.Length == 0)
    {
      throw new InvalidOperationException($"No documents found in: {docsDir}");
    }
    return files;
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
