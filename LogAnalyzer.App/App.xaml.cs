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
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.Logging;

namespace LogAnalyzer.App
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private Logger logger;

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			AppBootstrapper bootstrapper = new AppBootstrapper();
			bootstrapper.Start( e.Args );
		}

		protected override void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			logger.WriteInfo( "Exiting" );
		}
	}
}
