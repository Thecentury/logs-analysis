using System.Diagnostics;

namespace LogAnalyzer.Logging
{
	public sealed class DebugLogWriter : LogWriter
	{
		public override void WriteLine( string message, MessageType messageType)
		{
			Debug.WriteLine( message );
		}
	}
}
