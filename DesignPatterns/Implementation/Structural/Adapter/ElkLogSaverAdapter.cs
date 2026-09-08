using Implementation.Behavioral.Strategy;

namespace Implementation.Structural.Adapter
{
	public class ElkLogSaverAdapter : ILogSaver
	{
		private readonly ElasticSearchLogSaver _elasticSearchLogSaver =
			new ElasticSearchLogSaver();

		public void Save(LogEntry logEntry)
		{
			_elasticSearchLogSaver.Save(
				new ElkLogEntry
				{
					Date = logEntry.Date,
					Severity = logEntry.Severity.ToString(),
					Message = logEntry.Message
				});
		}
	}
}