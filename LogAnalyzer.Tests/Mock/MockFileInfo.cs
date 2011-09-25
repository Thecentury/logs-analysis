using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace LogAnalyzer.Tests
{
	public sealed class MockFileInfo : IFileInfo
	{
		private readonly List<byte> bytes = new List<byte>();
		private readonly string name = null;
		private readonly string fullName = null;
		private readonly MockLogRecordsSource logSource = null;
		private readonly Encoding encoding = Encoding.Unicode;

		private readonly object sync = new object();

		public MockFileInfo( string name, string fullName, MockLogRecordsSource logSource )
		{
			this.name = name;
			this.fullName = fullName;
			this.logSource = logSource;
			lastWriteTime = DateTime.Now;
		}

		public MockFileInfo( string name, string fullName, MockLogRecordsSource logSource, Encoding encoding )
			: this( name, fullName, logSource )
		{
			if ( encoding == null )
				throw new ArgumentNullException( "encoding" );

			this.encoding = encoding;
		}

		#region IFileInfo Members

		void IFileInfo.Refresh()
		{
			// do nothing
		}

		Stream IFileInfo.OpenStream( int startPosition )
		{
			return new ByteListWrapperStream( bytes, sync );
		}

		int IFileInfo.Length
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

		private DateTime lastWriteTime;
		DateTime IFileInfo.LastWriteTime
		{
			get { return lastWriteTime; }
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
				lastWriteTime = DateTime.Now;
			}

			logSource.RaiseFileChanged( this.name );
		}

		public void WriteLine( string str )
		{
			Write( str + Environment.NewLine );
		}

		public void WriteLogMessage( char severity, int threadId, string message )
		{
			string logMessage = String.Format( "[{0}] [{1,3}] {2}\t{3}", severity, threadId, DateTime.Now.ToString( LogFile.DateTimeFormat ), message );
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
			string logMessage = String.Format( "[I] [123] {0}\t{1}", dateTime.ToString( LogFile.DateTimeFormat ), message );
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
