using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

namespace InvertedIndex_Program;


class Program
{
    readonly char[] punctuation = { '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']' };
    
    static void Main(string[] args)
    {
        

        Console.Write("Search: ");
        string searchedWord = Console.ReadLine().Trim().ToLower();

        // Handles punctuations in the searched word:
        foreach (char p in punctuation)
            searchedWord = searchedWord.Replace(p.ToString(), "");

        // File directory handling
        string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        string documentsPath = Path.Combine(projectDir, "Documents");
        string[] txtFilesDirs = Directory.GetFiles(documentsPath, "*.txt");

        // Show Inverted Index:
        Console.WriteLine("======================\nHere's the inverted index: ");
        foreach (var pair in InvertedIndex(txtFilesDirs))
        {
            Console.WriteLine($"{pair.Key}: {string.Join(", ", pair.Value)}");
        }
        Console.WriteLine("======================");
        
    }
    public static List<string> SearchDocuments(string query, Dictionary<string, List<string>> invertedIndex)
    {
        var queryArray = query.Split();

        var result = new List<string>();
        var mustHave = new List<string>();
        var atLeastOne = new List<string>();
        var mustNotHave = new List<string>();

        foreach (string item in queryArray)
        {
            if (item[0] == '+')
                atLeastOne.Add(item.Substring(1));
            else if (item[0] == '-')
                mustNotHave.Add(item.Substring(1));
            else
                mustHave.Add(item);
        }

        foreach (var term in mustHave)
        {
            if (invertedIndex.TryGetValue(term, out var value))
                result.AddRange(value);
        }

        return result;
    }

    public static Dictionary<string, List<string>> InvertedIndex(string[] fileDirectories)
    {
        var invertedIndex = new Dictionary<string, List<string>>();
        char[] punctuation = { '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']' };

        foreach (string txtFileDir in fileDirectories)
        {
            string fileName = Path.GetFileNameWithoutExtension(txtFileDir);
            string content = File.ReadAllText(txtFileDir).ToLower().Trim();

            // Handles punctuations in the documents:
            foreach (char p in punctuation)
                content = content.Replace(p, ' ');

            // Handles multiple spaces in the documents (and the punctuatuins that got repleaced with ' '):
            string[] terms = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string term in terms)
            {
                if (!invertedIndex.ContainsKey(term))
                    invertedIndex[term] = new List<string>();

                if (!invertedIndex[term].Contains(fileName))
                    invertedIndex[term].Add(fileName);
            }
        }

        return invertedIndex;


    }

}

