using System;

namespace LogAnalyzer.Logging
{
	public sealed class ConsoleLogWriter : SingleThreadedLogWriter
	{
		protected override void OnNewMessage( LogMessage logMessage )
		{
			Console.WriteLine( logMessage.Message );
		}
	}
}
