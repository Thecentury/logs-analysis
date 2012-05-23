using System;
using System.Collections.Generic;

namespace LogAnalyzer.LoggingTemplates
{
	internal sealed class LogEntryFormatByPatternLengthComparer : IComparer<LogEntryFormat>
	{
		public int Compare( LogEntryFormat x, LogEntryFormat y )
		{
			string rawTextX = LoggerUsage.FormatPlaceholderRegex.Replace( x.Usage.FormatString, String.Empty );
			string rawTextY = LoggerUsage.FormatPlaceholderRegex.Replace( y.Usage.FormatString, String.Empty );

			int l1 = rawTextX.Length;
			int l2 = rawTextY.Length;

			return -l1.CompareTo( l2 );
		}
	}
}