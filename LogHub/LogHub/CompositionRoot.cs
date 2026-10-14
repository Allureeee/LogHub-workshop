using LogHub.Adapters;
using LogHub.Application;
using LogHub.Building;
using LogHub.Catalog;
using LogHub.Configuration;
using LogHub.Creation;
using LogHub.Decorating;
using LogHub.External;
using LogHub.Families;
using LogHub.Filtering;
using System.Reflection;
using LogHub.Formatting;
using LogHub.Graphs;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Infrastructure;
using LogHub.Observing;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Proxy;
using LogHub.Rules;
using LogHub.Sinks;
using LogHub.Visiting;
using Microsoft.Extensions.DependencyInjection;
namespace LogHub;

public static class CompositionRoot
{
    public static IServiceCollection CreateServices(string configPath) => CreateServices(new ConfigReader().Load(configPath));
    public static IServiceCollection CreateServices(LogConfiguration configuration) => CreateServices(new PipelinePlan(configuration));
    public static BuiltPipeline Build(PipelinePlan plan)
    {
        var provider = CreateServices(plan).BuildServiceProvider(new ServiceProviderOptions
            { ValidateOnBuild = true, ValidateScopes = true });
        try
        {
            return new BuiltPipeline(provider.GetRequiredService<LogHubApplication>(), provider);
        }
        catch { provider.Dispose(); throw; }
    }
    private static IServiceCollection CreateServices(PipelinePlan plan)
    {
        var configuration = plan.Configuration.DeepClone();
        configuration.Family = configuration.Family.Trim().ToLowerInvariant();
        configuration.Adapter = configuration.Adapter.Trim().ToLowerInvariant();
        configuration.Builder = configuration.Builder.Trim().ToLowerInvariant();
        Validate(configuration);
        var resources = plan.Resources ?? new PhysicalHubResources();
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<IConfigService, ConfigService>();
        var parser = new LogParser();
        services.AddSingleton(parser);
        services.AddSingleton(new SeverityFilter(configuration.MinSeverity));
        services.AddSingleton(new LogFormatter(configuration.Format));
        services.AddSingleton<StatisticsVisitor>();
        services.AddSingleton<DuplicateKeyVisitor>();
        TimeProvider clock = configuration.ReferenceTimeUtc.HasValue
            ? new FixedTimeProvider(configuration.ReferenceTimeUtc.Value) : resources.Clock;
        services.AddSingleton(clock);
        services.AddSingleton<TextWriter>(resources.Output);
        var severities = new CachingSeverityCatalog(new SeverityCatalog());
        services.AddSingleton<ISeverityCatalog>(severities);
        var rule = plan.Rule ?? new RuleParser(severities).Parse(configuration.RuleJson);
        services.AddSingleton<ILogRule>(rule);
        if ((plan.Rule is not null || !string.IsNullOrWhiteSpace(configuration.RuleJson)) &&
            !configuration.Filters.Split(',', StringSplitOptions.TrimEntries).Contains("rule", StringComparer.OrdinalIgnoreCase))
            configuration.Filters = string.IsNullOrWhiteSpace(configuration.Filters) ? "rule" : configuration.Filters + ",rule";

        var creators = typeof(LogImporterFactory).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && typeof(LogImporterFactory).IsAssignableFrom(t))
            .Select(t => (LogImporterFactory)Activator.CreateInstance(t)!).ToArray();
        var registry = new ImporterFactoryRegistry(creators);
        services.AddSingleton(registry);
        var selected = registry.Get(configuration.Importer);
        var source = plan.Source ?? (configuration.Importer.Equals("memory", StringComparison.OrdinalIgnoreCase)
            ? new InMemoryTextSource(MemoryText) : resources.CreateSource(configuration.LogPath));
        var importer = plan.Importer ?? selected.Create(source, parser);
        services.AddSingleton<ITextSource>(source);
        services.AddSingleton<ILogImporter>(importer);
        services.AddSingleton<ImportSession>();
        var filterTypes = typeof(LogFilter).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && typeof(LogFilter).IsAssignableFrom(t))
            .OrderBy(t => t.FullName).ToArray();
        foreach (var type in filterTypes) services.AddSingleton(typeof(LogFilter), type);
        services.AddSingleton<FilterChain>();
        var events = new PipelineEvents();
        services.AddSingleton(events);
        var counter = new ProcessedCounterObserver();
        var alerts = new CriticalAlertObserver(resources.Output);
        services.AddSingleton(counter); services.AddSingleton(alerts);
        services.AddSingleton<LogObserver>(counter); services.AddSingleton<LogObserver>(alerts);

        var targets = plan.Sinks.Count > 0 ? plan.Sinks.ToList() : CreateTargets(configuration, resources, severities);
        var measurements = new List<WriteMeasurement>();
        var sinks = targets.Select(target => SinkDecoration.Apply(target, configuration.Delivery, clock, measurements.Add)).ToArray();
        var lifetime = new PipelineSinkLifetime(sinks, events, measurements);
        services.AddSingleton<PipelineSinkLifetime>(_ => lifetime);
        foreach (var sink in sinks) services.AddSingleton<LogObserver>(new SinkObserver<ILogSink>(sink));
        services.AddSingleton<PipelineSubscriptions>();
        services.AddSingleton<ProcessingPipeline>();
        services.AddSingleton<LogFlowMediator>();
        services.AddSingleton<LogProcessor>();
        services.AddSingleton<PipelineReport>();
        services.AddSingleton(CreateGraphDemo(importer.GetType(), configuration.Importer, targets.Select(t => t.GetType()), filterTypes));
        services.AddSingleton<LogHubApplication>();
        return services;
    }
    private static void Validate(LogConfiguration configuration)
    {
        if (configuration.Family is not ("legacy" or "development" or "production"))
            throw new ArgumentException("family must be legacy, development or production.");
        if (configuration.Adapter is not ("object" or "class")) throw new ArgumentException("adapter must be object or class.");
        if (configuration.Builder is not ("fluent" or "classic")) throw new ArgumentException("builder must be fluent or classic.");
        if (configuration.Delivery.BatchSize < 1 || configuration.Delivery.Retry.Attempts < 1)
            throw new ArgumentException("batchSize and retryAttempts must be positive.");
        var names = configuration.Delivery.Decorators.Select(n => n.Trim().ToLowerInvariant()).ToArray();
        if (names.Any(n => n is not ("buffer" or "retry" or "timing")) || names.Distinct(StringComparer.Ordinal).Count() != names.Length)
            throw new ArgumentException("List buffer, retry and timing at most once each.");
    }
    private static List<ILogSink> CreateTargets(LogConfiguration configuration, IHubResources resources, ISeverityCatalog severities)
    {
        var names = configuration.Sink.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (names.Length == 0) throw new ArgumentException("At least one receiver is required.");
        var result = new List<ILogSink>();
        try
        {
            if (configuration.Family == "legacy")
            {
                foreach (var name in names) result.Add(CreateExtra(name, configuration, resources, severities));
                return result;
            }
            if (!names.Contains("console", StringComparer.OrdinalIgnoreCase) || !names.Contains("file", StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Variant 20 final profiles require sink=console,file (additional receivers are allowed).");
            LogFamilyFactory factory = configuration.Family == "development"
                ? new DevelopmentFactory(resources, configuration)
                : new ProductionFactory(resources, configuration, severities);
            var family = factory.CreateFamily();
            result.Add(new FamilyLogSink(family.Output, family.Formatter));
            result.Add(new FamilyLogSink(family.Storage, family.Formatter));
            foreach (var name in names)
            {
                if (name.Equals(family.Output.Name, StringComparison.OrdinalIgnoreCase) ||
                    name.Equals(family.Storage.Name, StringComparison.OrdinalIgnoreCase)) continue;
                result.Add(new FamilyFormattedSink(CreateExtra(name, configuration, resources, severities), family.Formatter));
            }
            return result;
        }
        catch { foreach (var sink in result) sink.Dispose(); throw; }
    }
    private static ILogSink CreateExtra(string name, LogConfiguration configuration, IHubResources resources, ISeverityCatalog severities)
    {
        var factories = new Dictionary<string, Func<ILogSink>>(StringComparer.OrdinalIgnoreCase)
        {
            ["console"] = () => resources.CreateConsole(configuration),
            ["file"] = () => new VirtualLogSink("file", () => resources.CreateFile(configuration)),
            ["database"] = () => new VirtualLogSink("database", () => configuration.Adapter == "class"
                ? new SqlClassAdapter(resources.CreateSqlSession(configuration.DatabasePath), severities)
                : new SqlObjectAdapter(new SqlLogApi(resources.CreateSqlSession(configuration.DatabasePath)), severities)),
            ["document"] = () => new VirtualLogSink("document", () => configuration.Adapter == "class"
                ? new JsonClassAdapter(resources.CreateDocumentChannel(configuration.DocumentPath), new LogFormatter("json"))
                : new JsonObjectAdapter(new JsonLogApi(resources.CreateDocumentChannel(configuration.DocumentPath)), new LogFormatter("json")))
        };
        if (factories.TryGetValue(name, out var factory)) return factory();
        var type = typeof(ILogSink).Assembly.GetTypes().FirstOrDefault(t => t.IsClass && !t.IsAbstract
            && !t.ContainsGenericParameters && typeof(ILogSink).IsAssignableFrom(t)
            && t.GetCustomAttribute<SinkNameAttribute>()?.Name.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
        if (type is null) throw new NotSupportedException($"Unknown receiver '{name}'.");
        return new VirtualLogSink(name, () => (ILogSink)Activator.CreateInstance(type, configuration)!);
    }
    private const string MemoryText = "2026-10-07T11:30:00|Info|Memory pipe record\n" +
        "{\"date\":\"2026-10-07T11:30:01Z\",\"level\":\"Critical\",\"message\":\"Memory failure\",\"kind\":\"exception\",\"stackTrace\":\"at Demo.Run()\"}\n" +
        "{\"date\":\"2026-10-07T11:30:02Z\",\"level\":\"Info\",\"message\":\"CPU\",\"kind\":\"metric\",\"value\":42.5}\n" + "bad input\n";
    private static GraphDemo CreateGraphDemo(Type importerType, string importerName,
        IEnumerable<Type> sinks, IEnumerable<Type> filters)
    {
        var nodes = new Dictionary<string, Node>();
        var edges = new List<(Node, Node)>();
        Node Get(string name)
        {
            if (!nodes.TryGetValue(name, out var node)) { node = new Node(name); nodes.Add(name, node); }
            return node;
        }
        void Edge(string from, string to) => edges.Add((Get(from), Get(to)));
        foreach (var dependency in new[] { "LogProcessor", "ConfigService", "PipelineReport", "GraphDemo", "TextWriter", "PipelineSinkLifetime" })
            Edge("LogHubApplication", dependency);
        Edge("ConfigService", "LogConfiguration");
        Edge("LogProcessor", "LogFlowMediator");
        Edge("LogFlowMediator", "ImportSession"); Edge("LogFlowMediator", "ProcessingPipeline");
        Edge("ImportSession", importerType.Name);
        Edge(importerType.Name, importerName.Equals("memory", StringComparison.OrdinalIgnoreCase)
            ? "InMemoryTextSource" : "FileTextSource");
        Edge(importerType.Name, "LogParser");
        foreach (var dependency in new[] { "LogConfiguration", "FilterChain", "LogFormatter", "StatisticsVisitor", "PipelineEvents", "PipelineSubscriptions" })
            Edge("ProcessingPipeline", dependency);
        Edge("FilterChain", "LogConfiguration");
        foreach (var type in filters) Edge("FilterChain", type.Name);
        Edge("SeverityLogFilter", "SeverityFilter");
        Edge("AgeLogFilter", "LogConfiguration"); Edge("AgeLogFilter", "TimeProvider");
        Edge("DuplicateLogFilter", "DuplicateKeyVisitor");
        Edge("CompositeRuleFilter", "ILogRule");
        Edge("PipelineSinkLifetime", "PipelineEvents");
        foreach (var observer in new[] { "ProcessedCounterObserver", "CriticalAlertObserver" })
            Edge("PipelineSubscriptions", observer);
        Edge("PipelineSubscriptions", "PipelineEvents"); Edge("PipelineSubscriptions", "LogConfiguration");
        Edge("CriticalAlertObserver", "TextWriter");
        foreach (var type in sinks)
        {
            var observer = $"SinkObserver<{type.Name}>";
            Edge("PipelineSubscriptions", observer); Edge(observer, type.Name); Edge(type.Name, "LogConfiguration");
            if (type == typeof(ConsoleLogSink)) Edge(type.Name, "TextWriter");
        }
        foreach (var dependency in new[] { "StatisticsVisitor", "ProcessedCounterObserver", "CriticalAlertObserver", "LogFlowMediator", "PipelineEvents" })
            Edge("PipelineReport", dependency);
        Edge("GraphDemo", "DependencyGraph");
        var serviceGraph = new DependencyGraph(nodes.Values.ToArray(), edges);
        var a = new Node("DemoA"); var b = new Node("DemoB"); var c = new Node("DemoC");
        var cycle = new DependencyGraph(new[] { a, b, c }, new[] { (a, b), (b, c), (c, a) });
        return new GraphDemo(serviceGraph, Get("LogHubApplication"), cycle);
    }
}
