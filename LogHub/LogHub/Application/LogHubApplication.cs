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
    public LogHubApplication(IConfigService config, LogProcessor processor, GraphDemo graphDemo,
        PipelineReport report, TextWriter output)
    {
        _config = config; _processor = processor; _graphDemo = graphDemo;
        _report = report; _output = output;
    }
    public ProcessingResult Run(bool showGraph = false)
    {
        _output.WriteLine($"Importer: {_config.GetValue("importer")}; sinks: {_config.GetValue("sink")}");
        _output.WriteLine($"Filters: {_config.GetValue("filters")}; notifications: {_config.GetValue("notifications")}");
        var result = _processor.Run();
        _output.WriteLine($"Processed: {result.Written}; skipped: {result.Skipped}; observer errors: {result.NotificationErrors}");
        _report.Print(_output);
        if (showGraph) _graphDemo.Print(_output);
        return result;
    }
}
