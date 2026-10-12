using LogHub;
using LogHub.Application;
using Microsoft.Extensions.DependencyInjection;

var showGraph = args.Contains("--graph", StringComparer.OrdinalIgnoreCase);
var configPath = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal))
    ?? Path.Combine(AppContext.BaseDirectory, "profiles", "pipeline-push.config");
try
{
    var services = CompositionRoot.CreateServices(configPath);
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        { ValidateOnBuild = true, ValidateScopes = true });
    var application = provider.GetRequiredService<LogHubApplication>();
    var result = application.Run(showGraph);
    return result.NotificationErrors == 0 ? 0 : 2;
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
    or ArgumentException or NotSupportedException or InvalidOperationException)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
