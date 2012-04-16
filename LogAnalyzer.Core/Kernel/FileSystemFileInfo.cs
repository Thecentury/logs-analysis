using System;
using System.IO;
using JetBrains.Annotations;
using LogAnalyzer.Auxilliary;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public sealed class FileSystemFileInfo : IFileInfo
	{
		private readonly FileInfo _fileInfo;
		private LogFileReaderBase _reader;

		public FileSystemFileInfo( [NotNull] string path )
		{
			if ( path == null )
			{
				throw new ArgumentNullException( "path" );
			}
			
			_fileInfo = new FileInfo( path );
			
			if ( !_fileInfo.Exists )
			{
				throw new InvalidOperationException( String.Format( "File '{0}' doesn't exist.", path ) );
			}
		}

		public void Refresh()
		{
			_fileInfo.Refresh();
		}

		public LogFileReaderBase GetReader( LogFileReaderArguments args )
		{
			// todo brinchuk многопоточность?! Тут может быть доступ из разных потоков?
			if ( _reader == null )
			{
				FileSystemStreamProvider streamReader = new FileSystemStreamProvider( _fileInfo );
				_reader = new StreamLogFileReader( args, streamReader );
			}

			return _reader;
		}

		public Stream OpenStream()
		{
			return new FileStream( _fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete );
		}

		public long Length
		{
			get { return _fileInfo.Length; }
		}

		public string Name
		{
			get { return _fileInfo.Name; }
		}

		public string FullName
		{
			get { return _fileInfo.FullName; }
		}

		public string Extension
		{
			get { return _fileInfo.Extension; }
		}

		public DateTime LastWriteTime
		{
			get { return _fileInfo.LastWriteTime; }
		}

		public DateTime LoggingDate
		{
			get
			{
				// todo probably perform some kind of analysis here,
				// or use LastWriteTime, or smth like that
				throw new NotImplementedException();

				return _fileInfo.LastWriteTime.Date;
			}
		}
	}

	internal sealed class FileSystemStreamProvider : IStreamProvider
	{
		private readonly FileInfo _fileInfo;

		public FileSystemStreamProvider( FileInfo fileInfo )
		{
			if ( fileInfo == null )
				throw new ArgumentNullException( "fileInfo" );

			this._fileInfo = fileInfo;
		}

		public Stream OpenStream( int startPosition )
		{
			try
			{
				Stream stream = new FileStream( _fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 8192, FileOptions.SequentialScan );
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
