using LogHub.Models;
namespace LogHub.Visiting;

public interface ILogEntryVisitor
{
    void Visit(SimpleLogEntry entry);
    void Visit(ExceptionLogEntry entry);
    void Visit(MetricLogEntry entry);
}
