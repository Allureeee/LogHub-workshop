using Implementation.Behavioral.Strategy;

namespace Implementation.Structural.Adapter
{
	public interface ILogSaver
	{
		void Save(LogEntry logEntry);
	}
}