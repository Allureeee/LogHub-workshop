using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Implementation.Behavioral.Strategy;

namespace Implementation.Behavioral.Iterator
{
	public class LogFileSource : IEnumerable<LogEntry>
	{
		private readonly string _logFileName;

		public LogFileSource(string logFileName)
		{
			_logFileName = logFileName;
		}

		public IEnumerator<LogEntry> GetEnumerator()
		{
			foreach (var line in File.ReadAllLines(_logFileName))
			{
				yield return LogEntry.Parse(line);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

}
