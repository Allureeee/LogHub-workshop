using System.Text.Json;
using LogHub.External;
using LogHub.Formatting;
using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Adapters;
public sealed class JsonClassAdapter : JsonLogApi, ILogSink
{
    private readonly LogFormatter _formatter;
    public JsonClassAdapter(IDocumentChannel channel, LogFormatter formatter) : base(channel) => _formatter = formatter;
    public string Name => "json-class";
    public void Write(LogRecord record, string text)
    {
        using var document = JsonDocument.Parse(_formatter.Format(record));
        Push(document);
    }
}
