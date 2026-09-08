namespace Implementation.Behavioral.Visitor
{
	public class ExceptionLogEntry : LogEntry
	{
		public override void Accept(ILogEntryVisitor logEntryVisitor)
		{
			// Благодаря перегрузке методов выбирается метод Visit(ExceptionLogEntry)
			logEntryVisitor.Visit(this);
		}
	}

	public class CriticalLogEntry : LogEntry
	{
		public override void Accept(ILogEntryVisitor logEntryVisitor)
		{
			// Благодаря перегрузке методов выбирается метод Visit(CriticalLogEntry)
			logEntryVisitor.Visit(this);
		}
	}

}