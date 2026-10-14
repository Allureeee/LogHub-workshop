using LogHub.Adapters;
using LogHub.Catalog;
using LogHub.Configuration;
using LogHub.External;
using LogHub.Formatting;
using LogHub.Infrastructure;
using LogHub.Proxy;
using LogHub.Sinks;
namespace LogHub.Families;
public sealed class ProductionFactory : LogFamilyFactory
{
    private readonly IHubResources _resources;
    private readonly LogConfiguration _configuration;
    private readonly ISeverityCatalog _severities;
    public ProductionFactory(IHubResources resources, LogConfiguration configuration, ISeverityCatalog severities)
        => (_resources, _configuration, _severities) = (resources, configuration, severities);
    public override FamilyOutput CreateOutput() => new FileFamilyOutput(
        new VirtualLogSink("file", () => _resources.CreateFile(_configuration)));
    public override FamilyStorage CreateStorage() => new DatabaseFamilyStorage(
        new VirtualLogSink("database", CreateDatabase));
    private ILogSink CreateDatabase() => _configuration.Adapter == "class"
        ? new SqlClassAdapter(_resources.CreateSqlSession(_configuration.DatabasePath), _severities)
        : new SqlObjectAdapter(new SqlLogApi(_resources.CreateSqlSession(_configuration.DatabasePath)), _severities);
    public override FamilyFormatter CreateFormatter() => new CompactFamilyFormatter(new LogFormatter("json"));
}
