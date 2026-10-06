using LogHub.Models;

namespace LogHub.Sinks;
public interface ILogSink : IDisposable
{
    string Name { get; }
    void Write(LogRecord record, string text);
}