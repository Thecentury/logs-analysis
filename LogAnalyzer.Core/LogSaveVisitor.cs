using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace LogAnalyzer
{
	public sealed class LogSaveVisitor : ILogVisitor
	{
		private readonly TextWriter writer;
		private readonly ILogEntryFormatter formatter;

		public LogSaveVisitor( [NotNull] TextWriter writer, [NotNull] ILogEntryFormatter formatter )
		{
			if ( writer == null ) throw new ArgumentNullException( "writer" );
			if ( formatter == null ) throw new ArgumentNullException( "formatter" );

			this.writer = writer;
			this.formatter = formatter;
		}

		public void Visit( LogEntry logEntry )
		{
			Save( logEntry );
		}

		private void Save( LogEntry logEntry )
		{
			string str = formatter.Format( logEntry );
			writer.Write( str );
		}

		private void Save( IEnumerable<LogEntry> entries )
		{
			foreach ( var logEntry in entries )
			{
				Save( logEntry );
			}
		}

		public void Visit( LogFile logFile )
		{
			Save( logFile.LogEntries );
		}

		public void Visit( LogDirectory logDirectory )
		{
			Save( logDirectory.MergedEntries );
		}

		public void Visit( LogAnalyzerCore core )
		{
			Save( core.MergedEntries );
		}
	}
}