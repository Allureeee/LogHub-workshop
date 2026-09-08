namespace Implementation.Behavioral.Visitor
{
	public class SimpleLogEntry : LogEntry
	{
		public override void Accept(ILogEntryVisitor logEntryVisitor)
		{
			// Благодаря перегрузке методов выбирается метод Visit(SimpleLogEntry):
			// статический тип this здесь — SimpleLogEntry, а не LogEntry
			logEntryVisitor.Visit(this);
		}
	}
}