using LogHub.Configuration;
using LogHub.Infrastructure;
namespace LogHub.Building;
public sealed class StandardPipelineBuilder : IPipelineConstruction
{
    private readonly IHubResources? _resources;
    private PipelineBuilder? _builder;
    private LogConfiguration? _configuration;
    private bool _processingSet;
    public StandardPipelineBuilder(IHubResources? resources = null) => _resources = resources;
    public void Reset(LogConfiguration configuration)
    {
        _configuration = configuration.DeepClone();
        _builder = new PipelineBuilder(_resources); _processingSet = false;
    }
    private PipelineBuilder Builder => _builder ?? throw new InvalidOperationException("Call Reset before construction.");
    private LogConfiguration Configuration => _configuration ?? throw new InvalidOperationException("No configuration.");
    public void BuildSource() => Builder.FromFile(Configuration.LogPath, Configuration.Importer);
    public void BuildProcessing() { Builder.Configure(Configuration); _processingSet = true; }
    public void BuildOutputs() => Builder.UseConfiguredOutputs();
    public BuiltPipeline GetResult()
    {
        if (!_processingSet) throw new InvalidOperationException("Missing processing configuration.");
        return Builder.Build();
    }
}
