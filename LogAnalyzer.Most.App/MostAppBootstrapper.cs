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
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Most;

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
			const string logsServiceAddress = "net.tcp://127.0.0.1:9999/MostLogSourceService/";
			MostServerLogSourceFactory serviceFactory = new MostServerLogSourceFactory( logsServiceAddress );

			const string dirName = "MOST";
			const string filesFilter = "*";
			const string displayName = "MOST.Local";

			var config = LogAnalyzerConfiguration
				.CreateNew()
				.AcceptAllLogTypes()
				.AddLogDirectory( dirName, filesFilter, displayName )
				.RegisterInstance<ILogSourceServiceFactory>( serviceFactory )
				.RegisterInstance<ITimer>( timer )
				.Register<IOperationsQueue>( () => new WorkerThreadOperationsQueue( Logger ) )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<IErrorReportingService>( new NullErrorReportingService() );

			logger = config.Logger;
			var environment = new MostEnvironment( config );

			ApplicationViewModel applicationViewModel = new ApplicationViewModel( config, environment );
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
