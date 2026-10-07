using LogHub.Application;
using LogHub.Configuration;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Parsing;
using LogHub.Processing;
using LogHub.Reading;
using LogHub.Sinks;
using Microsoft.Extensions.DependencyInjection;

var configPath = args.Length > 0 ? args[0] : "loghub.config";
var configReader = new ConfigReader();
var configuration = configReader.Load(configPath);
var services = new ServiceCollection();

services.AddSingleton(configuration);
services.AddSingleton<IConfigService, ConfigService>();
services.AddSingleton<LogFileReader>();
services.AddSingleton<LogParser>();
services.AddSingleton(
    new SeverityFilter(configuration.MinSeverity));
services.AddSingleton(
    new LogFormatter(configuration.Format));
switch (configuration.Sink.ToLowerInvariant())
{
    case "console":
        services.AddSingleton<ILogSink, ConsoleLogSink>();
        break;

    case "file":
        services.AddSingleton<ILogSink, FileLogSink>();
        break;

    default:
        throw new NotSupportedException(
            $"Приёмник '{configuration.Sink}' не поддерживается");
}
services.AddSingleton<LogProcessingModule>();
services.AddSingleton<LogHubApplication>();

using var provider = services.BuildServiceProvider();
var application = provider.GetRequiredService<LogHubApplication>();
application.Run();