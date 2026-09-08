using Implementation.Behavioral.Strategy;

namespace Implementation.Structural.Adapter
{
	public class LogSaverClient
	{
		private readonly ILogSaver _logSaver;

		public LogSaverClient(ILogSaver logSaver)
		{
			_logSaver = logSaver;
		}

		public void Save(LogEntry entry)
		{
			_logSaver.Save(entry);
		}
	}
}