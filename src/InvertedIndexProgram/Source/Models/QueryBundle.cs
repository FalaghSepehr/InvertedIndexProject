namespace InvertedIndexProgram;
/// <summary>
/// Holds the three categories of terms parsed from a user query:
/// must-have, at-least-one, and must-not-have.
/// </summary>
public record QueryBundle
{
    /// <summary>Terms that must all be present in matching documents.</summary>
    public List<string> MustHave { get; init; }

    /// <summary>Terms where at least one must be present in matching documents.</summary>
    public List<string> AtLeastOne { get; init; }

    /// <summary>Terms that must not be present in matching documents.</summary>
    public List<string> MustNotHave { get; init; }
}