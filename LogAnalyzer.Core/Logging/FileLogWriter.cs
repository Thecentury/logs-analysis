using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Threading;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace LogAnalyzer
{
	public sealed class FileLogWriter : SingleThreadedLogWriter, ISupportInitialize
	{
		public string FileName { get; set; }

		public bool ShouldCleanLogFile { get; set; }

		protected override void OnNewMessage( string message )
		{
			try
			{
				File.AppendAllLines( FileName, new string[] { message } );
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

			this.FileName = fileName;
			string fullPath = Path.GetFullPath( fileName );
			string directoryPath = Path.GetDirectoryName( fullPath );
			if ( !Directory.Exists( fullPath ) )
			{
				Directory.CreateDirectory( fullPath );
			}
		}

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			// do nothing
		}

		void ISupportInitialize.EndInit()
		{
			if ( ShouldCleanLogFile )
			{
				if ( File.Exists( FileName ) )
				{
					File.Delete( FileName );
				}
			}
		}

		#endregion
	}
}
