using LogHub;
using LogHub.Application;
using Microsoft.Extensions.DependencyInjection;

var showGraph = args.Contains("--graph", StringComparer.OrdinalIgnoreCase);
var configPath = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal))
    ?? Path.Combine(AppContext.BaseDirectory, "profiles", "variant20.config");
try
{
    var services = CompositionRoot.CreateServices(configPath);
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        { ValidateOnBuild = true, ValidateScopes = true });
    // The only explicit service-provider resolution in production code.
    var application = provider.GetRequiredService<LogHubApplication>();
    application.Run(showGraph);
    return 0;
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
    or ArgumentException or NotSupportedException or InvalidOperationException)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
