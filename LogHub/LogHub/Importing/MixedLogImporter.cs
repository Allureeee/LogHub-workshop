using LogHub.Importing.Sources;
using LogHub.Models;
using LogHub.Parsing;
namespace LogHub.Importing;

public sealed class MixedLogImporter : LogImporterBase
{
    private static readonly HashSet<string> Levels = new(StringComparer.OrdinalIgnoreCase)
        { "Debug", "Info", "Warning", "Error", "Critical" };
    private readonly LogParser _parser;
    public MixedLogImporter(ITextSource source, LogParser parser) : base(source)
    {
        ArgumentNullException.ThrowIfNull(parser);
        _parser = parser;
    }
    protected override string? Read(TextReader reader) => reader.ReadLine();
    protected override LogRecord? Parse(string line) => _parser.Parse(line);
    protected override bool Validate(LogRecord record) =>
        Levels.Contains(record.Severity) && !string.IsNullOrWhiteSpace(record.Message);
}
