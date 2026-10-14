using LogHub.Configuration;
namespace LogHub.Building;
public sealed class PipelineDirector
{
    private readonly IPipelineConstruction _builder;
    public PipelineDirector(IPipelineConstruction builder) => _builder = builder ?? throw new ArgumentNullException(nameof(builder));
    public BuiltPipeline BuildDefault() => Build(ProfileCatalog.Default());
    public BuiltPipeline Build(LogConfiguration configuration)
    {
        _builder.Reset(configuration);
        _builder.BuildProcessing();
        _builder.BuildSource();
        _builder.BuildOutputs();
        return _builder.GetResult();
    }
}
