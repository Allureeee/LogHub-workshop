using System;
using Implementation.Behavioral.Strategy;

namespace Implementation.Structural.Adapter
{
	internal class SqlServerLogSaver
	{
		public void Save(DateTime dateTime, string severity, string message)
		{
			Console.WriteLine($"SqlServer {dateTime} [{severity}] - {message}");
		}
	}
}