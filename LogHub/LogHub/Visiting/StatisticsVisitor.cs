using LogHub.Models;
namespace LogHub.Visiting;

public sealed class StatisticsVisitor : ILogEntryVisitor
{
    public int SimpleCount { get; private set; }
    public int ExceptionCount { get; private set; }
    public int MetricCount { get; private set; }
    public int Total => SimpleCount + ExceptionCount + MetricCount;
    public void Visit(SimpleLogEntry entry) => SimpleCount++;
    public void Visit(ExceptionLogEntry entry) => ExceptionCount++;
    public void Visit(MetricLogEntry entry) => MetricCount++;
    public void Reset() => SimpleCount = ExceptionCount = MetricCount = 0;
}
