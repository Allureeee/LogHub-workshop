using System.Globalization;
namespace LogHub.Configuration;
public class ConfigReader
{
    public LogConfiguration Load(string path)
    {
        var fullPath = Path.GetFullPath(path);
        return Parse(File.ReadLines(fullPath), Path.GetDirectoryName(fullPath)!);
    }
    public LogConfiguration Parse(IEnumerable<string> lines, string baseDirectory)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["sink"] = "console,file", ["importer"] = "mixed", ["minSeverity"] = "Info",
            ["format"] = "plain", ["connection"] = "local", ["logPath"] = "sample.log",
            ["outputPath"] = string.Empty, ["filters"] = "severity,age,duplicates",
            ["maxAgeMinutes"] = "60", ["notifications"] = "push", ["bufferCapacity"] = "4",
            ["referenceTimeUtc"] = string.Empty,
            ["family"] = "legacy", ["builder"] = "fluent", ["adapter"] = "object",
            ["databasePath"] = "../output/loghub.sqlite", ["documentPath"] = "../output/documents.jsonl",
            ["decorators"] = string.Empty, ["batchSize"] = "4", ["retryAttempts"] = "3", ["rule"] = string.Empty
        };
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            var parts = line.Split('=', 2);
            if (parts.Length != 2) throw new ArgumentException($"Invalid configuration line: {raw}");
            values[parts[0].Trim()] = parts[1].Trim();
        }
        DateTimeOffset? reference = null;
        if (!string.IsNullOrWhiteSpace(values["referenceTimeUtc"]))
        {
            if (!DateTimeOffset.TryParse(values["referenceTimeUtc"], CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
                throw new ArgumentException("referenceTimeUtc must be an ISO date/time.");
            reference = date;
        }
        var notifications = values["notifications"].ToLowerInvariant();
        if (notifications != "push" && notifications != "pull")
            throw new ArgumentException("notifications must be push or pull.");
        return new LogConfiguration
        {
            Sink = values["sink"], Importer = values["importer"], MinSeverity = values["minSeverity"],
            Format = values["format"], Connection = values["connection"],
            LogPath = Path.GetFullPath(values["logPath"], baseDirectory),
            OutputPath = string.IsNullOrWhiteSpace(values["outputPath"]) ? string.Empty
                : Path.GetFullPath(values["outputPath"], baseDirectory),
            Filters = values["filters"], Notifications = notifications, ReferenceTimeUtc = reference,
            MaxAgeMinutes = Number(values, "maxAgeMinutes", 0),
            BufferCapacity = Number(values, "bufferCapacity", 1),
            Family = Choice(values, "family", "legacy", "development", "production"),
            Builder = Choice(values, "builder", "fluent", "classic"),
            Adapter = Choice(values, "adapter", "object", "class"),
            DatabasePath = Path.GetFullPath(values["databasePath"], baseDirectory),
            DocumentPath = Path.GetFullPath(values["documentPath"], baseDirectory),
            RuleJson = values["rule"],
            Delivery = new DeliveryOptions
            {
                Decorators = values["decorators"].Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList(),
                BatchSize = Number(values, "batchSize", 1),
                Retry = new RetryOptions { Attempts = Number(values, "retryAttempts", 1) }
            }
        };
    }
    private static string Choice(IReadOnlyDictionary<string, string> values, string key, params string[] allowed)
    {
        var value = values[key].ToLowerInvariant();
        return allowed.Contains(value, StringComparer.Ordinal) ? value
            : throw new ArgumentException($"{key} must be one of: {string.Join(", ", allowed)}.");
    }
    private static int Number(IReadOnlyDictionary<string, string> values, string key, int minimum)
    {
        if (!int.TryParse(values[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            || number < minimum) throw new ArgumentException($"{key} must be an integer >= {minimum}.");
        return number;
    }
}
