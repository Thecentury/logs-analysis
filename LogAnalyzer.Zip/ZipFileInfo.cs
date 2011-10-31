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
		private readonly string zipFileName;
		private readonly string zipEntryName;
		private readonly ZipEntry zipEntry;

		public ZipFileInfo( [NotNull] string zipFileName, [NotNull] string zipEntryName )
		{
			if ( zipFileName == null ) throw new ArgumentNullException( "zipFileName" );
			if ( zipEntryName == null ) throw new ArgumentNullException( "zipEntryName" );

			this.zipFileName = zipFileName;
			this.zipEntryName = zipEntryName;

			var zipFile = new ZipFile( zipFileName );
			this.zipEntry = zipFile.Entries.First( z => z.FileName == zipEntryName );
		}

		public void Refresh()
		{
			// do nothing
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			return new StreamLogFileReader( args, new ZipStreamProvider( zipEntry ) );
		}

		public int Length
		{
			get { return (int)zipEntry.UncompressedSize; }
		}

		public string Name
		{
			get { return Path.GetFileName( zipEntryName ); }
		}

		public string FullName
		{
			get { return zipEntryName; }
		}

		public string Extension
		{
			get { return Path.GetExtension( zipEntryName ); }
		}

		private sealed class ZipStreamProvider : IStreamProvider
		{
			private readonly ZipEntry zipEntry;

			public ZipStreamProvider( [NotNull] ZipEntry zipEntry )
			{
				if ( zipEntry == null ) throw new ArgumentNullException( "zipEntry" );
				this.zipEntry = zipEntry;
			}

			public Stream OpenStream( int startPosition )
			{
				// todo brinchuk this is not the best solution
				MemoryStream memoryStream = new MemoryStream( (int)zipEntry.UncompressedSize );
				zipEntry.Extract( memoryStream );
				memoryStream.Position = startPosition;

				return memoryStream;
			}
		}
	}
}
