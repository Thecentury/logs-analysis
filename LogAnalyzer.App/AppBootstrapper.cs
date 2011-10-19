using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.App
{
	internal sealed class AppBootstrapper : Bootstrapper
	{
		protected override void Init()
		{
			string configPath = ArgsParser.GetValueOrDefault( "config", Settings.Default.ConfigPath );

			LogAnalyzerConfiguration config = LogAnalyzerConfiguration.LoadFromFile( configPath );
			InitConfig( config );

			var environment = new FileSystemEnvironment( config );
			ApplicationViewModel applicationViewModel = new ApplicationViewModel( config, environment );

			Application.Current.Dispatcher.BeginInvoke( () =>
			{
				Application.Current.MainWindow.DataContext = applicationViewModel;
			} );
		}
	}
}
