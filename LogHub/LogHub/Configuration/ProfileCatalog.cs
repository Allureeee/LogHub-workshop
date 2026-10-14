namespace LogHub.Configuration;
public sealed class ProfileCatalog
{
    private readonly Dictionary<string, LogConfiguration> _prototypes = new(StringComparer.OrdinalIgnoreCase);
    public void Register(string name, LogConfiguration prototype)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(prototype);
        _prototypes[name] = prototype.DeepClone();
    }
    public LogConfiguration Create(string name) => _prototypes.TryGetValue(name, out var prototype)
        ? prototype.DeepClone() : throw new KeyNotFoundException($"Unknown profile '{name}'.");
    public static LogConfiguration Default() => BuiltIn("development");
    public static LogConfiguration BuiltIn(string name)
    {
        var catalog = new ProfileCatalog();
        var development = new LogConfiguration
        {
            Family = "development", Sink = "console,file", Importer = "mixed",
            LogPath = Path.Combine(AppContext.BaseDirectory, "data", "pipeline-variant20.txt"),
            OutputPath = Path.Combine(AppContext.BaseDirectory, "output", "development.log"),
            DatabasePath = Path.Combine(AppContext.BaseDirectory, "output", "loghub.sqlite"),
            DocumentPath = Path.Combine(AppContext.BaseDirectory, "output", "documents.jsonl"),
            ReferenceTimeUtc = new DateTimeOffset(2026, 10, 7, 12, 0, 0, TimeSpan.Zero),
            Delivery = new DeliveryOptions { Decorators = new() { "buffer", "retry", "timing" }, BatchSize = 7 }
        };
        catalog.Register("development", development);
        var production = development.DeepClone();
        production.Family = "production";
        production.OutputPath = Path.Combine(AppContext.BaseDirectory, "output", "production.log");
        catalog.Register("production", production);
        return catalog.Create(name);
    }
}
