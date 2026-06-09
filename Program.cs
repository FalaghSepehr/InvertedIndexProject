using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PorterStemmer.Stemmers;

namespace InvertedIndex_Program;

public static class AppConstatnts
{
    public readonly static string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
    public readonly static string outputPath = Path.Combine(projectDir, "myOutputs/inverted_index.txt");
    public readonly static char[] symbolsAndNumbers = File.ReadAllText(Path.Combine(projectDir, "AppConstants/symbolsAndNumbers")).Where(c => !char.IsWhiteSpace(c)).ToArray();
    public readonly static string[] stopWords = File.ReadAllText(Path.Combine(projectDir, "AppConstants/stopWords")).Split(' ', StringSplitOptions.RemoveEmptyEntries);
    public static string[] documentPaths = Directory.GetFiles(Path.Combine(projectDir, "Documents"));
}

class Program
{
    static void Main(string[] args)
    {
        var myInvertedIndex = new InvertedIndex(AppConstatnts.documentPaths);
        StreamWriter writer = new StreamWriter(AppConstatnts.outputPath);
        foreach (var pair in myInvertedIndex.InvertedIndexDic)
        {
            writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value)}");
        }
        Console.WriteLine($"Index written to {AppConstatnts.outputPath}");
        Console.Write("Search: ");
        Console.WriteLine(GetSearchResult(GetQueryBundle(), myInvertedIndex.InvertedIndexDic));
    }
    public static List<List<string>> GetQueryBundle()
    {
        string query = Console.ReadLine().Trim().ToLower();
        var queryArray = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var mustHaveTerms = new List<string>();
        var atLeastOneTerms = new List<string>();
        var mustNotHaveTerms = new List<string>();

        foreach (string item in queryArray)
        {
            if (item[0] == '+')
            {
                atLeastOneTerms.Add(item.Substring(1));
            }
            else if (item[0] == '-')
            {
                mustNotHaveTerms.Add(item.Substring(1));
            }
            else
            {
                mustHaveTerms.Add(item);
            }
        }
        var queryBundle = new List<List<string>>() { mustHaveTerms, atLeastOneTerms, mustNotHaveTerms };
        for (int i = 0; i < queryBundle.Count; i++)
        {
            queryBundle[i] = queryBundle[i]
            .Select(word => AppConstatnts.symbolsAndNumbers.Aggregate(word, (current, nextChar) => current.Replace(nextChar, ' ')))
            .Where(word => !AppConstatnts.stopWords.Contains(word))
            .Select(StemmerHelper.Stem).ToList();
        }
        return queryBundle;
    }
    public static string GetSearchResult(List<List<string>> queryBundle, Dictionary<string, List<string>> invertedIndex)
    {
        var result = new List<string>();
        var mustHaveDocs = new List<string>();
        var atLeastOneDocs = new List<string>();
        var mustNotHaveDocs = new List<string>();

        var mustHaveTerms = queryBundle[0];
        var atLeastOneTerms = queryBundle[1];
        var mustNotHaveTerms = queryBundle[2];

        bool first = true;
        foreach (var term in mustHaveTerms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
            {
                if (first)
                {
                    mustHaveDocs = documents;
                    first = false;
                }
                else
                {
                    mustHaveDocs = mustHaveDocs.Intersect(documents).ToList();
                }
            }
            else
            {
                mustHaveDocs.Clear();
            }
        }
        foreach (var term in atLeastOneTerms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
            {
                atLeastOneDocs = atLeastOneDocs.Union(documents).ToList();
            }
        }
        foreach (var term in mustNotHaveTerms)
        {
            if (invertedIndex.TryGetValue(term, out var documents))
            {
                mustNotHaveDocs = mustNotHaveDocs.Union(documents).ToList();
            }
        }

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

        if (result.Count == 0)
        {
            return "No results!";
        }
        else
        {
            return string.Join(", ", result);
        }
    }

}
public class InvertedIndex
{
    public Dictionary<string, List<string>> InvertedIndexDic { get; set; } = new();
    public InvertedIndex(string[] fileDirectories)
    {

        foreach (string docFileDir in fileDirectories)
        {
            string fileName = Path.GetFileNameWithoutExtension(docFileDir);
            string content = File.ReadAllText(docFileDir).ToLower().Trim();

            foreach (var p in AppConstatnts.symbolsAndNumbers)
            {
                content = content.Replace(p, ' ');
            }

            List<string> terms = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            terms.RemoveAll(t => t.Length < 3);

            foreach (var stopWord in AppConstatnts.stopWords)
            {
                terms.RemoveAll(t => t == stopWord);
            }

            terms = terms.Select(StemmerHelper.Stem).ToList();

            foreach (string term in terms)
            {
                if (!InvertedIndexDic.ContainsKey(term))
                {
                    InvertedIndexDic[term] = new List<string>();
                }

                if (!InvertedIndexDic[term].Contains(fileName))
                {
                    InvertedIndexDic[term].Add(fileName);
                }
            }
        }
    }
}

public static class StemmerHelper
{
    public static readonly EnglishStemmer Stemmer = new();

    public static string Stem(string word)
    {
        return Stemmer.GetStem(word);
    }
}

