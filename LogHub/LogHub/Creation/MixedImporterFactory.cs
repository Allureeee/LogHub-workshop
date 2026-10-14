using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Parsing;
namespace LogHub.Creation;
public sealed class MixedImporterFactory : LogImporterFactory
{
    public override string Name => "mixed";
    public override ILogImporter Create(ITextSource source, LogParser parser) => new MixedLogImporter(source, parser);
}
