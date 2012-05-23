namespace LogAnalyzer.LoggingTemplates
{
	public interface ILogEntryFormatRecognizer
	{
		LogEntryFormat FindFormat( ILogEntry logEntry );
	}
}