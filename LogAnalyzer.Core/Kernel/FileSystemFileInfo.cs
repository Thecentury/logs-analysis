using System;
using System.IO;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	internal sealed class FileSystemFileInfo : IFileInfo
	{
		private readonly FileInfo fileInfo;

		public FileSystemFileInfo( string path )
		{
			this.fileInfo = new FileInfo( path );
		}

		public void Refresh()
		{
			fileInfo.Refresh();
		}

		public ILogFileReader GetReader( LogFileReaderArguments args )
		{
			FileSystemStreamReader streamReader = new FileSystemStreamReader( fileInfo );
			StreamLogFileReader reader = new StreamLogFileReader( args, streamReader );
			return reader;
		}

		public int Length
		{
			get { return (int)fileInfo.Length; }
		}

		public string Name
		{
			get { return fileInfo.Name; }
		}

		public string FullName
		{
			get { return fileInfo.FullName; }
		}

		public string Extension
		{
			get { return fileInfo.Extension; }
		}

		public DateTime LastWriteTime
		{
			get { return fileInfo.LastWriteTime; }
		}

		public DateTime LoggingDate
		{
			get
			{
				// todo probably perform some kind of analysis here,
				// or use LastWriteTime, or smth like that
				throw new NotImplementedException();

				return fileInfo.LastWriteTime.Date;
			}
		}
	}

	internal sealed class FileSystemStreamReader : IStreamProvider
	{
		private readonly FileInfo fileInfo;

		public FileSystemStreamReader( FileInfo fileInfo )
		{
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );

			this.fileInfo = fileInfo;
		}

		public Stream OpenStream( int startPosition )
		{
			try
			{
				FileStream stream = fileInfo.Open( FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
				stream.Position = startPosition;
				return stream;
			}
			catch ( IOException exc )
			{
				throw new LogAnalyzerIOException( "Ошибка при открытии файла \"{0}\"".Format2( fileInfo.FullName ), exc );
			}
		}
	}
}
