using System.Data.Common;
using LogHub.Building;
using LogHub.Configuration;
using LogHub.Infrastructure;
using LogHub.Processing;
namespace LogHub;
public static class LogHub
{
    public static ProcessingResult Run(LogConfiguration? configuration = null, IHubResources? resources = null, bool showGraph = false)
    {
        var profile = (configuration ?? ProfileCatalog.Default()).DeepClone();
        using var pipeline = profile.Builder.Equals("classic", StringComparison.OrdinalIgnoreCase)
            ? new PipelineDirector(new StandardPipelineBuilder(resources)).Build(profile)
            : new PipelineBuilder(resources).Configure(profile).FromFile(profile.LogPath, profile.Importer).UseConfiguredOutputs().Build();
        return pipeline.Run(showGraph);
    }
    public static int Run(string[] args)
    {
        try
        {
            if (args.Any(a => a.StartsWith("--", StringComparison.Ordinal) && a != "--graph"))
                throw new ArgumentException("Supported option: --graph; an optional argument is a configuration path.");
            var paths = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToArray();
            if (paths.Length > 1) throw new ArgumentException("Specify at most one configuration path.");
            var profile = paths.Length == 0 ? ProfileCatalog.Default() : new ConfigReader().Load(paths[0]);
            var result = Run(profile, showGraph: args.Contains("--graph", StringComparer.Ordinal));
            return result.NotificationErrors == 0 ? 0 : 2;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or ArgumentException
            or NotSupportedException or InvalidOperationException or DbException or AggregateException)
        {
            Console.Error.WriteLine(error.Message);
            return 1;
        }
    }
}
