using LogHub.Configuration;
using LogHub.Models;
namespace LogHub.Sinks;
[SinkName("console")]
public sealed class ConsoleLogSink : ILogSink
{
    private readonly TextWriter _output;
    public ConsoleLogSink(LogConfiguration configuration) : this(configuration, Console.Out) { }
    public ConsoleLogSink(LogConfiguration configuration, TextWriter output) => _output = output;
    public string Name => "console";
    public void Write(LogRecord record, string text) => _output.WriteLine(text);
    public void Dispose() { }
}
