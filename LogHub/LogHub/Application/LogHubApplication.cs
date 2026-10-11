using LogHub.Configuration;
using LogHub.Graphs;
using LogHub.Processing;
namespace LogHub.Application;
public sealed class LogHubApplication
{
    private readonly IConfigService _config;
    private readonly LogProcessor _processor;
    private readonly GraphDemo _graphDemo;
    private readonly TextWriter _output;
    public LogHubApplication(IConfigService config, LogProcessor processor,
        GraphDemo graphDemo, TextWriter output)
    {
        _config = config;
        _processor = processor;
        _graphDemo = graphDemo;
        _output = output;
    }
    public ProcessingResult Run(bool showGraph = false)
    {
        _output.WriteLine($"Importer: {_config.GetValue("importer")}; sinks: {_config.GetValue("sink")}");
        var result = _processor.Run();
        _output.WriteLine($"Written: {result.Written}; skipped: {result.Skipped}");
        if (showGraph) _graphDemo.Print(_output);
        return result;
    }
}
