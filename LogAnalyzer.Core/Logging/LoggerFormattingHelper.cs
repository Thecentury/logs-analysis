using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Logging
{
	internal static class LoggerFormattingHelper
	{
		public static string FormatString( string message, MessageType messageType )
		{
			DateTime now = DateTime.Now;

			string typeString = "";
			switch ( messageType )
			{
				case MessageType.Error:
					typeString = "E";
					break;
				case MessageType.Info:
					typeString = "I";
					break;
				default:
					throw new InvalidOperationException();
			}

			string logLine = String.Format( "[{0}] {1} {2}", typeString, now.ToShortTimeString(), message );

			return logLine;
		}
	}
}
