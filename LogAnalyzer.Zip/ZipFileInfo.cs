using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using JetBrains.Annotations;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Zip
{
	internal sealed class ZipFileInfo : IFileInfo
	{
		[UsedImplicitly]
		private readonly string _zipFileName;
		private readonly string _zipEntryName;
		private readonly ZipEntry _zipEntry;
		private readonly ZipStreamProvider _streamProvider;

		public ZipFileInfo( [NotNull] string zipFileName, [NotNull] string zipEntryName )
		{
			if ( zipFileName == null ) throw new ArgumentNullException( "zipFileName" );
			if ( zipEntryName == null ) throw new ArgumentNullException( "zipEntryName" );

			_zipFileName = zipFileName;
			_zipEntryName = zipEntryName;

			var zipFile = new ZipFile( zipFileName );
			_zipEntry = zipFile.Entries.First( z => z.FileName == zipEntryName );

			_streamProvider = new ZipStreamProvider( _zipEntry );
		}

		public void Refresh()
		{
			// do nothing
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			return new StreamLogFileReader( args, _streamProvider );
		}

		public Stream OpenStream()
		{
			return _streamProvider.OpenStream();
		}

		public long Length
		{
			get { return _zipEntry.UncompressedSize; }
		}

		public string Name
		{
			get { return Path.GetFileName( _zipEntryName ); }
		}

		public string FullName
		{
			get { return _zipEntryName; }
		}

		public string Extension
		{
			get { return Path.GetExtension( _zipEntryName ); }
		}

		private sealed class ZipStreamProvider : IStreamProvider
		{
			private readonly ZipEntry _zipEntry;

			public ZipStreamProvider( [NotNull] ZipEntry zipEntry )
			{
				if ( zipEntry == null ) throw new ArgumentNullException( "zipEntry" );
				this._zipEntry = zipEntry;
			}

			public Stream OpenStream( int startPosition )
			{
				// todo brinchuk this is not the best solution
				MemoryStream memoryStream = new MemoryStream( (int)_zipEntry.UncompressedSize );
				_zipEntry.Extract( memoryStream );
				memoryStream.Position = startPosition;

				return memoryStream;
			}
		}
	}
}
