namespace InvertedIndexProgram.Tests;

public class SimpleTextProcessorTests
{
  // static fields are safe here because SimpleTextProcessor has immutable state.
  private static readonly SimpleTextProcessor _bareProcessor = new SimpleTextProcessor([], []);
  private static readonly SimpleTextProcessor _processor = new(['.'], ["the"]);

  [Fact]
  public void SplitOnSpaces_SingleSpace_SplitsCorrectly()
  {
    var result = _bareProcessor.SplitOnSpaces("hello world");
    Assert.Equal(["hello", "world"], result);
  }

  [Fact]
  public void Stem_ReturnsStemmedWord()
  {
    var result = _bareProcessor.Stem("running");
    Assert.Equal("run", result);
  }

  public class ExtractTerms
  {
    [Fact]
    public void ReturnsNormalizedTerms()
    {
      var result = _processor.ExtractTerms("  The cat is running.  ");
      Assert.Equal(["cat", "run"], result);
    }

    [Fact]
    public void HandlesAllWhitespace()
    {
      var result = _bareProcessor.ExtractTerms("hello\tworld\nfoo\rbar");
      Assert.Equal(["hello", "world", "foo", "bar"], result);
    }

    [Fact]
    public void SplitsOnSymbolsWithinTerms()
    {
      var result = _processor.ExtractTerms("cat.dog");
      Assert.Equal(["cat", "dog"], result);
    }
  }

  public class CleanSymbolsAndNumbers
  {
    [Fact]
    public void ReplacesSymbolsWithSpace()
    {
      var result = _processor.CleanSymbolsAndNumbers(".hello.world.");
      Assert.Equal("hello world", result);
    }

    [Fact]
    public void ReplacesNumbersWithSpace()
    {
      var result = _bareProcessor.CleanSymbolsAndNumbers("12hello5world0");
      Assert.Equal("hello world", result);
    }
  }
  public class IsIndexable
  {
    [Fact]
    public void ReturnsFalse_if_length_less_than_3()
    {
      var result = _processor.IsIndexable("ab");
      Assert.False(result);
    }

    [Fact]
    public void ReturnsFalse_if_stop_word()
    {
      var result = _processor.IsIndexable("the");
      Assert.False(result);
    }

    [Fact]
    public void ReturnsTrue_if_length_more_than_2_and_not_stop_word()
    {
      var result = _processor.IsIndexable("hello");
      Assert.True(result);
    }
  }
}
