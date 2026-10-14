using LogHub.Configuration;
using LogHub.External;
using LogHub.Importing.Sources;
using LogHub.Sinks;
namespace LogHub.Infrastructure;
public sealed class PhysicalHubResources : IHubResources
{
    public TextWriter Output => Console.Out;
    public TimeProvider Clock => TimeProvider.System;
    public ITextSource CreateSource(string path) => new FileTextSource(path);
    public ILogSink CreateConsole(LogConfiguration configuration) => new ConsoleLogSink(configuration, Output);
    public ILogSink CreateFile(LogConfiguration configuration) => new FileLogSink(configuration);
    public ISqlSession CreateSqlSession(string path) => new SqliteLogSession(path);
    public IDocumentChannel CreateDocumentChannel(string path) => new JsonFileChannel(path);
}
