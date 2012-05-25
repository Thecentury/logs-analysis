using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using JetBrains.Annotations;
using LogAnalyzer.Auxilliary;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Zip;

namespace LogAnalyzer.App
{
	internal sealed class AppBootstrapper : Bootstrapper
	{
		protected override void Init()
		{
			Logger logger = Logger.Instance;
			logger.WriteInfo( "Bootstrapper.Init()" );

			AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
			TaskScheduler.UnobservedTaskException += TaskSchedulerUnobservedTaskException;

			bool breakAtStart = Properties.Settings.Default.BreakAtStart;
			if ( breakAtStart )
			{
				Debugger.Launch();
			}

			var config = LoadConfig();

			SetDebugParameters();
			InitConfig( config );
			DirectoryManager.RegisterFactory( new ZipDirectoryFactory() );
			HandleOpenWithCalls( config );

			var environment = new FileSystemEnvironment( config );
			ApplicationViewModel applicationViewModel = new ApplicationViewModel( config, environment );

			Application.Current.Dispatcher.BeginInvoke( () =>
			{
				Application.Current.MainWindow.DataContext = applicationViewModel;
			} );
		}

		private void TaskSchedulerUnobservedTaskException( object sender, UnobservedTaskExceptionEventArgs e )
		{
			if ( Debugger.IsAttached )
			{
				Debugger.Break();
			}

			Logger.WriteLine( MessageType.Error, "TaskSchdeduler - Unhandled exception: " + e.Exception );
			MessageBox.Show( e.Exception.ToString(), "Task Scheduler unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			e.SetObserved();
		}

		private LogAnalyzerConfiguration LoadConfig()
		{
			string exeLocation = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );
			if ( exeLocation == null )
			{
				throw new InvalidOperationException( "Exe location should not be null." );
			}

			string settingsSubPath = Properties.Settings.Default.ConfigPath;
			string defaultSettingsPath = Path.GetFullPath( Path.Combine( exeLocation, settingsSubPath ) );
			var paths = GetPathsFromCommandArgs( CommandLineArgs );
			string projectFile = paths.OfType<FileInfo>().Select( f => f.FullName ).FirstOrDefault( f => Path.GetExtension( f ) == Constants.ProjectExtension );

			string configPath = ArgsParser.GetValueOrDefault( "config", defaultSettingsPath );
			if ( projectFile != null )
			{
				configPath = projectFile;
			}
			else
			{
				if ( AppDomain.CurrentDomain.SetupInformation.ActivationArguments != null )
				{
					var activationData = AppDomain.CurrentDomain.SetupInformation.ActivationArguments.ActivationData;
					if ( activationData != null )
					{
						projectFile =
							activationData
								.FirstOrDefault( f => Path.GetExtension( f ) == Constants.ProjectExtension );
						if ( projectFile != null )
						{
							configPath = projectFile;
						}
					}
				}
			}

			LogAnalyzerConfiguration config;
			bool configPathExists = File.Exists( configPath );
			if ( !configPathExists )
			{
				configPath = Path.Combine( exeLocation, configPath );
				configPathExists = File.Exists( configPath );
			}


			if ( configPathExists )
			{
				config = LogAnalyzerConfiguration.LoadFromFile( configPath );

				if ( projectFile != null )
				{
					SettingsHelper.AddProjectToRecent( projectFile );
				}
			}
			else
			{
				string args = Environment.CommandLine;
				string[] parts = args.Split( ' ' );

				string imagePath = parts[0].Trim( '"' );
				string currentDirectory = Environment.CurrentDirectory;
				bool launchedFromOtherDirectory = !String.Equals( Path.GetDirectoryName( imagePath ), currentDirectory );

				if ( !launchedFromOtherDirectory )
				{
					config = new LogAnalyzerConfiguration()
						.AcceptAllLogTypes()
						.AddLogWriter( new FileLogWriter( Path.Combine( exeLocation, "log.log" ) ) );

					config.Logger.WriteLine( MessageType.Warning, string.Format( "Config not found at '{0}'", configPath ) );
				}
				else
				{
					config = new LogAnalyzerConfiguration()
						.AddLoggerAcceptedMessageType( MessageType.Warning )
						.AddLoggerAcceptedMessageType( MessageType.Info )
						.AddLoggerAcceptedMessageType( MessageType.Error )
						.AddLogWriter( new FileLogWriter( Path.Combine( exeLocation, "log.log" ) ) );

					string currentDirectoryName = Path.GetFileNameWithoutExtension( currentDirectory );

					LogDirectoryConfigurationInfo dir = new LogDirectoryConfigurationInfo( currentDirectory, currentDirectoryName );

					config.AddLogDirectory( dir );

					List<CommandLineSwitch> switches = new List<CommandLineSwitch>();
					CommandLineSwitch lastSwitch = null;
					foreach ( string commandLinePart in parts )
					{
						if ( commandLinePart.StartsWith( "-" ) )
						{
							string cleanedName = commandLinePart.TrimStart( '-' );
							lastSwitch = new CommandLineSwitch( cleanedName );
							switches.Add( lastSwitch );
						}
						else
						{
							if ( lastSwitch != null )
							{
								lastSwitch.Parameters.Add( commandLinePart );
							}
						}
					}

					CommandLineFilterBuilder filterBuilder = new CommandLineFilterBuilder();
					config.GlobalLogEntryFilterBuilder = filterBuilder.CreateLogEntryFilter( switches );
					config.GlobalFilesFilterBuilder = filterBuilder.CreateFileFilter( switches );
				}
			}

			return config;
		}

		private void CurrentDomainUnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			Logger.WriteLine( MessageType.Error, "AppDomain - Unhandled exception: " + e.ExceptionObject );
			MessageBox.Show( e.ExceptionObject.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );
		}

		private void SetDebugParameters()
		{
			// замедление чтения
			//KeyValueStorage.Instance.Add( "FileSystemStreamReaderTransformer",
			//    new SlowStreamTransformer( TimeSpan.FromMilliseconds( 1 ) ) );
		}
	}

	public sealed class CommandLineSwitch
	{
		public CommandLineSwitch( [NotNull] string name )
		{
			if ( name == null )
			{
				throw new ArgumentNullException( "name" );
			}
			Name = name;
		}

		public string Name { get; set; }

		private readonly List<string> _parameters = new List<string>();
		public List<string> Parameters
		{
			get { return _parameters; }
		}
	}

	public static class CommandLineSwitchesCollectionExtensions
	{
		public static CommandLineSwitch GetSwitchByName( this IEnumerable<CommandLineSwitch> switches, string lowerName )
		{
			return switches.FirstOrDefault( s => s.Name.ToLowerInvariant() == lowerName.ToLowerInvariant() );
		}
	}

	public sealed class CommandLineFilterBuilder
	{
		public ExpressionBuilder CreateLogEntryFilter( List<CommandLineSwitch> switches )
		{
			var typesSwitch = switches.GetSwitchByName( "types" );
			if ( typesSwitch != null )
			{
				string messageTypes = typesSwitch.Parameters[0];
				if ( messageTypes.Length < 1 )
				{
					throw new ArgumentException( "MessageTypes switch should have at least one argument." );
				}
				else if ( messageTypes.Length == 1 )
				{
					return new MessageTypeEquals( messageTypes );
				}
				else
				{
					return new OrCollection(
						messageTypes
						.Select( c => c.ToString( CultureInfo.InvariantCulture ) )
						.Select( s => new MessageTypeEquals( s ) ) );
				}
			}

			return new AlwaysTrue();
		}

		public ExpressionBuilder CreateFileFilter( List<CommandLineSwitch> switches )
		{
			List<ExpressionBuilder> builders = new List<ExpressionBuilder>();

			var todaySwitch = switches.GetSwitchByName( "today" );
			if ( todaySwitch != null )
			{
				builders.Add( new Equals( new GetDateByFileNameBuilder(), new DateTimeConstant( DateTime.Today ) ) );
			}

			var sizeLessSwitch = switches.GetSwitchByName( "sizeLess" );
			if ( sizeLessSwitch != null )
			{
				if ( sizeLessSwitch.Parameters.Count != 1 )
				{
					throw new ArgumentException( "SizeLess switch is missing required argument: size in Kb." );
				}

				int kiloBytes = Int32.Parse( sizeLessSwitch.Parameters[0] );

				builders.Add( new SizeLessThanFilter { Megabytes = kiloBytes } );
			}

			if ( builders.Count == 0 )
			{
				return new AlwaysTrue();
			}
			else
			{
				return ExpressionBuilder.CreateAnd( builders );
			}
		}
	}
}
