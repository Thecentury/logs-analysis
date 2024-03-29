﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.Regions;
using ModuleLogsProvider.GUI.ViewModels;
using ModuleLogsProvider.GUI.Views;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Most;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace LogAnalyzer.Most.App
{
	public sealed class MostAppBootstrapper : Bootstrapper
	{
		private MostServerUrls GetSelectedUrls( MostLogAnalyzerConfiguration config )
		{
			MostServerUrls result = MostServerUrls.Local;

			if ( ArgsParser.ContainsSwitch( "server" ) )
			{
				string selectedServerTag = ArgsParser["server"];
				result = config.Urls.First( urls => urls.Tag == selectedServerTag );
			}

			return result;
		}

		protected override void Init()
		{
			string configName = ArgsParser.GetValueOrDefault( "config", "config.xaml" );

			string configPath = Path.Combine( Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ), configName );
			var config = MostLogAnalyzerConfiguration.LoadFromFile( configPath );
			InitConfig( config );

			MostServerUrls serverUrls = GetSelectedUrls( config );

			MostServiceFactory<ILogSourceService> serviceFactory = new MostServiceFactory<ILogSourceService>(
				new NetTcpBindingFactory(), serverUrls.LogsSourceServiceUrl );

			config
				.AddLogDirectory( "MOST", "*", "MOST" )
				.WithLogsUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 20 ) ) )
				.WithPerformanceDataUpdateTimer( new WpfDispatcherTimer( TimeSpan.FromSeconds( 2 ) ) )
				.SetSelectedUrls( serverUrls )
				.Register<ITimer>( () => new WpfDispatcherTimer() )
				.RegisterInstance<IServiceFactory<ILogSourceService>>( serviceFactory );

			config.ViewManager.RegisterView( typeof( PerformanceView ), typeof( ServerPerformanceViewModel ) );

			var environment = new MostEnvironment( config );

			MostApplicationViewModel applicationViewModel = new MostApplicationViewModel( config, environment );
			Application.Current.Dispatcher.BeginInvoke(
				() =>
				{
					var mainWindow = Application.Current.MainWindow;

					RegionManager.SetRegionManager( mainWindow, config.ResolveNotNull<RegionManager>() );
					mainWindow.DataContext = applicationViewModel;
				} );
		}
	}
}
