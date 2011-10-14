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
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;
using ModuleLogsProvider.GUI.ViewModels;
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

		private readonly ITimer timer = new WpfDispatcherTimer( TimeSpan.FromSeconds( 20 ) );
		public ITimer Timer
		{
			get { return timer; }
		}

		public void Start()
		{
			Thread.CurrentThread.Name = "UIThread";

			//AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			Task.Factory
				.StartNew( Init )
				.ContinueWith( OnInitException, TaskContinuationOptions.OnlyOnFaulted );
		}

		private void Init()
		{
			MostServiceFactory<ILogSourceService> serviceFactory = new MostServiceFactory<ILogSourceService>( new NetTcpBindingFactory(), MostServerUrls.Local.LogsSourceServiceUrl );

			const string dirName = "MOST";
			const string filesFilter = "*";
			const string displayName = "MOST.Local";

			var config = MostLogAnalyzerConfiguration
				.LoadFromFile( @"..\..\config.xaml" )
				.AcceptAllLogTypes();

			logger = config.Logger;
			var operationsQueue = new WorkerThreadOperationsQueue( logger );

			config
				.AddLogDirectory( dirName, filesFilter, displayName )
				.WithLogsUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 20 ) ) )
				.WithPerformanceDataUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 2 ) ) )
				.SetSelectedUrls( MostServerUrls.Local )
				.RegisterInstance<IFactory<IDisposableService<ILogSourceService>>>( serviceFactory )
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<OperationScheduler>( OperationScheduler.TaskScheduler )
				.RegisterInstance<IErrorReportingService>( new LoggingErrorReportingService( Logger ) );

			var environment = new MostEnvironment( config );

			MostApplicationViewModel applicationViewModel = new MostApplicationViewModel( config, environment );
			Application.Current.Dispatcher.BeginInvoke( () => { Application.Current.MainWindow.DataContext = applicationViewModel; } );
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
