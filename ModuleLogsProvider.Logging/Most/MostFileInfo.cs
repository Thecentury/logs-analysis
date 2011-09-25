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
		private readonly List<LogEntry> logEntries = new List<LogEntry>();
		private readonly string name;

		public MostFileInfo( string name )
		{
			if ( name == null ) throw new ArgumentNullException( "name" );

			this.name = name;
		}

		public void Refresh()
		{
			// do nothing
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			return new MostLogFileReader( logEntries );
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
