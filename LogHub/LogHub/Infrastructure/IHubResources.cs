using LogHub.Configuration;
using LogHub.External;
using LogHub.Importing.Sources;
using LogHub.Sinks;
namespace LogHub.Infrastructure;
public interface IHubResources
{
    TextWriter Output { get; }
    TimeProvider Clock { get; }
    ITextSource CreateSource(string path);
    ILogSink CreateConsole(LogConfiguration configuration);
    ILogSink CreateFile(LogConfiguration configuration);
    ISqlSession CreateSqlSession(string path);
    IDocumentChannel CreateDocumentChannel(string path);
}
