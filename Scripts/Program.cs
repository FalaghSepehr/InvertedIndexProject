global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;

namespace InvertedIndex_Program;

class Program
{
    static void Main(string[] args)
    {
        var myInvertedIndex = new InvertedIndex(AppUtility.Paths.documentPaths);
        StreamWriter writer = new StreamWriter(AppUtility.Paths.outputPath);
        foreach (var pair in myInvertedIndex.InvertedIndexDic)
        {
            writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value)}");
        }
        Console.WriteLine($"Index written to {AppUtility.Paths.outputPath}");
        Console.Write("Search: ");
        Console.WriteLine(InvertedIndex.GetSearchResult(QueryParser.ParseQueryBundle(), myInvertedIndex.InvertedIndexDic));
    }
}