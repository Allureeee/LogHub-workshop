using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Parsing;
namespace LogHub.Creation;

public abstract class LogImporterFactory
{
    public abstract string Name { get; }
    public abstract ILogImporter Create(ITextSource source, LogParser parser);
}
