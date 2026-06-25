using PorterStemmer.Stemmers;

namespace InvertedIndexProgram;
/// <summary>
/// Holds file paths and helper methods.
/// </summary>
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
        public readonly static char[] symbolsAndNumbers = LoadSymbolsAndNumbers();
        public readonly static HashSet<string> stopWords = LoadStopWords();
        private static readonly EnglishStemmer Stemmer = new();
        /// <summary>
        /// Stems an English word to its root form using the Porter algorithm
        /// (via the PorterStemmer NuGet package).
        /// </summary>
        /// <param name="word">The word to stem.</param>
        /// <returns>The stemmed word (e.g., "running" → "run").</returns>
        public static string Stem(string word) => Stemmer.GetStem(word);
        private static char[] LoadSymbolsAndNumbers()
        {
            var path = Path.Combine(Paths.projectDir, "AppConstants/symbolsAndNumbers.txt");
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Required file not found: {path}");
            }
            return File.ReadAllText(path).Where(c => !char.IsWhiteSpace(c)).ToArray();
        }

        private static HashSet<string> LoadStopWords()
        {
            var path = Path.Combine(Paths.projectDir, "AppConstants/stopWords.txt");
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Required file not found: {path}");
            }
            return new HashSet<string>(File.ReadAllText(path).Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
        }
    }
}