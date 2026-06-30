namespace InvertedIndexProgram;

/// <summary>
/// Processes raw text into normalized, stemmed terms for indexing and search.
/// </summary>
public interface ITextProcessor
{
    /// <summary>
    /// Extracts normalized terms from raw text by tokenizing, cleaning symbols,
    /// removing stop words, and applying stemming.
    /// </summary>
    /// <param name="rawText">The raw text to process.</param>
    /// <returns>A collection of normalized, stemmed terms.</returns>
    List<string> ExtractTerms(string rawText);
    /// <summary>
    /// Normalizes a list of already-tokenized terms by cleaning symbols,
    /// removing stop words, and applying stemming. Does not tokenize.
    /// </summary>
    /// <param name="terms">The tokenized terms to normalize.</param>
    /// <returns>A collection of normalized, stemmed terms.</returns>
    List<string> NormalizeTerms(List<string> terms);
    /// <summary>
    /// Prepares raw input text by trimming, lowercasing, and tokenizing.
    /// Does not apply normalization or stemming.
    /// </summary>
    /// <param name="rawText">The raw input string.</param>
    /// <returns>A list of raw, lowercase tokens.</returns>
    List<string> PrepareTokens(string rawText);
}