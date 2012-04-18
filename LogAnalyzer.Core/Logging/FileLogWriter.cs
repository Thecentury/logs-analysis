using System;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Reflection;

namespace LogAnalyzer.Logging
{
	public sealed class FileLogWriter : SingleThreadedLogWriter, ISupportInitialize
	{
		public string FilePath { get; set; }

		public bool ShouldCleanLogFile { get; set; }

		protected override void OnNewMessage( LogMessage logMessage )
		{
			try
			{
				File.AppendAllLines( FilePath, new[] { logMessage.Message } );
			}
			catch ( IOException )
			{
				if ( Debugger.IsAttached )
					Debugger.Break();

				// todo retry?
			}
		}

		public FileLogWriter() { }

		public FileLogWriter( string fileName )
			: this()
		{
			if ( String.IsNullOrWhiteSpace( fileName ) )
				throw new ArgumentException();

			string fullPath = Path.GetFullPath( fileName );
			this.FilePath = fullPath;
			string directoryPath = Path.GetDirectoryName( fullPath );
			if ( !Directory.Exists( directoryPath ) )
			{
				Directory.CreateDirectory( directoryPath );
			}
		}

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			// do nothing
		}

		void ISupportInitialize.EndInit()
		{
			if ( !Path.IsPathRooted( FilePath ) )
			{
				string exeLocation = Path.GetDirectoryName( Path.GetFullPath( Assembly.GetExecutingAssembly().Location ) );
				FilePath = Path.Combine( exeLocation, FilePath );
			}

			if ( ShouldCleanLogFile )
			{
				if ( File.Exists( FilePath ) )
				{
					File.Delete( FilePath );
				}
			}
		}

		#endregion
	}
}
