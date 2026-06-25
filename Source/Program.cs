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
        var configuration = new ConfigurationBuilder()
            .SetBasePath(ProjectDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var appSettings = configuration.GetSection("AppSettings");
        var docsDir = appSettings["DocumentsPath"];
        var outputPath = appSettings["OutputPath"];
        var symbolsAndNumbers = LoadSymbolsAndNumbers(appSettings["SymbolsAndNumbersPath"]);
        var stopWords = LoadStopWords(appSettings["StopWordsPath"]);

        ITextProcessor simpleTextProcessor = new SimpleTextProcessor(symbolsAndNumbers, stopWords);
        var invertedIndex = new InvertedIndex(GetDocumentPathsArray(docsDir), simpleTextProcessor);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
        using (var writer = new StreamWriter(outputPath))
        {
            foreach (var pair in invertedIndex.IndexDic)
            {
                writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value.OrderBy(v => v))}");
            }
        }
        Console.WriteLine($"Index written to {outputPath}");

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
}