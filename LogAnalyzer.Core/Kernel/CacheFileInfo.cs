using System;
using System.IO;

namespace LogAnalyzer.Kernel
{
	internal sealed class CacheFileInfo : IFileInfo
	{
		private readonly FileInfo _remoteFile;
		private readonly FileInfo _cacheFile;
		private readonly DateTime _loggingDate;
		private readonly CacheStreamProvider _streamReader;

		public CacheFileInfo( FileInfo remoteFile, FileInfo cacheFile, DateTime loggingDate )
		{
			if ( remoteFile == null )
				throw new ArgumentNullException( "remoteFile" );
			if ( cacheFile == null )
				throw new ArgumentNullException( "cacheFile" );

			_remoteFile = remoteFile;
			_cacheFile = cacheFile;
			_loggingDate = loggingDate;

			_streamReader = new CacheStreamProvider( _cacheFile, _remoteFile );
		}

		public void Refresh()
		{
			_remoteFile.Refresh();
			_cacheFile.Refresh();
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			StreamLogFileReader reader = new StreamLogFileReader( args, _streamReader );
			return reader;
		}

		public Stream OpenStream()
		{
			return _streamReader.OpenStream();
		}

		public long Length
		{
			get { return _remoteFile.Length; }
		}

		public string Name
		{
			get { return _remoteFile.Name; }
		}

		public string FullName
		{
			get { return _remoteFile.FullName; }
		}

		public string Extension
		{
			get { return _cacheFile.Extension; }
		}

		public DateTime LastWriteTime
		{
			get { return _remoteFile.LastWriteTime; }
		}

		public DateTime LoggingDate
		{
			get { return _loggingDate; }
		}
	}

	public static class StreamProviderExtensions
	{
		public static Stream OpenStream( this IStreamProvider streamProvider )
		{
			return streamProvider.OpenStream( 0 );
		}
	}

	internal sealed class CacheStreamProvider : IStreamProvider
	{
		private readonly FileInfo _remoteFile;
		private readonly FileInfo _cacheFile;

		public CacheStreamProvider( FileInfo cacheFile, FileInfo remoteFile )
		{
			if ( cacheFile == null ) throw new ArgumentNullException( "cacheFile" );
			if ( remoteFile == null ) throw new ArgumentNullException( "remoteFile" );

			this._cacheFile = cacheFile;
			this._remoteFile = remoteFile;
		}

		public Stream OpenStream( int startPosition )
		{
			Stream remoteStream = OpenReadStream( _remoteFile );
			Stream cacheWriteStream = OpenWriteStream( _cacheFile );
			Stream cacheReadStream = OpenReadStream( _cacheFile );

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
