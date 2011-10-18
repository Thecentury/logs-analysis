using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Regions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;
using ModuleLogsProvider.GUI.ViewModels;
using ModuleLogsProvider.GUI.Views;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace LogAnalyzer.Most.App
{
	public sealed class MostAppBootstrapper
	{
		private Logger logger;
		public Logger Logger
		{
			get { return logger; }
		}

		private string[] commandLineArgs;

		public void Start( string[] commandLineArgs )
		{
			this.commandLineArgs = commandLineArgs;

			Thread.CurrentThread.Name = "UIThread";

			Task.Factory
				.StartNew( Init )
				.ContinueWith( OnInitException, TaskContinuationOptions.OnlyOnFaulted );
		}

		private MostServerUrls GetSelectedUrls( MostLogAnalyzerConfiguration config )
		{
			MostServerUrls result = MostServerUrls.Local;

			string selectedServerString = commandLineArgs.FirstOrDefault( line => line.StartsWith( "-server:" ) );
			if ( selectedServerString != null )
			{
				string[] parts = selectedServerString.Split( ':' );
				string selectedServerTag = parts[1];

				result = config.Urls.First( urls => urls.Tag == selectedServerTag );
			}

			return result;
		}

		private void Init()
		{
			MostServiceFactory<ILogSourceService> serviceFactory = new MostServiceFactory<ILogSourceService>(
				new NetTcpBindingFactory(), MostServerUrls.Local.LogsSourceServiceUrl );

			const string dirName = "MOST";
			const string filesFilter = "*";
			const string displayName = "MOST.Local";

			var config = MostLogAnalyzerConfiguration
				.LoadFromFile( @"../../config.xaml" )
				.AcceptAllLogTypes();
			//.AddLogWriter( new DebugLogWriter() );

			logger = config.Logger;
			var operationsQueue = new WorkerThreadOperationsQueue( logger );

			RegionManager regionManager = new RegionManager();

			MostServerUrls serverUrls = GetSelectedUrls( config );

			config
				.AddLogDirectory( dirName, filesFilter, displayName )
				.WithLogsUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 20 ) ) )
				.WithPerformanceDataUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 2 ) ) )
				.SetSelectedUrls( serverUrls )
				.Register<ITimer>( () => new WpfDispatcherTimer() )
				.RegisterInstance<IServiceFactory<ILogSourceService>>( serviceFactory )
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<OperationScheduler>( OperationScheduler.TaskScheduler )
				.RegisterInstance<ErrorReportingServiceBase>( new LoggingErrorReportingService( Logger ) )
				.RegisterInstance<RegionManager>( regionManager );

			config.ViewManager.RegisterView( typeof( PerformanceView ), typeof( ServerPerformanceViewModel ) );

			var environment = new MostEnvironment( config );

			MostApplicationViewModel applicationViewModel = new MostApplicationViewModel( config, environment );
			Application.Current.Dispatcher.BeginInvoke( () =>
															{
																var mainWindow = Application.Current.MainWindow;

																RegionManager.SetRegionManager( mainWindow, regionManager );
																mainWindow.DataContext = applicationViewModel;
															} );
		}

		private void OnInitException( Task parentTask )
		{
			var exception = parentTask.Exception;

			Logger.WriteError( "Crash. Exception: {0}", exception );
			Extensions.Condition.BreakIfAttached();

			MessageBox.Show( "Unhandled exception: " + exception, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			Environment.Exit( -1 );
		}
	}
}
