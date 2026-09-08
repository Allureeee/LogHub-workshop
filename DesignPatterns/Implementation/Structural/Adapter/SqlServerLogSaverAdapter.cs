using System;
using System.Collections.Generic;
using System.Text;
using Implementation.Behavioral.Visitor;
using LogEntry = Implementation.Behavioral.Strategy.LogEntry;

namespace Implementation.Structural.Adapter
{
	public class SqlServerLogSaverAdapter : ILogSaver
	{
		private readonly SqlServerLogSaver _sqlServerLogSaver =
			new SqlServerLogSaver();

		public void Save(LogEntry logEntry)
		{
			_sqlServerLogSaver.Save(logEntry.Date, logEntry.Severity.ToString(), logEntry.Message);
		}
	}
}