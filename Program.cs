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
    public readonly static string[] symbols = File.ReadAllText(Path.Combine(projectDir, "AppConstants/symbols")).Split(' ', StringSplitOptions.RemoveEmptyEntries);
    public readonly static string[] stopWords = File.ReadAllText(Path.Combine(projectDir, "AppConstants/stopWords")).Split(' ', StringSplitOptions.RemoveEmptyEntries);
    public readonly static char[] numbers = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9'];
    public static string[] documentPaths = Directory.GetFiles(Path.Combine(projectDir, "Documents"));
}

class Program
{
    static void Main(string[] args)
    {
        var myInvertedIndex = new InvertedIndex(AppConstatnts.documentPaths);

        using StreamWriter writer = new StreamWriter(AppConstatnts.outputPath);
        foreach (var pair in myInvertedIndex.InvertedIndexDic)
        {
            writer.WriteLine($"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value)}");
        }
        Console.WriteLine($"Index written to {AppConstatnts.outputPath}");

        
        System.Console.Write("Search: ");
        System.Console.WriteLine(GetSearchResult(GetInput(), myInvertedIndex.InvertedIndexDic));
    }
    public static string GetSearchResult(string query, Dictionary<string, List<string>> invertedIndex)
    {
        if (query == "")
        {
            return "No results!";
        }

        var queryArray = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

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

        mustHaveTerms.RemoveAll(t => AppConstatnts.stopWords.Contains(t));
        atLeastOneTerms.RemoveAll(t => AppConstatnts.stopWords.Contains(t));
        mustNotHaveTerms.RemoveAll(t => AppConstatnts.stopWords.Contains(t));

        mustHaveTerms = mustHaveTerms.Select(t => StemmerHelper.Stem(t)).ToList();
        atLeastOneTerms = atLeastOneTerms.Select(t => StemmerHelper.Stem(t)).ToList();
        mustNotHaveTerms = mustNotHaveTerms.Select(t => StemmerHelper.Stem(t)).ToList();

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
                    mustHaveDocs = mustHaveDocs.Intersect(documents).ToList();
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

        if (mustHaveDocs.Count() == 0 && atLeastOneDocs.Count() == 0 && mustNotHaveDocs.Count() == 0)
        {
            if (mustNotHaveTerms.Count() != 0)
            {
                result = invertedIndex.Values.SelectMany(list => list).Distinct().ToList();
            }
        }
        else if (mustHaveDocs.Count() == 0 && atLeastOneDocs.Count() == 0 && mustNotHaveDocs.Count() != 0)
        {
            result = invertedIndex.Values.SelectMany(list => list).Distinct().Except(mustNotHaveDocs).ToList();
        }
        else if (mustHaveDocs.Count() != 0 && atLeastOneDocs.Count() != 0)
        {
            result = mustHaveDocs.Intersect(atLeastOneDocs).Except(mustNotHaveDocs).ToList();
        }
        else if (mustHaveDocs.Count() == 0 || atLeastOneDocs.Count() == 0)
        {
            result = mustHaveDocs.Union(atLeastOneDocs).Except(mustNotHaveDocs).ToList();
        }

        if (result.Count() == 0)
        {
            return "No results!";
        }
        else
        {
            return string.Join(", ", result);
        }
    }
    public static string GetInput()
    {
        string userInput = Console.ReadLine().Trim().ToLower();
        
        foreach (var p in AppConstatnts.symbols.Where(c => c != "+" && c != "-"))
        {
            userInput = userInput.Replace(p, " ");
        }
        foreach (var n in AppConstatnts.numbers)
        {
            userInput = userInput.Replace(n.ToString(), " ");
        }

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

            foreach (var p in AppConstatnts.symbols)
            {
                content = content.Replace(p, " ");
            }
            foreach (var n in AppConstatnts.numbers)
            {
                content = content.Replace(n.ToString(), " ");
            }

            List<string> terms = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

            terms.RemoveAll(t => t.Length < 3);

            foreach (var stopWord in AppConstatnts.stopWords)
            {
                terms.RemoveAll(t => t == stopWord);
            }

            terms = terms.Select(t => StemmerHelper.Stem(t)).ToList();

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

