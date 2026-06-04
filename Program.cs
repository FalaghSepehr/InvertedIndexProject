using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualBasic;

namespace InvertedIndex_Program;

public static class AppConstatnts
{
    public readonly static char[] punctuation = { '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']' };

}

class Program
{
    
    static void Main(string[] args)
    {
        Console.Write("Search: ");
        string userInput = Console.ReadLine().Trim().ToLower();

        foreach (char p in AppConstatnts.punctuation)
            userInput = userInput.Replace(p.ToString(), "");

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
        var mustHaveTerms = new List<string>();
        var atLeastOneTerms = new List<string>();
        var mustNotHaveTerms = new List<string>();

        var mustHaveDocs = new List<string>();
        var atLeastOneDocs = new List<string>();
        var mustNotHaveDocs = new List<string>();

        foreach (string item in queryArray)
        {
            if (item[0] == '+')
                atLeastOneTerms.Add(item.Substring(1));
            else if (item[0] == '-')
                mustNotHaveTerms.Add(item.Substring(1));
            else
                mustHaveTerms.Add(item);
        }
        bool first = true;
        foreach (var term in mustHaveTerms)
        {   
            if (invertedIndex.TryGetValue(term, out var documents))
                if (first)
                    mustHaveDocs = documents;
                else
                    mustHaveDocs = mustHaveDocs.Intersect(documents).ToList();
            else
                mustHaveDocs.Clear();
        }
        foreach (var term in atLeastOneTerms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
                atLeastOneDocs = atLeastOneDocs.Union(documents).ToList();
        }
        foreach (var term in mustNotHaveTerms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
                mustNotHaveDocs = mustNotHaveDocs.Union(documents).ToList();
        }

        return mustHaveDocs.Intersect(atLeastOneDocs).Except(mustHaveDocs).ToList();
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

