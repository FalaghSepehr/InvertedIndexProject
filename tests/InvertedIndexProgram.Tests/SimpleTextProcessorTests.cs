namespace InvertedIndexProgram.Tests;

public class SimpleTextProcessorTests
{
  // static fields are safe here because SimpleTextProcessor has immutable state.
  private static readonly SimpleTextProcessor _bareProcessor = new SimpleTextProcessor([], []);
  private static readonly SimpleTextProcessor _processor = new(['.'], ["the"]);

  public class ExtractTerms
  {
    [Fact]
    public void HandlesStopWordsSymbolsAndNumbers()
    {
      var result = _processor.ExtractTerms("  The cat is running. 2 ");
      Assert.Equal(["cat", "run"], result);
    }

    [Fact]
    public void HandlesAllWhitespace()
    {
      var result = _bareProcessor.ExtractTerms("hello\tworld\nfoo\rbar");
      Assert.Equal(["hello", "world", "foo", "bar"], result);
    }

    [Fact]
    public void SplitsOnSymbolsAndNumberWithinTerms()
    {
      var result = _processor.ExtractTerms("cat.2dog");
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

  [Fact]
  public void GetRawTokens_SplitsOnAllWhiteSpace()
  {
    var result = _bareProcessor.GetRawTokens("hello\tworld\nfoo\rbar goo");

    Assert.Equal(["hello", "world", "foo", "bar", "goo"], result);
  }

  [Fact]
  public void GetRawTokens_DoesNotCareAboutSymbolsAndNumbers()
  {
    var result = _processor.GetRawTokens("hello2.");

    Assert.Equal(["hello2."], result);
  }

  [Fact]
  public void GetRawTokens_DoesNotCareAboutStopWords()
  {
    var result = _processor.GetRawTokens("hello the");

    Assert.Equal(["hello", "the"], result);
  }

  [Fact]
  public void GetRawTokens_DoesNotStem()
  {
    var result = _bareProcessor.GetRawTokens("running");
    Assert.Equal(["running"], result);
  }
}
