using System.Text.Json;
using LogHub.External;
using LogHub.Formatting;
using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Adapters;
public sealed class JsonObjectAdapter : ILogSink
{
    private readonly JsonLogApi _api;
    private readonly LogFormatter _formatter;
    public JsonObjectAdapter(JsonLogApi api, LogFormatter formatter) => (_api, _formatter) = (api, formatter);
    public string Name => "json-object";
    public void Write(LogRecord record, string text)
    {
        using var document = JsonDocument.Parse(_formatter.Format(record));
        _api.Push(document);
    }
    public void Dispose() => _api.Dispose();
}
