using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Security.AccessControl;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;

namespace LogAnalyzer
{
	public sealed class LogAnalyzerCore : LogEntriesList
	{
		private readonly LogAnalyzerConfiguration config;
		private readonly IOperationsQueue operationsQueue;

		public IOperationsQueue OperationsQueue
		{
			get { return operationsQueue; }
		}

		private readonly List<LogDirectory> directories = new List<LogDirectory>();
		private readonly ReadOnlyCollection<LogDirectory> readonlyDirectories;
		public IList<LogDirectory> Directories
		{
			get { return readonlyDirectories; }
		}

		internal override int TotalFilesCount
		{
			get { return directories.Sum( d => d.TotalFilesCount ); }
		}

		public LogAnalyzerConfiguration Config
		{
			get { return config; }
		}

		// todo в видах файла и папки не показывать столбцы, одинаковые для всех записей. То же и для потока и т.п.

		// todo заменить NotImplementedException на NotSupportedException
		// todo статистика по типам записей
		// todo записывать число исключений при чтении файлов
		// todo открывать в VS файл на нужной строке (где исключение)

		//string vsCommandLine = @"c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\devenv.exe";
		// todo не работает переход на заданную строку
		//string vsArgs = "/edit " + @"""C:\Development\HttpServer\HttpServer\Server.cs""" + " /command \"Edit.Find 123\"";//\"Edit.GoTo 150\"";

		//Process.Start(vsCommandLine, vsArgs);
		//Environment.Exit(0);

		private int directoriesLoadedCount;

		public LogAnalyzerCore( LogAnalyzerConfiguration config, IEnvironment environment )
			: base( environment, config.Logger )
		{
			if ( config == null )
				throw new ArgumentNullException( "config" );
			if ( config.Logger == null )
				throw new ArgumentNullException();
			if ( environment == null )
				throw new ArgumentNullException( "environment" );

			if ( config.Directories.Count == 0 )
				throw new ArgumentException( "Config should have at least one LogDirectory." );

			if ( !config.EnabledDirectories.Any() )
				throw new ArgumentException( "Config should have at least one enabled LogDirectory." );

			// todo обрабатывать null в элементах конфига
			this.config = config;

			logger.DebugWriteInfo( "" );
			logger.DebugWriteInfo( "" );
			logger.DebugWriteInfo( "" );

			foreach ( LogDirectoryConfigurationInfo dir in config.EnabledDirectories )
			{
				if ( String.IsNullOrWhiteSpace( dir.EncodingName ) )
				{
					dir.EncodingName = config.DefaultEncodingName;
				}

				var logDirectory = new LogDirectory( dir, config, environment, this );
				directories.Add( logDirectory );
			}

			this.readonlyDirectories = directories.AsReadOnly();

			this.loadedEvent = new CountdownEvent( directories.Count );

			foreach ( var dir in directories )
			{
				dir.Loaded += OnDirectoryLoaded;
				dir.ReadProgress += OnDirectoryReadProgress;
			}

			this.operationsQueue = environment.OperationsQueue;
		}

		private void OnDirectoryReadProgress( object sender, FileReadEventArgs e )
		{
			RaiseReadProgress( e );
		}

		private void OnDirectoryLoaded( object sender, EventArgs e )
		{
			Interlocked.Increment( ref directoriesLoadedCount );
			if ( directoriesLoadedCount == directories.Count )
			{
				PerformInitialMerge();
				RaiseLoadedEvent();
			}

			loadedEvent.Signal();
			LogDirectory dir = (LogDirectory)sender;
			dir.Loaded -= OnDirectoryLoaded;
		}

		protected override void StartImpl()
		{
			foreach ( var dir in Directories )
			{
				dir.Start();
			}
		}

		private readonly CountdownEvent loadedEvent;

		public void WaitForLoaded()
		{
			if ( IsLoaded )
				return;

			loadedEvent.Wait();
			loadedEvent.Dispose();
		}

		private void PerformInitialMerge()
		{
			// todo не сделать ли тут запас по Capacity?
			MergedEntriesList.Capacity = Directories.Sum( d => d.Files.Sum( f => f.LogEntries.Count ) );

			LogEntry[] sortedEntries = Directories
				.SelectMany( d => d.Files.SelectMany( f => f.LogEntries ) )
				.AsParallel()
				.OrderBy( LogEntryByDateComparer.Instance )
				.ToArray();

#if ASSERT
			Condition.DebugAssert( sortedEntries.AreSorted( LogEntryByDateComparer.Instance ) );
#endif

			// ILSpy: используется Buffer.BlockCopy
			MergedEntriesList.AddRange( sortedEntries );
			MergedEntries.RaiseCollectionReset();
			MessageSeverityCount.Update( sortedEntries );
		}

		public override int TotalLengthInBytes
		{
			get { return Directories.Sum( d => d.TotalLengthInBytes ); }
		}
	}
}
