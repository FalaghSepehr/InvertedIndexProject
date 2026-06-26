global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
using Microsoft.Extensions.Configuration;

namespace InvertedIndexProgram;

class Program
{
    private static readonly string ProjectDir = GetProjectDirectory();
    static void Main(string[] args)
    {
        var config = LoadConfig();

        ITextProcessor simpleTextProcessor = new SimpleTextProcessor(config.SymbolsAndNumbers, config.StopWords);
        var invertedIndex = new InvertedIndex(GetDocumentPathsArray(config.DocumentsDir), simpleTextProcessor);

        Directory.CreateDirectory(Path.GetDirectoryName(config.OutputPath));
        using (var writer = new StreamWriter(config.OutputPath))
        {
            foreach (var pair in invertedIndex.InvertedIndexDic)
            {
                writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value.OrderBy(v => v))}");
            }
        }
        Console.WriteLine($"Index written to {config.OutputPath}");

        var queryParser = new QueryParser(simpleTextProcessor);
        var consoleUI = new ConsoleUI(invertedIndex, queryParser);
        consoleUI.Run();
    }
    private static string GetProjectDirectory()
    {
        var current = new DirectoryInfo(Environment.CurrentDirectory);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "Documents")))
                return current.FullName;
            current = current.Parent;
        }
        throw new InvalidOperationException("Could not locate project root directory containing 'Documents' folder.");
    }
    private static string[] GetDocumentPathsArray(string docsDir)
    {
        if (!Directory.Exists(docsDir))
        {
            throw new InvalidOperationException($"Documents directory not found at: {docsDir}");
        }
        return Directory.GetFiles(docsDir);
    }
    private static char[] LoadSymbolsAndNumbers(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Required file not found: {path}");
        }
        return File.ReadAllText(path).Where(c => !char.IsWhiteSpace(c)).ToArray();
    }
    private static HashSet<string> LoadStopWords(string path)
    {
        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"Required file not found: {path}");
        }
        return new HashSet<string>(File.ReadAllText(path).Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
    }
    private static AppConfig LoadConfig()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(ProjectDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var appSettings = configuration.GetSection("AppSettings");
        
        return new AppConfig
        {
            DocumentsDir = appSettings["DocumentsPath"],
            OutputPath = appSettings["OutputPath"],
            SymbolsAndNumbers = LoadSymbolsAndNumbers(appSettings["SymbolsAndNumbersPath"]),
            StopWords = LoadStopWords(appSettings["StopWordsPath"])
        };
    }
}
record AppConfig
{
    public string DocumentsDir { get; init; }
    public string OutputPath { get; init; }
    public char[] SymbolsAndNumbers { get; init; }
    public HashSet<string> StopWords { get; init; }
}