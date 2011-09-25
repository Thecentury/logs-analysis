using System;
using System.IO;

namespace LogAnalyzer.Kernel
{
	internal sealed class CacheFileInfo : IFileInfo
	{
		private readonly FileInfo remoteFile;
		private readonly FileInfo cacheFile;
		private readonly DateTime loggingDate;

		public CacheFileInfo( FileInfo remoteFile, FileInfo cacheFile, DateTime loggingDate )
		{
			if ( remoteFile == null )
				throw new ArgumentNullException( "remoteFile" );
			if ( cacheFile == null )
				throw new ArgumentNullException( "cacheFile" );

			this.remoteFile = remoteFile;
			this.cacheFile = cacheFile;
			this.loggingDate = loggingDate;
		}

		public void Refresh()
		{
			remoteFile.Refresh();
			cacheFile.Refresh();
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			CacheStreamReader streamReader = new CacheStreamReader( cacheFile, remoteFile );
			StreamLogFileReader reader = new StreamLogFileReader( args, streamReader );
			return reader;
		}

		public int Length
		{
			get
			{
				int length = (int)remoteFile.Length;
				return length;
			}
		}

		public string Name
		{
			get { return remoteFile.Name; }
		}

		public string FullName
		{
			get { return remoteFile.FullName; }
		}

		public string Extension
		{
			get { return cacheFile.Extension; }
		}

		public DateTime LastWriteTime
		{
			get { return remoteFile.LastWriteTime; }
		}

		public DateTime LoggingDate
		{
			get { return loggingDate; }
		}
	}

	internal sealed class CacheStreamReader : IStreamProvider
	{
		private readonly FileInfo remoteFile;
		private readonly FileInfo cacheFile;

		public CacheStreamReader( FileInfo cacheFile, FileInfo remoteFile )
		{
			if ( cacheFile == null ) throw new ArgumentNullException( "cacheFile" );
			if ( remoteFile == null ) throw new ArgumentNullException( "remoteFile" );

			this.cacheFile = cacheFile;
			this.remoteFile = remoteFile;
		}

		public Stream OpenStream( int startPosition )
		{
			Stream remoteStream = OpenReadStream( remoteFile );
			Stream cacheWriteStream = OpenWriteStream( cacheFile );
			Stream cacheReadStream = OpenReadStream( cacheFile );

			CacheStream cacheStream = new CacheStream( remoteStream, cacheReadStream, cacheWriteStream, startPosition );
			return cacheStream;
		}

		private Stream GetReadStream( FileInfo fileInfo, FileMode mode )
		{
			return fileInfo.Open( mode, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
		}

		private Stream OpenReadStream( FileInfo fileInfo )
		{
			return GetReadStream( fileInfo, FileMode.Open );
		}

		private Stream OpenWriteStream( FileInfo fileInfo )
		{
			FileStream fs = new FileStream( fileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete );
			return fs;
		}
	}
}
