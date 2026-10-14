using LogHub.Catalog;
using LogHub.External;
using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Adapters;
public sealed class SqlObjectAdapter : ILogSink
{
    private readonly SqlLogApi _api;
    private readonly ISeverityCatalog _severities;
    public SqlObjectAdapter(SqlLogApi api, ISeverityCatalog severities)
        => (_api, _severities) = (api ?? throw new ArgumentNullException(nameof(api)),
            severities ?? throw new ArgumentNullException(nameof(severities)));
    public string Name => "sql-object";
    public void Write(LogRecord record, string text) => _api.Save(record.Date, _severities.GetCode(record.Severity), text);
    public void Dispose() => _api.Dispose();
}
