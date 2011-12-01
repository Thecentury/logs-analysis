using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostFileInfo : IFileInfo
	{
		private readonly string name;
		private readonly OneFileMessages messages;

		internal MostFileInfo( string name, OneFileMessages messages )
		{
			if ( name == null ) throw new ArgumentNullException( "name" );
			if ( messages == null ) throw new ArgumentNullException( "messages" );

			this.name = name;
			this.messages = messages;
		}

		public void Refresh()
		{
			// do nothing
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			return new MostLogFileReader( args, messages );
		}

		/// <summary>
		/// Длина файла, в байтах.
		/// </summary>
		/// <value></value>
		public long Length
		{
			get { return messages.Entries.Count; }
		}

		public string Name
		{
			get { return name; }
		}

		public string FullName
		{
			get { return Path.Combine( MostLogNotificationSource.DirectoryName, name ); }
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
			get { return DateTime.Now; }
		}

		public DateTime LoggingDate
		{
			get { return DateTime.Now.Date; }
		}
	}
}
