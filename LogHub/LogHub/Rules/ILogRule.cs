using LogHub.Models;
namespace LogHub.Rules;
public interface ILogRule
{
    bool Matches(LogRecord record);
}
