namespace LogHub.Catalog;
public interface ISeverityCatalog
{
    int GetCode(string name);
}
public sealed class SeverityCatalog : ISeverityCatalog
{
    private static readonly IReadOnlyDictionary<string, int> Codes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["Debug"] = 0, ["Info"] = 1, ["Warning"] = 2, ["Error"] = 3, ["Critical"] = 4
    };
    public int GetCode(string name) => Codes.TryGetValue(name, out var code)
        ? code : throw new ArgumentException($"Unknown severity '{name}'.", nameof(name));
}
