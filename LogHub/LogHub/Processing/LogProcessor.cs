using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Sinks;
namespace LogHub.Processing;

public sealed class LogProcessor
{
    private ILogImporter _importer;
    private readonly SeverityFilter _filter;
    private readonly LogFormatter _formatter;
    private readonly ILogSink[] _sinks;
    public LogProcessor(ILogImporter importer, SeverityFilter filter,
        LogFormatter formatter, IEnumerable<ILogSink> sinks)
    {
        ArgumentNullException.ThrowIfNull(importer);
        ArgumentNullException.ThrowIfNull(filter);
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(sinks);
        _importer = importer;
        _filter = filter;
        _formatter = formatter;
        _sinks = sinks.ToArray();
        if (_sinks.Length == 0)
            throw new ArgumentException("At least one sink is required.", nameof(sinks));
    }

    public void ChangeImporter(ILogImporter importer)
    {
        ArgumentNullException.ThrowIfNull(importer);
        _importer = importer;
    }

    public ProcessingResult Run()
    {
        var written = 0;
        var skipped = 0;
        foreach (var item in _importer.Import())
        {
            var record = item.Record;
            if (record is null || !_filter.ShouldInclude(record))
            {
                skipped++;
                continue;
            }
            var text = _formatter.Format(record);
            foreach (var sink in _sinks)
                sink.Write(record, text);
            written++;
        }
        return new ProcessingResult(written, skipped);
    }
}
