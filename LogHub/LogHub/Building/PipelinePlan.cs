using LogHub.Configuration;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Infrastructure;
using LogHub.Rules;
using LogHub.Sinks;
namespace LogHub.Building;
public sealed class PipelinePlan
{
    public LogConfiguration Configuration { get; }
    public IHubResources? Resources { get; }
    public ITextSource? Source { get; }
    public ILogImporter? Importer { get; }
    public ILogRule? Rule { get; }
    public IReadOnlyList<ILogSink> Sinks { get; }
    public PipelinePlan(LogConfiguration configuration, IHubResources? resources = null,
        ITextSource? source = null, ILogImporter? importer = null, ILogRule? rule = null,
        IEnumerable<ILogSink>? sinks = null)
    {
        Configuration = configuration.DeepClone(); Resources = resources; Source = source;
        Importer = importer; Rule = rule; Sinks = Array.AsReadOnly((sinks ?? Array.Empty<ILogSink>()).ToArray());
    }
}
