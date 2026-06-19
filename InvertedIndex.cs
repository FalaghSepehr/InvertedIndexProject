public class InvertedIndex
{
    public Dictionary<string, List<string>> InvertedIndexDic { get; set; } = new();
    public InvertedIndex(string[] fileDirectories)
    {
        foreach (string docFileDir in fileDirectories)
        {
            string fileName = Path.GetFileNameWithoutExtension(docFileDir);
            string content = File.ReadAllText(docFileDir).ToLower().Trim();
            List<string> terms = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList().FilterTerms();

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
        foreach (var term in InvertedIndexDic.Keys)
        {
            InvertedIndexDic[term].Sort();
        }
    }
}