public static class QueryParser
{
    public static List<List<string>> ParseQuery()
    {
        string query = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
        var queryArray = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var mustHaveTerms = new List<string>();
        var atLeastOneTerms = new List<string>();
        var mustNotHaveTerms = new List<string>();

        foreach (string item in queryArray)
        {
            switch (item[0])
            {
                case '+':
                    if (item.Length > 1) atLeastOneTerms.Add(item.Substring(1));
                    break;
                case '-':
                    if (item.Length > 1) mustNotHaveTerms.Add(item.Substring(1));
                    break;
                default:
                    mustHaveTerms.Add(item);
                    break;
            }
        }
        var queryBundle = new List<List<string>>() { mustHaveTerms, atLeastOneTerms, mustNotHaveTerms };
        for (int i = 0; i < queryBundle.Count; i++)
        {
            queryBundle[i] = queryBundle[i].FilterTerms();
        }
        return queryBundle;
    }
}