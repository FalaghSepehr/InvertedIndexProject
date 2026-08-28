using Microsoft.AspNetCore.Mvc;
using InvertedIndex.Core;

namespace InvertedIndex.Api;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
  private readonly ISearchService _searchService;
  private readonly IQueryParser _queryParser;
  public SearchController(ISearchService searchService, IQueryParser queryParser)
  {
    _searchService = searchService;
    _queryParser = queryParser;
  }

  [HttpGet]
  public ActionResult<IEnumerable<string>> Search(string query)
  {
    if (string.IsNullOrWhiteSpace(query))
    {
      return BadRequest("Query cannot be empty");
    }

    var results = _searchService.Search(_queryParser.ParseQuery(query));

    if (results.Count == 0)
    {
      return NotFound("No matching documents");
    }

    return Ok(results);
  }
}