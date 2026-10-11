using System.Reflection;
using LogHub.Application;
using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Graphs;
using LogHub.Importing;
using LogHub.Importing.Sources;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Sinks;
using Microsoft.Extensions.DependencyInjection;
namespace LogHub;
public static class CompositionRoot
{
    public static IServiceCollection CreateServices(string configPath)
    {
        var configuration = new ConfigReader().Load(configPath);
        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<LogParser>();
        services.AddSingleton(new SeverityFilter(configuration.MinSeverity));
        services.AddSingleton(new LogFormatter(configuration.Format));
        services.AddSingleton<TextWriter>(Console.Out);
        var importers = new Dictionary<string, (Type Type, Func<ITextSource> Source)>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["mixed"] = (typeof(MixedLogImporter), () => new FileTextSource(configuration.LogPath)),
            ["json"] = (typeof(JsonLogImporter), () => new FileTextSource(configuration.LogPath)),
            ["memory"] = (typeof(InMemoryLogImporter), () => new InMemoryTextSource(
                "2026-10-07T10:00:00|Info|Memory pipe record\n" +
                "{\"date\":\"2026-10-07T10:01:00\",\"level\":\"Error\",\"message\":\"Memory JSON record\"}\n" +
                "bad input\n"))
        };
        if (!importers.TryGetValue(configuration.Importer, out var selected))
            throw new NotSupportedException($"Unknown importer '{configuration.Importer}'. Use mixed, json or memory.");
        services.AddSingleton<ITextSource>(selected.Source());
        services.AddSingleton(typeof(ILogImporter), selected.Type);

        var availableSinks = typeof(ILogSink).Assembly.GetTypes()
            .Where(t => typeof(ILogSink).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .Select(t => (Type: t, Attribute: t.GetCustomAttribute<SinkNameAttribute>()))
            .Where(pair => pair.Attribute is not null)
            .ToDictionary(pair => pair.Attribute!.Name, pair => pair.Type, StringComparer.OrdinalIgnoreCase);
        var sinkNames = configuration.Sink.Split(',',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (sinkNames.Length == 0) throw new ArgumentException("The sink list is empty.");
        var sinkTypes = new List<Type>();
        foreach (var name in sinkNames)
        {
            if (!availableSinks.TryGetValue(name, out var type))
                throw new NotSupportedException($"Unknown sink '{name}'.");
            services.AddSingleton(typeof(ILogSink), type);
            sinkTypes.Add(type);
        }
        services.AddSingleton<LogProcessor>();
        services.AddSingleton(CreateGraphDemo(selected.Type, configuration.Importer, sinkTypes));
        services.AddSingleton<LogHubApplication>();
        return services;
    }

    private static GraphDemo CreateGraphDemo(Type importerType, string importerName, IEnumerable<Type> sinkTypes)
    {
        var app = new Node(nameof(LogHubApplication));
        var processor = new Node(nameof(LogProcessor));
        var importer = new Node(importerType.Name);
        var source = new Node(importerName.Equals("memory", StringComparison.OrdinalIgnoreCase)
            ? nameof(InMemoryTextSource) : nameof(FileTextSource));
        var parser = new Node(nameof(LogParser));
        var filter = new Node(nameof(SeverityFilter));
        var formatter = new Node(nameof(LogFormatter));
        var config = new Node(nameof(ConfigService));
        var configuration = new Node(nameof(LogConfiguration));
        var writer = new Node(nameof(TextWriter));
        var demo = new Node(nameof(GraphDemo));
        var graphData = new Node(nameof(DependencyGraph));
        var nodes = new List<Node> { app, processor, importer, source, parser, filter,
            formatter, config, configuration, writer, demo, graphData };
        var edges = new List<(Node, Node)>
        {
            (app, processor), (app, config), (app, demo), (app, writer),
            (processor, importer), (processor, filter), (processor, formatter),
            (importer, source), (importer, parser), (config, configuration), (demo, graphData)
        };
        foreach (var type in sinkTypes)
        {
            var sink = new Node(type.Name);
            nodes.Add(sink);
            edges.Add((processor, sink));
            edges.Add((sink, configuration));
        }
        var serviceGraph = new DependencyGraph(nodes, edges);
        var cyclicExample = new DependencyGraph(new[] { processor, importer, parser },
            new[] { (processor, importer), (importer, parser), (parser, processor) });
        return new GraphDemo(serviceGraph, app, cyclicExample);
    }
}
