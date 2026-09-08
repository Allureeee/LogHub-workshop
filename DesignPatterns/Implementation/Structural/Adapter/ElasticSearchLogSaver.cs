using System;

namespace Implementation.Structural.Adapter
{
	internal class ElasticSearchLogSaver
	{
		public void Save(ElkLogEntry logEntry)
		{
			Console.WriteLine($"ElkSaver {logEntry.Date} [{logEntry.Severity}] - {logEntry.Message}");
		}
	}
}