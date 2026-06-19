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
        Console.WriteLine(GetSearchResult(ParseQueryBundle(), myInvertedIndex.InvertedIndexDic));
    }
    public static List<List<string>> ParseQueryBundle()
    {
        string query = Console.ReadLine().Trim().ToLower();
        var queryArray = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var mustHaveTerms = new List<string>();
        var atLeastOneTerms = new List<string>();
        var mustNotHaveTerms = new List<string>();

        foreach (string item in queryArray)
        {
            switch (item[0])
            {
                case '+':
                    atLeastOneTerms.Add(item.Substring(1));
                    break;
                case '-':
                    mustNotHaveTerms.Add(item.Substring(1));
                    break;
                default:
                    mustHaveTerms.Add(item);
                    break;
            }
        }
        var queryBundle = new List<List<string>>() { mustHaveTerms, atLeastOneTerms, mustNotHaveTerms };
        for (int i = 0; i < queryBundle.Count; i++)
        {
            queryBundle[i] = queryBundle[i].FilterTerms();
        }
        return queryBundle;
    }
    public static string GetSearchResult(List<List<string>> queryBundle, Dictionary<string, List<string>> invertedIndex)
    {
        var mustHaveTerms = queryBundle[0];
        var atLeastOneTerms = queryBundle[1];
        var mustNotHaveTerms = queryBundle[2];

        var mustHaveDocs = IntersectTermDocs(mustHaveTerms, invertedIndex);
        var atLeastOneDocs = UnionTermDocs(atLeastOneTerms, invertedIndex);
        var mustNotHaveDocs = UnionTermDocs(mustNotHaveTerms, invertedIndex);

        var result = BuildResult(mustNotHaveTerms, mustHaveDocs, atLeastOneDocs, mustNotHaveDocs, invertedIndex);
        if (result.Count == 0)
        {
            return "No results!";
        }
        return string.Join(", ", result);
    }
    private static List<string> IntersectTermDocs(List<string> terms, Dictionary<string, List<string>> invertedIndex)
    {
        var result = new List<string>();

        bool first = true;
        foreach (var term in terms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
            {
                if (first)
                {
                    result = documents;
                    first = false;
                }
                else
                {
                    result = result.Intersect(documents).ToList();
                }
            }
            else
            {
                result.Clear();
            }
        }
        return result;
    }
    private static List<string> UnionTermDocs(List<string> terms, Dictionary<string, List<string>> invertedIndex)
    {
        var result = new List<string>();
        foreach (var term in terms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
                result.AddRange(documents);
        }
        return result.Distinct().ToList();
    }
    private static List<string> BuildResult(List<string> mustNotHaveTerms, List<string> mustHaveDocs, List<string> atLeastOneDocs, List<string> mustNotHaveDocs, Dictionary<string, List<string>> invertedIndex)
    {
        var result = new List<string>();
        if (mustHaveDocs.Count == 0 && atLeastOneDocs.Count == 0 && mustNotHaveDocs.Count == 0)
        {
            if (mustNotHaveTerms.Count != 0)
            {
                result = invertedIndex.Values.SelectMany(list => list).Distinct().ToList();
            }
        }
        else if (mustHaveDocs.Count == 0 && atLeastOneDocs.Count == 0 && mustNotHaveDocs.Count != 0)
        {
            result = invertedIndex.Values.SelectMany(list => list).Distinct().Except(mustNotHaveDocs).ToList();
        }
        else if (mustHaveDocs.Count != 0 && atLeastOneDocs.Count != 0)
        {
            result = mustHaveDocs.Intersect(atLeastOneDocs).Except(mustNotHaveDocs).ToList();
        }
        else if (mustHaveDocs.Count == 0 || atLeastOneDocs.Count == 0)
        {
            result = mustHaveDocs.Union(atLeastOneDocs).Except(mustNotHaveDocs).ToList();
        }
        return result;
    }
}