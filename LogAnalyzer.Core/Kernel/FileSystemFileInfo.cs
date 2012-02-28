using System;
using System.IO;
using JetBrains.Annotations;
using LogAnalyzer.Auxilliary;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	internal sealed class FileSystemFileInfo : IFileInfo
	{
		private readonly FileInfo fileInfo;
		private LogFileReaderBase reader;

		public FileSystemFileInfo( [NotNull] string path )
		{
			if ( path == null ) throw new ArgumentNullException( "path" );
			fileInfo = new FileInfo( path );
			if ( !fileInfo.Exists )
				throw new InvalidOperationException( string.Format( "File '{0}' doesn't exist.", path ) );
		}

		public void Refresh()
		{
			fileInfo.Refresh();
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			// todo brinchuk многопоточность?! Тут может быть доступ из разных потоков?
			if ( reader == null )
			{
				FileSystemStreamReader streamReader = new FileSystemStreamReader( fileInfo );
				reader = new StreamLogFileReader( args, streamReader );
			}

			return reader;
		}

		public long Length
		{
			get { return fileInfo.Length; }
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
		private readonly FileInfo _fileInfo;

		public FileSystemStreamReader( FileInfo fileInfo )
		{
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );

			this._fileInfo = fileInfo;
		}

		public Stream OpenStream( int startPosition )
		{
			try
			{
				Stream stream = new FileStream(_fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 8192, FileOptions.SequentialScan );
				stream.Position = startPosition;

				if ( KeyValueStorage.Instance.Contains( "FileSystemStreamReaderTransformer" ) )
				{
					var transformer = (ITransformer<Stream>)KeyValueStorage.Instance["FileSystemStreamReaderTransformer"];
					stream = transformer.Transform( stream );
				}

				return stream;
			}
			catch ( IOException exc )
			{
				throw new LogAnalyzerIOException( String.Format( "Ошибка при открытии файла '{0}'", _fileInfo.FullName ), exc );
			}
		}
	}
}
