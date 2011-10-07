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
		private readonly MostAppBootstrapper bootstrapper = new MostAppBootstrapper();
		public MostAppBootstrapper Bootstrapper
		{
			get { return bootstrapper; }
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			Bootstrapper.Start();
		}

		//private void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		//{
		//    bootstrapper.Logger.WriteError( "Unhandled exception: {0}", e.ExceptionObject.ToString() );
		//    LogAnalyzer.Extensions.Condition.BreakIfAttached();
		//    MessageBox.Show( e.ExceptionObject.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );
		//    Environment.Exit( -1 );
		//}

		protected override void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			Bootstrapper.Logger.WriteInfo( "Exiting" );
		}
	}
}
