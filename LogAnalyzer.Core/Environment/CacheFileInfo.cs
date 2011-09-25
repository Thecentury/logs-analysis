using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace LogAnalyzer
{
	internal sealed class CacheFileInfo : IFileInfo
	{
		private readonly FileInfo remoteFile = null;
		private readonly FileInfo cacheFile = null;
		private readonly ICredentials credentials = null;
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

		private Stream OpenReadStream( FileInfo fileInfo )
		{
			return GetReadStream( fileInfo, FileMode.Open );
		}

		private Stream OpenOrCreateReadWriteStream( FileInfo fileInfo )
		{
			return fileInfo.Open( FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete );
		}

		private Stream GetReadStream( FileInfo fileInfo, FileMode mode )
		{
			return fileInfo.Open( mode, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
		}

		private Stream OpenWriteStream( FileInfo fileInfo )
		{
			FileStream fs = new FileStream( fileInfo.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete );
			return fs;
		}

		public Stream OpenStream( int startPosition )
		{
			Stream remoteStream = OpenReadStream( remoteFile );
			Stream cacheWriteStream = OpenWriteStream( cacheFile );
			Stream cacheReadStream = OpenReadStream( cacheFile );

			CacheStream cacheStream = new CacheStream( remoteStream, cacheReadStream, cacheWriteStream, startPosition );
			return cacheStream;
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
}
