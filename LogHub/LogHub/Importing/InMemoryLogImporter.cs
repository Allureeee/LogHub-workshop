using LogHub.Importing.Sources;
using LogHub.Models;
using LogHub.Parsing;
namespace LogHub.Importing;

public sealed class InMemoryLogImporter : LogImporterBase
{
    private readonly LogParser _parser;
    public InMemoryLogImporter(ITextSource source, LogParser parser) : base(source)
    {
        ArgumentNullException.ThrowIfNull(parser);
        _parser = parser;
    }
    protected override string? Read(TextReader reader) => reader.ReadLine();
    protected override LogRecord? Parse(string line) => _parser.Parse(line);
}
