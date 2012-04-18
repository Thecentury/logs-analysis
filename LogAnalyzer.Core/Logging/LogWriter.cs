namespace LogAnalyzer.Logging
{
	public abstract class LogWriter
	{
		public abstract void WriteLine( string message, MessageType messageType );
	}
}
