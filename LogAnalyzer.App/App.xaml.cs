﻿using System;
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
using LogAnalyzer.GUI.ViewModel;

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

			Thread.CurrentThread.Name = "UIThread";

			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			var args = e.Args;
			string configPath = Settings.Default.ConfigPath;

			if ( args.Length >= 1 )
			{
				string configPathArg = args.FirstOrDefault( s => s.StartsWith( "-c:" ) );
				if ( configPathArg != null )
				{
					configPath = configPathArg.Substring( 3 );
				}
			}

			Task task = new Task( () =>
			{
				// todo обработка исключений при ошибках загрузки конфига
				LogAnalyzerConfiguration config = LogAnalyzerConfiguration.LoadFromFile( configPath );

				this.logger = config.Logger;

				ApplicationViewModel applicationViewModel = new ApplicationViewModel( config );
				Application.Current.Dispatcher.BeginInvoke( () =>
				{
					Application.Current.MainWindow.DataContext = applicationViewModel;
				} );
			} );

			task.ContinueWith( t =>
			{
				logger.WriteError( "Crash. Exception: {0}", t.Exception );

				Extensions.Condition.BreakIfAttached();

				MessageBox.Show( "Unhandled exception: " + t.Exception.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

				Environment.Exit( -1 );

			}, TaskContinuationOptions.OnlyOnFaulted );

			task.Start();
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
