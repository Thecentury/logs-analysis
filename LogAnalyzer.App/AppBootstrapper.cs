using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Auxilliary;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
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
			var args = GetPathsFromCommandArgs( CommandLineArgs );
			string projectFile = args.OfType<FileInfo>().Select( f => f.FullName ).FirstOrDefault( f => Path.GetExtension( f ) == Constants.ProjectExtension );

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
				config = new LogAnalyzerConfiguration()
					.AcceptAllLogTypes()
					.AddLogWriter( new FileLogWriter( Path.Combine( exeLocation, "log.log" ) ) );

				config.Logger.WriteLine( MessageType.Warning, string.Format( "Config not found at '{0}'", configPath ) );
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
}
