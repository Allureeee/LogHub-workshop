using LogHub.Configuration;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Infrastructure;
using LogHub.Rules;
using LogHub.Sinks;
namespace LogHub.Building;
public sealed class PipelineBuilder
{
    private LogConfiguration _configuration = new();
    private readonly IHubResources? _resources;
    private ITextSource? _source;
    private ILogImporter? _importer;
    private ILogRule? _rule;
    private readonly List<ILogSink> _sinks = new();
    private bool _sourceSet, _destinationsSet, _built;
    public PipelineBuilder(IHubResources? resources = null) => _resources = resources;
    private void Check() { if (_built) throw new InvalidOperationException("This builder has already produced a pipeline."); }
    public PipelineBuilder Configure(LogConfiguration configuration)
    { Check(); ArgumentNullException.ThrowIfNull(configuration); _configuration = configuration.DeepClone(); return this; }
    public PipelineBuilder FromFile(string path, string importer = "mixed")
    {
        Check(); ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _configuration.LogPath = path; _configuration.Importer = importer;
        _source = null; _importer = null; _sourceSet = true; return this;
    }
    public PipelineBuilder FromSource(ITextSource source, string importer = "mixed")
    {
        Check(); _source = source ?? throw new ArgumentNullException(nameof(source));
        _configuration.Importer = importer; _importer = null; _sourceSet = true; return this;
    }
    public PipelineBuilder From(ILogImporter importer)
    { Check(); _importer = importer ?? throw new ArgumentNullException(nameof(importer)); _sourceSet = true; return this; }
    public PipelineBuilder FilterBySeverity(string minimum)
    {
        Check(); _configuration.MinSeverity = minimum;
        var names = _configuration.Filters.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!names.Contains("severity", StringComparer.OrdinalIgnoreCase)) names.Insert(0, "severity");
        _configuration.Filters = string.Join(',', names); return this;
    }
    public PipelineBuilder FilterWith(ILogRule rule)
    { Check(); _rule = rule ?? throw new ArgumentNullException(nameof(rule)); return this; }
    public PipelineBuilder UseConfiguredOutputs()
    { Check(); _destinationsSet = true; return this; }
    public PipelineBuilder WriteTo(ILogSink sink)
    {
        Check(); ArgumentNullException.ThrowIfNull(sink);
        if (_sinks.Contains(sink)) throw new ArgumentException("The same sink cannot be owned twice.", nameof(sink));
        _sinks.Add(sink); _destinationsSet = true; return this;
    }
    public BuiltPipeline Build()
    {
        Check();
        if (!_sourceSet) throw new InvalidOperationException("Missing source: call FromFile, FromSource or From.");
        if (!_destinationsSet) throw new InvalidOperationException("Missing output: call WriteTo or UseConfiguredOutputs.");
        var result = CompositionRoot.Build(new PipelinePlan(_configuration, _resources, _source, _importer, _rule, _sinks));
        _built = true;
        return result;
    }
}
