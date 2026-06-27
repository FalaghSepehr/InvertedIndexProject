namespace InvertedIndexProgram;
public record QueryBundle
{
    public List<string> MustHave { get; init; }
    public List<string> AtLeastOne { get; init; }
    public List<string> MustNotHave { get; init; }
}