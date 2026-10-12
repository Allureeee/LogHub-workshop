using LogHub.Application;
using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Graphs;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Observing;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;
using System.Reflection;
using LogHub.Visiting;
using Microsoft.Extensions.DependencyInjection;
namespace LogHub;

public static class CompositionRoot
{
    public static IServiceCollection CreateServices(string configPath) =>
        CreateServices(new ConfigReader().Load(configPath));
    public static IServiceCollection CreateServices(LogConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<LogParser>();
        services.AddSingleton(new SeverityFilter(configuration.MinSeverity));
        services.AddSingleton(new LogFormatter(configuration.Format));
        services.AddSingleton<StatisticsVisitor>();
        services.AddSingleton<DuplicateKeyVisitor>();
        services.AddSingleton<TimeProvider>(configuration.ReferenceTimeUtc.HasValue
            ? new FixedTimeProvider(configuration.ReferenceTimeUtc.Value) : TimeProvider.System);
        services.AddSingleton<TextWriter>(Console.Out);

        var importers = new Dictionary<string, (Type Type, Func<ITextSource> Source)>(StringComparer.OrdinalIgnoreCase)
        {
            ["mixed"] = (typeof(MixedLogImporter), () => new FileTextSource(configuration.LogPath)),
            ["json"] = (typeof(JsonLogImporter), () => new FileTextSource(configuration.LogPath)),
            ["memory"] = (typeof(InMemoryLogImporter), () => new InMemoryTextSource(
                "2026-10-07T11:30:00|Info|Memory pipe record\n" +
                "{\"date\":\"2026-10-07T11:30:01Z\",\"level\":\"Critical\",\"message\":\"Memory failure\",\"kind\":\"exception\",\"stackTrace\":\"at Demo.Run()\"}\n" +
                "{\"date\":\"2026-10-07T11:30:02Z\",\"level\":\"Info\",\"message\":\"CPU\",\"kind\":\"metric\",\"value\":42.5}\n" +
                "bad input\n"))
        };
        if (!importers.TryGetValue(configuration.Importer, out var selected))
            throw new NotSupportedException($"Unknown importer '{configuration.Importer}'.");
        services.AddSingleton<ITextSource>(selected.Source());
        services.AddSingleton(typeof(ILogImporter), selected.Type);
        services.AddSingleton<ImportSession>();

        var filterTypes = typeof(LogFilter).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters
                && typeof(LogFilter).IsAssignableFrom(t)).OrderBy(t => t.FullName).ToArray();
        foreach (var type in filterTypes) services.AddSingleton(typeof(LogFilter), type);
        services.AddSingleton<FilterChain>(); // Reads the order from configuration, not reflection order.
        services.AddSingleton<PipelineEvents>();
        var counter = new ProcessedCounterObserver();
        var alerts = new CriticalAlertObserver(Console.Out);
        services.AddSingleton(counter);
        services.AddSingleton(alerts);
        services.AddSingleton<LogObserver>(counter);
        services.AddSingleton<LogObserver>(alerts);

        var availableSinks = typeof(ILogSink).Assembly.GetTypes()
            .Where(t => typeof(ILogSink).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<SinkNameAttribute>()))
            .Where(pair => pair.Attribute is not null)
            .ToDictionary(pair => pair.Attribute!.Name, pair => pair.Type, StringComparer.OrdinalIgnoreCase);
        var names = configuration.Sink.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (names.Length == 0) throw new ArgumentException("At least one sink is required.");
        var sinkTypes = new List<Type>();
        foreach (var name in names)
        {
            if (!availableSinks.TryGetValue(name, out var type)) throw new NotSupportedException($"Unknown sink '{name}'.");
            services.AddSingleton(type); // Container owns and disposes this receiver.
            services.AddSingleton(typeof(LogObserver), typeof(SinkObserver<>).MakeGenericType(type));
            sinkTypes.Add(type);
        }
        services.AddSingleton<PipelineSubscriptions>();
        services.AddSingleton<ProcessingPipeline>();
        services.AddSingleton<LogFlowMediator>();
        services.AddSingleton<LogProcessor>();
        services.AddSingleton<PipelineReport>();
        services.AddSingleton(CreateGraphDemo(selected.Type, configuration.Importer, sinkTypes, filterTypes));
        services.AddSingleton<LogHubApplication>();
        return services;
    }

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
        foreach (var dependency in new[] { "LogProcessor", "ConfigService", "PipelineReport", "GraphDemo", "TextWriter" })
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
