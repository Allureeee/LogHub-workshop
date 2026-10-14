using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Parsing;
namespace LogHub.Creation;
public sealed class JsonImporterFactory : LogImporterFactory
{
    public override string Name => "json";
    public override ILogImporter Create(ITextSource source, LogParser parser) => new JsonLogImporter(source, parser);
}
