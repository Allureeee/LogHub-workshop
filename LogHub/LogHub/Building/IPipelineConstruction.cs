using LogHub.Configuration;
namespace LogHub.Building;
public interface IPipelineConstruction
{
    void Reset(LogConfiguration configuration);
    void BuildSource();
    void BuildProcessing();
    void BuildOutputs();
    BuiltPipeline GetResult();
}
