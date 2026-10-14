using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Parsing;
namespace LogHub.Creation;
public sealed class MemoryImporterFactory : LogImporterFactory
{
    public override string Name => "memory";
    public override ILogImporter Create(ITextSource source, LogParser parser) => new InMemoryLogImporter(source, parser);
}
