using LogHub.Configuration;
using LogHub.Graphs;
using LogHub.Processing;
namespace LogHub.Application;
public sealed class LogHubApplication
{
    private readonly IConfigService _config;
    private readonly LogProcessor _processor;
    private readonly GraphDemo _graphDemo;
    private readonly PipelineReport _report;
    private readonly TextWriter _output;
    private readonly PipelineSinkLifetime? _sinks;
    public LogHubApplication(IConfigService config, LogProcessor processor, GraphDemo graphDemo,
        PipelineReport report, TextWriter output, PipelineSinkLifetime? sinks = null)
    {
        _config = config; _processor = processor; _graphDemo = graphDemo;
        _report = report; _output = output; _sinks = sinks;
    }
    public ProcessingResult Run(bool showGraph = false)
    {
        _output.WriteLine($"Importer: {_config.GetValue("importer")}; sinks: {_config.GetValue("sink")}");
        _output.WriteLine($"Filters: {_config.GetValue("filters")}; notifications: {_config.GetValue("notifications")}");
        _sinks?.Measurements.Clear();
        if (_config.GetValue("family") is string family && family != "legacy")
            _output.WriteLine($"Family: {family}; builder: {_config.GetValue("builder")}; adapter: {_config.GetValue("adapter")}");
        var result = _processor.Run();
        _sinks?.Flush();
        if (_sinks is not null) result = result with { NotificationErrors = _sinks.ErrorCount };
        _output.WriteLine($"Processed: {result.Written}; skipped: {result.Skipped}; observer errors: {result.NotificationErrors}");
        _report.Print(_output);
        if (_sinks is not null && _sinks.Measurements.Count > 0)
            _output.WriteLine($"Delivery measurements: {_sinks.Measurements.Count}");
        if (showGraph) _graphDemo.Print(_output);
        return result;
    }
}
