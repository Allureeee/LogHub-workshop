using System;
using System.Linq;
using System.Reflection;
using LogHub.Configuration;

namespace LogHub.Sinks;
public static class LogSinkFactory
{
    public static ILogSink Create(LogConfiguration configuration)
    {
        var sinkType = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type =>
                typeof(ILogSink).IsAssignableFrom(type) &&
                !type.IsInterface &&
                !type.IsAbstract)
            .FirstOrDefault(type =>
                type.GetCustomAttribute<SinkNameAttribute>()?.Name
                    .Equals(
                        configuration.Sink,
                        StringComparison.OrdinalIgnoreCase) == true);
        if (sinkType is null)
        {
            throw new NotSupportedException(
                $"Приёмник '{configuration.Sink}' не поддерживается");
        }
        return (ILogSink)Activator.CreateInstance(
            sinkType,
            configuration)!;
    }
}