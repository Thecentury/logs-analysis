using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Most.App.Properties;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Mocks;
using ModuleLogsProvider.Logging.Most;

namespace LogAnalyzer.Most.App
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private Logger logger;

		private readonly ITimer timer = new WpfDispatcherTimer( TimeSpan.FromSeconds( 20 ) );
		public ITimer Timer
		{
			get { return timer; }
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			Thread.CurrentThread.Name = "UIThread";

			//AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			Task.Factory
				.StartNew( Init )
				.ContinueWith( OnInitException, TaskContinuationOptions.OnlyOnFaulted );
		}

		private void Init()
		{
			const string logsServiceAddress = "http://127.0.0.1:9999/MostLogSourceService/";
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
				.Register<IOperationsQueue>( () => new WorkerThreadOperationsQueue( logger ) )
				.RegisterInstance<IWindowService>( new RealWindowService() );

			logger = config.Logger;
			var environment = new MostEnvironment( config );

			ApplicationViewModel applicationViewModel = new ApplicationViewModel( config, environment );
			Application.Current.Dispatcher.BeginInvoke( () => { Application.Current.MainWindow.DataContext = applicationViewModel; } );
		}

		private void OnInitException( Task parentTask )
		{
			var exception = parentTask.Exception;

			logger.WriteError( "Crash. Exception: {0}", exception );
			Extensions.Condition.BreakIfAttached();

			MessageBox.Show( "Unhandled exception: " + exception, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			Environment.Exit( -1 );
		}

		private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			logger.WriteError( "Unhandled exception: {0}", e.ExceptionObject.ToString() );

			LogAnalyzer.Extensions.Condition.BreakIfAttached();

			MessageBox.Show( e.ExceptionObject.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			Environment.Exit( -1 );
		}

		protected override void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			logger.WriteInfo( "Exiting" );
		}
	}
}
