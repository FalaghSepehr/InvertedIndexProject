namespace InvertedIndexProgram;
/// <summary>
/// Builds and queries an inverted index from text documents.
/// Accepts an <see cref="ITextProcessor"/> for term extraction, enabling different text processing strategies.
/// </summary>
public class InvertedIndex
{
  private readonly Dictionary<string, HashSet<string>> _invertedIndexDic;
  public IReadOnlyDictionary<string, HashSet<string>> InvertedIndexDic => _invertedIndexDic;

  internal InvertedIndex(Dictionary<string, HashSet<string>> invertedIndexDic)
  {
    _invertedIndexDic = invertedIndexDic;
  }
  /// <summary>
  /// Builds an inverted index from the specified text documents using the given text processor.
  /// </summary>
  /// <param name="docPaths">Array of file paths to text documents.</param>
  /// <param name="textProcessor">The text processor for tokenizing and normalizing terms.</param>
  /// <returns>A fully built InvertedIndex ready for searching.</returns>
  public static InvertedIndex Build(string[] docPaths, ITextProcessor textProcessor)
  {
    var invertedIndexDic = new Dictionary<string, HashSet<string>>();

    foreach (string docFileDir in docPaths)
    {
      var fileName = Path.GetFileNameWithoutExtension(docFileDir);
      var content = File.ReadAllText(docFileDir);
      var terms = textProcessor.ExtractTerms(content);

      foreach (string term in terms)
      {
        if (!invertedIndexDic.TryGetValue(term, out var documents))
        {
          documents = new HashSet<string>();
          invertedIndexDic[term] = documents;
        }
        documents.Add(fileName);
      }
    }
    return new InvertedIndex(invertedIndexDic);
  }
  /// <summary>
  /// Exports the entire index to the specified output writer for debugging or external use.
  /// </summary>
  /// <param name="writer">The output writer to receive the formatted index.</param>
  public void ExportTo(IOutputWriter writer)
  {
    foreach (var pair in InvertedIndexDic)
    {
      writer.WriteLine(FormatEntry(pair));
    }
  }
  internal string FormatEntry(KeyValuePair<string, HashSet<string>> pair)
  {
    return $"\"{pair.Key}\":\n\t{string.Join(", ", pair.Value.OrderBy(v => v))}";
  }
}