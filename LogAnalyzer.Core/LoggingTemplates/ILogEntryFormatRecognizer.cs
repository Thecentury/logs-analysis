namespace LogAnalyzer.LoggingTemplates
{
	public interface ILogEntryFormatRecognizer
	{
		LogEntryFormat FindFormat( LogEntry logEntry );
	}
}