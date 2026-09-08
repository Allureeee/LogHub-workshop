using System;

namespace Implementation.Behavioral.ChainOfResponsibility
{
	public class ConsoleLogger : Logger
	{
		public ConsoleLogger(LogLevel mask)
			: base(mask)
		{ }

		protected override void WriteMessage(string msg, LogLevel severity)
		{
			Console.WriteLine($"Writing to console: {severity} - {msg}");
		}
	}
}