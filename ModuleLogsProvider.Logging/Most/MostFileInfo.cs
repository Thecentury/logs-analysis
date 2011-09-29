using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostFileInfo : IFileInfo
	{
		private readonly List<LogEntry> logEntries;
		private readonly string name;

		public MostFileInfo( string name, List<LogEntry> logEntries )
		{
			if ( name == null ) throw new ArgumentNullException( "name" );
			if ( logEntries == null ) throw new ArgumentNullException( "logEntries" );

			this.name = name;
			this.logEntries = logEntries;
		}

		public void Refresh()
		{
			// do nothing
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			return new MostLogFileReader( logEntries, args );
		}

		/// <summary>
		/// Длина файла, в байтах.
		/// </summary>
		/// <value></value>
		public int Length
		{
			get { return logEntries.Count; }
		}

		public string Name
		{
			get { return name; }
		}

		public string FullName
		{
			get { return name; }
		}

		/// <summary>
		/// Расширение (с начальной точкой).
		/// </summary>
		/// <value></value>
		public string Extension
		{
			get { return ".log"; }
		}

		public DateTime LastWriteTime
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LoggingDate
		{
			get { throw new NotImplementedException(); }
		}
	}
}
