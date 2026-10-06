using System.Collections.Generic;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Reading;
using LogHub.Sinks;

namespace LogHub.Processing;
public class LogProcessingModule
{
    private readonly LogFileReader _reader;
    private readonly LogParser _parser;
    private readonly SeverityFilter _filter;
    private readonly LogFormatter _formatter;
    private readonly ILogSink _sink;
    public LogProcessingModule(
        LogFileReader reader,
        LogParser parser,
        SeverityFilter filter,
        LogFormatter formatter,
        ILogSink sink)
    {
        _reader = reader;
        _parser = parser;
        _filter = filter;
        _formatter = formatter;
        _sink = sink;
    }
    public ProcessingResult Run(string logPath)
    {
        return Process(_reader.Read(logPath));
    }
    public ProcessingResult Run(IEnumerable<string> lines)
    {
        return Process(lines);
    }
    private ProcessingResult Process(IEnumerable<string> lines)
    {
        var written = 0;
        var skipped = 0;
        foreach (var line in lines)
        {
            var record = _parser.Parse(line);
            if (record is null)
            {
                skipped++;
                continue;
            }
            if (!_filter.ShouldInclude(record))
            {
                skipped++;
                continue;
            }
            var text = _formatter.Format(record);
            _sink.Write(record, text);
            written++;
        }
        Console.WriteLine(
            $"Записано: {written}, пропущено: {skipped}");
        return new ProcessingResult(written, skipped);
    }
}
public readonly record struct ProcessingResult(
    int Written,
    int Skipped);