namespace InvertedIndexProgram.Tests;

public static class TestHelpers
{
  public static void AssertEqual<TKey, TValue>(Dictionary<TKey, TValue> expected, Dictionary<TKey, TValue> actual)
  {
    var missingInActual = expected.Keys.Except(actual.Keys).ToList();
    var extraInActual = actual.Keys.Except(expected.Keys).ToList();
    var valueMismatches = expected.Keys
        .Where(k => actual.ContainsKey(k) && !EqualityComparer<TValue>.Default.Equals(expected[k], actual[k]))
        .ToList();

    var errors = new List<string>();

    if (missingInActual.Count > 0)
      errors.Add($"Missing keys: {string.Join(", ", missingInActual)}");

    if (extraInActual.Count > 0)
      errors.Add($"Extra keys: {string.Join(", ", extraInActual)}");

    foreach (var key in valueMismatches)
      errors.Add($"Key [{key}]: Expected {expected[key]}, Actual {actual[key]}");

    if (errors.Count > 0)
      Assert.Fail(string.Join("\n", errors));
  }
}