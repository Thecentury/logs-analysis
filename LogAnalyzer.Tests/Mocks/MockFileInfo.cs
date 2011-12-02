using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockFileInfo : IFileInfo
	{
		private readonly List<byte> bytes = new List<byte>();
		private readonly string name;
		private readonly string fullName;
		private readonly MockLogRecordsSource logSource;
		private readonly Encoding encoding = Encoding.Unicode;

		private string dateFormat = MostLogLineParser.DateTimeFormat;
		public string DateFormat
		{
			get { return dateFormat; }
			set { dateFormat = value; }
		}

		private readonly object sync = new object();

		public MockFileInfo( string name, string fullName, MockLogRecordsSource logSource )
		{
			this.name = name;
			this.fullName = fullName;
			this.logSource = logSource;
		}

		public MockFileInfo( string name, string fullName, MockLogRecordsSource logSource, Encoding encoding )
			: this( name, fullName, logSource )
		{
			if ( encoding == null ) throw new ArgumentNullException( "encoding" );

			this.encoding = encoding;
		}

		#region IFileInfo Members

		void IFileInfo.Refresh()
		{
			// do nothing
		}

		LogFileReaderBase IFileInfo.GetReader( LogFileReaderArguments args )
		{
			MockStreamProvider streamProvider = new MockStreamProvider( bytes, sync );
			StreamLogFileReader reader = new StreamLogFileReader( args, streamProvider );
			return reader;
		}

		long IFileInfo.Length
		{
			get
			{
				lock ( sync )
				{
					return bytes.Count;
				}
			}
		}

		public string Name
		{
			get { return name; }
		}

		public string FullName
		{
			get { return fullName; }
		}

		string IFileInfo.Extension
		{
			get { return ".ext"; }
		}

		public string Content
		{
			get { return encoding.GetString( bytes.ToArray() ); }
		}

		#endregion

		public void Write( string str )
		{
			byte[] messageBytes = encoding.GetBytes( str );

			lock ( sync )
			{
				bytes.AddRange( messageBytes );
			}

			logSource.RaiseFileChanged( name );
		}

		public void WriteLine( string str )
		{
			Write( str + Environment.NewLine );
		}

		public void WriteLogMessage( char severity, int threadId, string message )
		{
			string logMessage = String.Format( "[{0}] [{1,3}] {2}\t{3}{4}", 
				severity, threadId, DateTime.Now.ToString( dateFormat ), message, Environment.NewLine );
			Write( logMessage );
		}

		public void WriteLogMessage( char severity, string message )
		{
			WriteLogMessage( severity, Thread.CurrentThread.ManagedThreadId, message );
		}

		public void WriteInfo( string message )
		{
			WriteLogMessage( 'I', message );
		}

		public void WriteInfo( string message, DateTime dateTime )
		{
			string logMessage = String.Format( "[I] [123] {0}\t{1}", dateTime.ToString( dateFormat ), message );
			Write( logMessage );
		}

		public void WriteError( string message )
		{
			WriteLogMessage( 'E', message );
		}

		public DateTime LoggingDate
		{
			get { return DateTime.Now.Date; }
		}
	}
}
