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
        
        Directory.CreateDirectory(Path.GetDirectoryName(AppUtility.Paths.outputPath));
        using (var writer = new StreamWriter(AppUtility.Paths.outputPath))
        {
            foreach (var pair in myInvertedIndex.IndexDic)
            {
                writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value.OrderBy(v => v))}");
            }
        }
        Console.WriteLine($"Index written to {AppUtility.Paths.outputPath}");

        ConsoleUI.Run(myInvertedIndex);
    }
     
}