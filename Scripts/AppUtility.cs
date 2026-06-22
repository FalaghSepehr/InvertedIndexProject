using PorterStemmer.Stemmers;

namespace InvertedIndex_Program;

public static class AppUtility
{
    public static class Paths
    {
        public readonly static string projectDir = GetProjectDirectory();
        public readonly static string outputPath = Path.Combine(projectDir, "myOutputs/inverted_index.txt");
        public readonly static string[] documentPaths = GetDocumentPaths();

        private static string GetProjectDirectory()
        {
            var current = new DirectoryInfo(Environment.CurrentDirectory);
            while (current != null)
            {
                if (Directory.Exists(Path.Combine(current.FullName, "Documents")))
                    return current.FullName;
                current = current.Parent;
            }
            throw new InvalidOperationException("Could not locate project root directory containing 'Documents' folder.");
        }

        private static string[] GetDocumentPaths()
        {
            var docsPath = Path.Combine(projectDir, "Documents");
            if (!Directory.Exists(docsPath))
                throw new InvalidOperationException($"Documents directory not found at: {docsPath}");
            return Directory.GetFiles(docsPath);
        }
    }
    public static class TextProcessing
    {
        public readonly static char[] symbolsAndNumbers = File.ReadAllText(Path.Combine(Paths.projectDir, "AppConstants/symbolsAndNumbers")).Where(c => !char.IsWhiteSpace(c)).ToArray();
        public readonly static string[] stopWords = File.ReadAllText(Path.Combine(Paths.projectDir, "AppConstants/stopWords")).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        public static readonly EnglishStemmer Stemmer = new();
        public static string Stem(string word) => Stemmer.GetStem(word);
    }
}