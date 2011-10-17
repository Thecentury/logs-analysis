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

		public void Start()
		{
			Thread.CurrentThread.Name = "UIThread";

			Task.Factory
				.StartNew( Init )
				.ContinueWith( OnInitException, TaskContinuationOptions.OnlyOnFaulted );
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

			config
				.AddLogDirectory( dirName, filesFilter, displayName )
				.WithLogsUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 20 ) ) )
				.WithPerformanceDataUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 2 ) ) )
				.SetSelectedUrls( MostServerUrls.Local )
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
