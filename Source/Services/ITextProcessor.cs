namespace InvertedIndexProgram;
/// <summary>
/// Processes raw text into terms suitable for indexing and search.
/// Implementations define their own strategy for tokenization, normalization,
/// filtering, and stemming. Some may correct typos, preserve numbers, or apply
/// different linguistic rules.
/// </summary>
public interface ITextProcessor
{
    /// <summary>
    /// Extracts and normalizes terms from raw text using the implementation's processing pipeline.
    /// </summary>
    /// <param name="text">The raw text to process.</param>
    /// <returns>A collection of processed terms.</returns>
    List<string> ExtractTerms(string text);

    /// <summary>
    /// Normalizes already-tokenized terms using the implementation's processing pipeline.
    /// Does not perform tokenization.
    /// </summary>
    /// <param name="terms">The tokenized terms to normalize.</param>
    /// <returns>A collection of processed terms.</returns>
    List<string> NormalizeTerms(List<string> terms);

    /// <summary>
    /// Prepares raw input text for parsing by trimming, lowercasing, and tokenizing.
    /// Does not apply normalization or filtering.
    /// </summary>
    /// <param name="rawText">The raw input string.</param>
    /// <returns>A list of raw tokens.</returns>
    List<string> PrepareTokens(string rawText);
}