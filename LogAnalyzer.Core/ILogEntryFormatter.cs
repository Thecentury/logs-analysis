namespace LogAnalyzer
{
	public interface ILogEntryFormatter
	{
		string Format( LogEntry logEntry );
	}
}