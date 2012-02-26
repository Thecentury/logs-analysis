using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Tests.Mocks
{
	public sealed class MockFileInfo : IFileInfo
	{
		private readonly List<byte> _bytes = new List<byte>();
		private readonly string _name;
		private readonly string _fullName;
		private readonly MockLogRecordsSource _logSource;
		private readonly Encoding _encoding = Encoding.Unicode;

		private string _dateFormat = MostLogLineParser.DateTimeFormat;
		public string DateFormat
		{
			get { return _dateFormat; }
			set { _dateFormat = value; }
		}

		private readonly object _sync = new object();

		public MockFileInfo( string name, string fullName, MockLogRecordsSource logSource )
		{
			this._name = name;
			this._fullName = fullName;
			this._logSource = logSource;
		}

		public MockFileInfo( string name, string fullName, MockLogRecordsSource logSource, Encoding encoding )
			: this( name, fullName, logSource )
		{
			if ( encoding == null ) throw new ArgumentNullException( "encoding" );

			this._encoding = encoding;
		}

		#region IFileInfo Members

		void IFileInfo.Refresh()
		{
			// do nothing
		}

		LogFileReaderBase IFileInfo.GetReader( LogFileReaderArguments args )
		{
			MockStreamProvider streamProvider = new MockStreamProvider( _bytes, _sync );
			StreamLogFileReader reader = new StreamLogFileReader( args, streamProvider );
			return reader;
		}

		long IFileInfo.Length
		{
			get
			{
				lock ( _sync )
				{
					return _bytes.Count;
				}
			}
		}

		public string Name
		{
			get { return _name; }
		}

		public string FullName
		{
			get { return _fullName; }
		}

		string IFileInfo.Extension
		{
			get { return ".ext"; }
		}

		public string Content
		{
			get { return _encoding.GetString( _bytes.ToArray() ); }
		}

		#endregion

		public void Write( string str )
		{
			byte[] messageBytes = _encoding.GetBytes( str );

			lock ( _sync )
			{
				_bytes.AddRange( messageBytes );
			}

			_logSource.RaiseFileChanged( _name );
		}

		public void WriteLine( string str )
		{
			Write( str + Environment.NewLine );
		}

		public void WriteLogMessage( char severity, int threadId, string message )
		{
			string logMessage = String.Format( "[{0}] [{1,3}] {2}\t{3}{4}", 
				severity, threadId, DateTime.Now.ToString( _dateFormat ), message, Environment.NewLine );
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
			string logMessage = String.Format( "[I] [123] {0}\t{1}", dateTime.ToString( _dateFormat ), message );
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
