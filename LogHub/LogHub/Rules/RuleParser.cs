using System.Text.Json;
using LogHub.Catalog;
namespace LogHub.Rules;
public sealed class RuleParser
{
    private readonly ISeverityCatalog _catalog;
    public RuleParser(ISeverityCatalog catalog) => _catalog = catalog;
    public ILogRule Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new And();
        try
        {
            using var document = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 32 });
            return Read(document.RootElement);
        }
        catch (JsonException error) { throw new ArgumentException("rule must be a valid JSON expression.", nameof(json), error); }
    }
    private ILogRule Read(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty("op", out var operation)
            || operation.ValueKind != JsonValueKind.String)
            throw new ArgumentException("Every rule must have a string 'op'.");
        var op = operation.GetString();
        return op switch
        {
            "and" => new And(Children(element)),
            "or" => new Or(Children(element)),
            "not" => new Not(Read(Child(element))),
            "severity" => new SeverityAtLeast(Value(element), _catalog),
            "message" => new MessageContains(Value(element)),
            _ => throw new ArgumentException($"Unknown rule operation '{op}'.")
        };
    }
    private ILogRule[] Children(JsonElement element)
    {
        if (!element.TryGetProperty("children", out var children) || children.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("and/or require a 'children' array.");
        return children.EnumerateArray().Select(Read).ToArray();
    }
    private static JsonElement Child(JsonElement element) => element.TryGetProperty("child", out var child)
        ? child : throw new ArgumentException("not requires a 'child'.");
    private static string Value(JsonElement element)
    {
        if (!element.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.String)
            throw new ArgumentException("A leaf rule requires a string 'value'.");
        return value.GetString()!;
    }
}
