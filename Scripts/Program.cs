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
        
        using (var writer = new StreamWriter(AppUtility.Paths.outputPath))
        {
            foreach (var pair in myInvertedIndex.indexDic)
            {
                writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value)}");
            }
        }
        Console.WriteLine($"Index written to {AppUtility.Paths.outputPath}");

        ConsoleUI.Run(myInvertedIndex);
    }
     
}