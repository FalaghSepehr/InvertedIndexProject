using Xunit;

namespace InvertedIndexProgram.Tests;

public static class TestHelpers
{
  public static void AssertEqual(IReadOnlyDictionary<string, HashSet<string>> expected, IReadOnlyDictionary<string, HashSet<string>> actual)
  {
    var missingInActual = expected.Keys.Except(actual.Keys).ToList();
    var extraInActual = actual.Keys.Except(expected.Keys).ToList();
    var errors = new List<string>();

    if (missingInActual.Count > 0)
      errors.Add($"Missing keys: {string.Join(", ", missingInActual)}");

    if (extraInActual.Count > 0)
      errors.Add($"Extra keys: {string.Join(", ", extraInActual)}");

    foreach (var key in expected.Keys)
    {
      if (actual.ContainsKey(key))
      {
        var expectedValues = string.Join(", ", expected[key].OrderBy(v => v));
        var actualValues = string.Join(", ", actual[key].OrderBy(v => v));
        if (expectedValues != actualValues)
          errors.Add($"Key [{key}]: Expected [{expectedValues}], Actual [{actualValues}]");
      }
    }

    if (errors.Count > 0)
      Assert.Fail(string.Join("\n", errors));
  }
}