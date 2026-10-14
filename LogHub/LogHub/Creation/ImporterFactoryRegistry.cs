namespace LogHub.Creation;
public sealed class ImporterFactoryRegistry
{
    private readonly IReadOnlyDictionary<string, LogImporterFactory> _factories;
    public ImporterFactoryRegistry(IEnumerable<LogImporterFactory> factories)
    {
        ArgumentNullException.ThrowIfNull(factories);
        _factories = factories.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
    }
    public LogImporterFactory Get(string name) => _factories.TryGetValue(name, out var factory)
        ? factory : throw new NotSupportedException($"Unknown importer '{name}'.");
}
