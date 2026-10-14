using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Families;
public sealed class FamilyLogSink : ILogSink
{
    private readonly FamilyEndpoint _endpoint;
    private readonly FamilyFormatter _formatter;
    public FamilyLogSink(FamilyEndpoint endpoint, FamilyFormatter formatter)
    {
        if (endpoint.Family != formatter.Family) throw new ArgumentException("Incompatible endpoint and format.");
        _endpoint = endpoint; _formatter = formatter;
    }
    public string Name => _endpoint.Name;
    public void Write(LogRecord record, string text) => _endpoint.Write(_formatter.Format(record));
    public void Dispose() => _endpoint.Dispose();
}
