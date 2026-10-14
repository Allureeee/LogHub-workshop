using LogHub.Configuration;
using LogHub.Formatting;
using LogHub.Infrastructure;
namespace LogHub.Families;
public sealed class DevelopmentFactory : LogFamilyFactory
{
    private readonly IHubResources _resources;
    private readonly LogConfiguration _configuration;
    public DevelopmentFactory(IHubResources resources, LogConfiguration configuration)
        => (_resources, _configuration) = (resources, configuration);
    public override FamilyOutput CreateOutput() => new ConsoleFamilyOutput(_resources.CreateConsole(_configuration));
    public override FamilyStorage CreateStorage() => new MemoryFamilyStorage();
    public override FamilyFormatter CreateFormatter() => new DetailedFamilyFormatter(new LogFormatter("plain"));
}
