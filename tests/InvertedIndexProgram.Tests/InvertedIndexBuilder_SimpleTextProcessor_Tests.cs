namespace InvertedIndexProgram.Tests;

public class InvertedIndexBuilder_SimpleTextProcessor_Tests
{
  private readonly IOutputWriter _outputWriter;
  private readonly SimpleTextProcessor _simpleTextProcessor;
  public InvertedIndexBuilder_SimpleTextProcessor_Tests()
  {
    _outputWriter = Substitute.For<IOutputWriter>();
    _simpleTextProcessor = new SimpleTextProcessor(['.'], ["the", "is", "after"]);
  }

  // integration tests:
  [Fact]
  public void Build_CreatesIndexFromSingleFile()
  {
    var tempDocumentsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDocumentsDir);
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc1"), "cat dog");

    var sut = InvertedIndexBuilder.Build(Directory.GetFiles(tempDocumentsDir), _simpleTextProcessor);

    var expectedDictionary = new Dictionary<string, HashSet<string>>
    {
      ["cat"] = ["doc1"],
      ["dog"] = ["doc1"]
    };

    Assert.Equal(expectedDictionary, sut.InvertedIndexDic);

    Directory.Delete(tempDocumentsDir, true);
  }

  [Fact]
  public void Build_CreatesIndexFromMultipleFiles()
  {
    var tempDocumentsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDocumentsDir);
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc1"), "dog bird");
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc2"), "cat dog");
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc3"), "bird cat");

    var sut = InvertedIndexBuilder.Build(Directory.GetFiles(tempDocumentsDir), _simpleTextProcessor);

    var expectedDictionary = new Dictionary<string, HashSet<string>>
    {
      ["cat"] = ["doc2", "doc3"],
      ["dog"] = ["doc1", "doc2"],
      ["bird"] = ["doc1", "doc3"]
    };

    Assert.Equal(expectedDictionary, sut.InvertedIndexDic);

    Directory.Delete(tempDocumentsDir, true);
  }

  [Fact]
  public void Build_IndexesBothRawTokensAndProcessedTokens()
  {
    var tempDocumentsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDocumentsDir);
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc1"), "The Cats\tRunning dog\nhello\rMac");

    var sut = InvertedIndexBuilder.Build(Directory.GetFiles(tempDocumentsDir), _simpleTextProcessor);

    var expectedDictionary = new Dictionary<string, HashSet<string>>
    {
      // raw terms:
      ["The"] = ["doc1"],
      ["Cats"] = ["doc1"],
      ["Running"] = ["doc1"],
      ["dog"] = ["doc1"],
      ["hello"] = ["doc1"],
      ["Mac"] = ["doc1"],

      //non-duplicate processed terms:
      ["cat"] = ["doc1"],
      ["run"] = ["doc1"],
      ["mac"] = ["doc1"],
    };

    TestHelpers.AssertEqual(expectedDictionary, sut.InvertedIndexDic);

    Directory.Delete(tempDocumentsDir, true);
  }

  [Fact]
  public void Build_StoresBigramsForPhraseSearch()
  {
    var tempDocumentsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDocumentsDir);
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc1"), "star academy is great");

    var sut = InvertedIndexBuilder.Build(Directory.GetFiles(tempDocumentsDir), _simpleTextProcessor);

    var expectedDictionary = new Dictionary<string, HashSet<string>>
    {
      // Single tokens
      ["star"] = ["doc1"],
      ["academy"] = ["doc1"],
      ["is"] = ["doc1"],
      ["great"] = ["doc1"],
      // Bigrams
      ["star academy"] = ["doc1"],
      ["academy is"] = ["doc1"],
      ["is great"] = ["doc1"],
      // Processed tokens (stemmed)
      ["academi"] = ["doc1"]
    };

    TestHelpers.AssertEqual(expectedDictionary, sut.InvertedIndexDic);

    Directory.Delete(tempDocumentsDir, true);
  }

  //edge case
  [Fact]
  public void Build_CreatesEmptyIndex_when_file_has_no_content()
  {
    var tempDocumentsDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    Directory.CreateDirectory(tempDocumentsDir);
    File.WriteAllText(Path.Combine(tempDocumentsDir, "doc1"), "");

    var sut = InvertedIndexBuilder.Build(Directory.GetFiles(tempDocumentsDir), _simpleTextProcessor);

    var expectedDictionary = new Dictionary<string, HashSet<string>>([]);

    Assert.Equal(expectedDictionary, sut.InvertedIndexDic);

    Directory.Delete(tempDocumentsDir, true);
  }

  // unit tests:
  [Fact]
  public void ExportTo_FormatsEachPairAndWrites()
  {
    var invertedIndexDic = new Dictionary<string, HashSet<string>>
    {
      ["cat"] = ["doc1", "doc2"],
      ["dog"] = ["doc3", "doc2"],
      ["bird"] = ["doc3", "doc1"]
    };

    var sut = new InvertedIndexBuilder(invertedIndexDic);

    sut.ExportTo(_outputWriter);

    _outputWriter.Received().WriteLine("\"cat\":\n\tdoc1, doc2");
    _outputWriter.Received().WriteLine("\"dog\":\n\tdoc2, doc3");
    _outputWriter.Received().WriteLine("\"bird\":\n\tdoc1, doc3");
  }

  //edge case
  [Fact]
  public void ExportTo_WritesEmptyMessage_when_dictionary_empty()
  {
    var invertedIndexDic = new Dictionary<string, HashSet<string>>([]);

    var sut = new InvertedIndexBuilder(invertedIndexDic);

    sut.ExportTo(_outputWriter);

    _outputWriter.Received().WriteLine("Empty Inverted_Index");
  }

  [Fact]
  public void FormatEntry_FormatsPairWithOrderedDocs()
  {
    var sut = new InvertedIndexBuilder([]);

    var pair = new KeyValuePair<string, HashSet<string>>("cat", ["doc3", "doc1", "doc2"]);

    var result = sut.FormatEntry(pair);

    Assert.Equal("\"cat\":\n\tdoc1, doc2, doc3", result);
  }
}