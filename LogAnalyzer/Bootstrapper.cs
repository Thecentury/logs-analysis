using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Auxilliary;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Regions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.LoggingTemplates;
using LogAnalyzer.Misc;
using LogAnalyzer.Operations;

namespace LogAnalyzer.GUI
{
	public abstract class Bootstrapper
	{
		private Logger _logger;
		public Logger Logger
		{
			get { return _logger; }
		}

		private string[] _commandLineArgs;
		public string[] CommandLineArgs
		{
			get { return _commandLineArgs; }
		}

		private CommandLineArgumentsParser _argsParser;
		protected CommandLineArgumentsParser ArgsParser
		{
			get { return _argsParser; }
		}

		private readonly DirectoryManager _directoryManager = new DirectoryManager();
		protected DirectoryManager DirectoryManager
		{
			get { return _directoryManager; }
		}

		public void Start( string[] commandLineArgs )
		{
			if ( commandLineArgs == null )
			{
				throw new ArgumentNullException( "commandLineArgs" );
			}

			Logger logger = Logger.Instance;
			logger.WriteInfo( "Bootstrapper.Start()" );
			TimerStorage.Instance.StartTimer( "AppStartup" );

			this._commandLineArgs = commandLineArgs;

			_argsParser = new CommandLineArgumentsParser( commandLineArgs );

			Thread.CurrentThread.Name = "UIThread";

			Task.Factory
				.StartNew( Init )
				.ContinueWith( OnInitException, TaskContinuationOptions.OnlyOnFaulted );
		}

		protected abstract void Init();

		private void OnInitException( Task parentTask )
		{
			var exception = parentTask.Exception;

			LogAnalyzer.Extensions.Condition.BreakIfAttached();
			Logger.WriteError( "Crash. Exception: {0}", exception );

			MessageBox.Show( "Unhandled exception: " + exception, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			Environment.Exit( -1 );
		}

		/// <summary>
		/// Обрабатывает пути к файлам логов, переданные как аргументы командной строки.
		/// </summary>
		/// <param name="config"></param>
		protected void HandleOpenWithCalls( LogAnalyzerConfiguration config )
		{
			var paths = GetPathsFromCommandArgs( CommandLineArgs )
				.Where( item => item.Extension != Constants.ProjectExtension )
				.ToList();

			if ( paths.Count > 0 )
			{
				config.Directories.Clear();
				foreach ( var dir in paths.OfType<DirectoryInfo>() )
				{
					config.Directories.Add( new LogDirectoryConfigurationInfo( dir.FullName, dir.Name )
					{
						EncodingName = config.DefaultEncodingName
					} );
				}

				var files = paths
					.OfType<FileInfo>()
					.Where( f => f.Extension == ".log" )
					.ToList();

				if ( files.Count > 0 )
				{
					var directoryForSeparateFiles = new LogDirectoryConfigurationInfo( "Files", "Files" ) { EncodingName = config.DefaultEncodingName };

					directoryForSeparateFiles.PredefinedFiles.AddRange( files.Select( f => f.FullName ) );

					config.Directories.Add( directoryForSeparateFiles );
				}

				var zipFiles = paths.OfType<FileInfo>().Where( f => f.Extension == ".zip" ).ToList();
				if ( zipFiles.Count > 0 )
				{
					foreach ( FileInfo zipFile in zipFiles )
					{
						var zipDir = new LogDirectoryConfigurationInfo( zipFile.FullName, zipFile.Name ) { IncludeNestedDirectories = true };
						config.Directories.Add( zipDir );
					}
				}
			}
		}

		protected List<FileSystemInfo> GetPathsFromCommandArgs( IEnumerable<string> commandLineArgs )
		{
			List<FileSystemInfo> result = new List<FileSystemInfo>();

			foreach ( string arg in commandLineArgs )
			{
				bool isForConfig = arg.StartsWith( "/" ) && arg.Contains( ":" );
				if ( isForConfig )
				{
					continue;
				}

				if ( File.Exists( arg ) )
				{
					result.Add( new FileInfo( arg ) );
				}
				else if ( Directory.Exists( arg ) )
				{
					result.Add( new DirectoryInfo( arg ) );
				}
			}

			return result;
		}

		protected void InitConfig( LogAnalyzerConfiguration config )
		{
			this._logger = config.Logger;

			var operationsQueue = new WorkerThreadOperationsQueue( _logger );

			DirectoryManager.RegisterCommonFactories();

			Lazy<LogEntryFormatRecognizer> recognizerLazy = new Lazy<LogEntryFormatRecognizer>( () =>
																									{
																										using ( var fs = new FileStream( @"LoggingTemplates\usages.xml", FileMode.Open, FileAccess.Read ) )
																										{
																											var usages = LoggerUsageInAssembly.Deserialize( fs );
																											return new LogEntryFormatRecognizer( usages );
																										}
																									}, true );
			config
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance( OperationScheduler.TaskScheduler )
				.RegisterInstance<IErrorReportingService>( new ErrorReportingService() )
				.RegisterInstance( new RegionManager() )
				.RegisterInstance<IDirectoryFactory>( DirectoryManager )
				.RegisterInstance<ITimeService>( new NeverOldTimeService() )
				.RegisterInstance<IFileSystem>( new RealFileSystem() )
				.Register<ISaveToStreamDialog>( () => new UISaveFileDialog() )
				.Register<ILogEntryFormatRecognizer>( () => recognizerLazy.Value );
		}
	}
}
