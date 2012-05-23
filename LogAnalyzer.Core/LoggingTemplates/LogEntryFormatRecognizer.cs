using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace LogAnalyzer.LoggingTemplates
{
	public sealed class LogEntryFormatRecognizer : ILogEntryFormatRecognizer
	{
		private readonly List<LogEntryFormat> _usages = new List<LogEntryFormat>();

		public LogEntryFormatRecognizer( [NotNull] IEnumerable<LoggerUsageInAssembly> rawUsages )
		{
			if ( rawUsages == null )
			{
				throw new ArgumentNullException( "rawUsages" );
			}

			foreach ( var usageInAssembly in rawUsages )
			{
				foreach ( var usage in usageInAssembly.Usages )
				{
					_usages.Add( new LogEntryFormat( usage, usageInAssembly.AssemblyName ) );
				}
			}

			_usages.Sort( new LogEntryFormatByPatternLengthComparer() );
		}

		public LogEntryFormat FindFormat( ILogEntry logEntry )
		{
			string message = logEntry.UnitedText;
			var format = _usages.FirstOrDefault( u => u.Usage.Regex.IsMatch( message ) );
			return format;
		}
	}
}