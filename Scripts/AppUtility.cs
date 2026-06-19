using PorterStemmer.Stemmers;
public static class AppUtility
{
    public static class Paths
    {
        public readonly static string projectDir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public readonly static string outputPath = Path.Combine(projectDir, "myOutputs/inverted_index.txt");
        public static string[] documentPaths = Directory.GetFiles(Path.Combine(projectDir, "Documents"));
    }
    public static class TextProcessing
    {
        public readonly static char[] symbolsAndNumbers = File.ReadAllText(Path.Combine(Paths.projectDir, "AppConstants/symbolsAndNumbers")).Where(c => !char.IsWhiteSpace(c)).ToArray();
        public readonly static string[] stopWords = File.ReadAllText(Path.Combine(Paths.projectDir, "AppConstants/stopWords")).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        public static readonly EnglishStemmer Stemmer = new();
        public static string Stem(string word) => Stemmer.GetStem(word);
    }
    
}