using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
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

		private readonly MockTimer timer = new MockTimer();
		public MockTimer Timer
		{
			get { return timer; }
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			Thread.CurrentThread.Name = "UIThread";

			//AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			Task task = new Task( Init );
			task.ContinueWith( t =>
			{
				logger.WriteError( "Crash. Exception: {0}", t.Exception );

				Extensions.Condition.BreakIfAttached();

				MessageBox.Show( "Unhandled exception: " + t.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

				Environment.Exit( -1 );

			}, TaskContinuationOptions.OnlyOnFaulted );

			task.Start();
		}

		private void Init()
		{
			MostEnvironment env;

			MostServerLogSourceFactory serviceFactory = new MostServerLogSourceFactory();

			LogAnalyzerConfiguration config = null;

			config = LogAnalyzerConfiguration
				.CreateNew()
				.AcceptAllLogTypes()
				.AddLogDirectory( "MOST", "*", "MOST-Display-Name" )
				.Register<IEnvironment>( () => env = new MostEnvironment( config ) )
				.RegisterInstance<ILogSourceServiceFactory>( serviceFactory )
				.RegisterInstance<ITimer>( timer )
				.Register<IOperationsQueue>( () => new WorkerThreadOperationsQueue( logger ) )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.BuildConfig();

			logger = config.Logger;

			ApplicationViewModel applicationViewModel = new ApplicationViewModel( config );
			Application.Current.Dispatcher.BeginInvoke( () => { Application.Current.MainWindow.DataContext = applicationViewModel; } );
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
