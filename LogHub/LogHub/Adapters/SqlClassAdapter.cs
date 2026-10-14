using LogHub.Catalog;
using LogHub.External;
using LogHub.Models;
using LogHub.Sinks;
namespace LogHub.Adapters;
public sealed class SqlClassAdapter : SqlLogApi, ILogSink
{
    private readonly ISeverityCatalog _severities;
    public SqlClassAdapter(ISqlSession session, ISeverityCatalog severities) : base(session)
        => _severities = severities ?? throw new ArgumentNullException(nameof(severities));
    public string Name => "sql-class";
    public void Write(LogRecord record, string text) => Save(record.Date, _severities.GetCode(record.Severity), text);
}
