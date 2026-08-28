using InvertedIndex.Core;
using InvertedIndex.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var solutionRoot = GetSolutionRoot();

var config = builder.Configuration;
var symbolsPath = Path.Combine(solutionRoot, config["FilePaths:SymbolsPath"] ?? "");
var stopWordsPath = Path.Combine(solutionRoot, config["FilePaths:StopWordsPath"] ?? "");
var DocumentsDir = Path.Combine(solutionRoot, config["FilePaths:DocumentsPath"] ?? "");

builder.Services.AddOpenApi();

builder.Services.AddSingleton<ITextProcessor>(new SimpleTextProcessor(LoadSymbols(symbolsPath), LoadStopWords(stopWordsPath)));

// inverted index
builder.Services.AddSingleton(sp =>
{
  var textProcessor = sp.GetRequiredService<ITextProcessor>();
  return InvertedIndexBuilder.Build(GetDocumentPathsArray(DocumentsDir), textProcessor);
});

// search dependencies
builder.Services.AddSingleton<Searcher>();
builder.Services.AddSingleton<ISearchService>(sp =>
{
  var invertedIndex = sp.GetRequiredService<InvertedIndexBuilder>();
  var searcher = sp.GetRequiredService<Searcher>();
  return new SearchService(searcher, invertedIndex.InvertedIndexDic);
});

builder.Services.AddSingleton<IQueryParser, QueryParser>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseHttpsRedirection();

app.Run();

static char[] LoadSymbols(string path)
{
  if (!File.Exists(path))
  {
    throw new InvalidOperationException($"Required file not found: {path}");
  }
  return File.ReadAllText(path).Where(c => !char.IsWhiteSpace(c)).ToArray();
}
static HashSet<string> LoadStopWords(string path)
{
  if (!File.Exists(path))
  {
    throw new InvalidOperationException($"Required file not found: {path}");
  }
  return new HashSet<string>(File.ReadAllText(path).Split(' ', StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
}

static string GetSolutionRoot()
{
  var current = new DirectoryInfo(Environment.CurrentDirectory);
  while (current != null)
  {
    if (Directory.EnumerateFiles(current.FullName, "*.slnx").Any())
    {
      return current.FullName;
    }

    current = current.Parent;
  }
  throw new InvalidOperationException("Could not locate solution root containing a .slnx file.");
}

static string[] GetDocumentPathsArray(string docsDir)
{
  if (!Directory.Exists(docsDir))
  {
    throw new InvalidOperationException($"Documents directory not found at: {docsDir}");
  }
  var files = Directory.GetFiles(docsDir);
  if (files.Length == 0)
  {
    throw new InvalidOperationException($"No documents found in: {docsDir}");
  }
  return files;
}