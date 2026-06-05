using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace InvertedIndex_Program;

public static class AppConstatnts
{
    public readonly static char[] punctuation = { '.', ',', ';', ':', '!', '?', '"', '\'', '(', ')', '[', ']' };

}

class Program
{
    static void Main(string[] args)
    {
        var myInvertedIndex = new InvertedIndex(GetDocumentsPaths());

        Console.WriteLine("======================\nHere's the inverted index: ");
        foreach (var pair in myInvertedIndex.InvertedIndexDic)
        {
            Console.WriteLine($"{pair.Key}: {string.Join(", ", pair.Value)}");
        }
        Console.WriteLine("======================");

        System.Console.Write("Search: ");
        System.Console.WriteLine(GetSearchResult(GetInput(), myInvertedIndex.InvertedIndexDic));
    }
    public static string[] GetDocumentsPaths()
    {
        string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        string documentsPath = Path.Combine(projectDir, "Documents");
        return Directory.GetFiles(documentsPath, "*.txt");
    }
    public static string GetSearchResult(string query, Dictionary<string, List<string>> invertedIndex)
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

        result = mustHaveDocs.Intersect(atLeastOneDocs).Except(mustNotHaveDocs).ToList();

        if (result.Count() == 0)
            return "No results!";
        else
        {
            return string.Join(", ", result);
        }
    }
    public static string GetInput()
    {
        string userInput = Console.ReadLine().Trim().ToLower();

        foreach (char p in AppConstatnts.punctuation)
            userInput = userInput.Replace(p.ToString(), "");

        return userInput;

    }
}
public class InvertedIndex
{
    public Dictionary<string, List<string>> InvertedIndexDic { get; set; } = new();
    public InvertedIndex(string[] fileDirectories)
    {

        foreach (string txtFileDir in fileDirectories)
        {
            string fileName = Path.GetFileNameWithoutExtension(txtFileDir);
            string content = File.ReadAllText(txtFileDir).ToLower().Trim();

            // Handles punctuations in the documents:
            foreach (char p in AppConstatnts.punctuation)
                content = content.Replace(p, ' ');

            // Handles multiple spaces in the documents (and the punctuatuins that got repleaced with ' '):
            string[] terms = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (string term in terms)
            {
                if (!InvertedIndexDic.ContainsKey(term))
                    InvertedIndexDic[term] = new List<string>();

                if (!InvertedIndexDic[term].Contains(fileName))
                    InvertedIndexDic[term].Add(fileName);
            }
        }
    }
}
