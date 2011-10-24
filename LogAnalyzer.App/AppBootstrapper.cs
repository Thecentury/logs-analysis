using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.Properties;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;

namespace LogAnalyzer.App
{
	internal sealed class AppBootstrapper : Bootstrapper
	{
		protected override void Init()
		{
			string exeLocation = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

			string settingsSubPath = Settings.Default.ConfigPath;
			string defaultSettingsPath =
				Path.GetFullPath( Path.Combine( exeLocation, settingsSubPath ) );
			string configPath = ArgsParser.GetValueOrDefault( "config", defaultSettingsPath );

			LogAnalyzerConfiguration config;
			bool configPathExists = File.Exists( configPath );
			if ( configPathExists )
			{
				config = LogAnalyzerConfiguration.LoadFromFile( configPath );
			}
			else
			{
				Logger.WriteLine( MessageType.Warning, string.Format( "Config not found at '{0}'", configPath ) );
				config = new LogAnalyzerConfiguration()
					.AcceptAllLogTypes()
					.AddLogWriter( new FileLogWriter( Path.Combine( exeLocation, "log.log" ) ) );
			}

			Debugger.Launch();

			InitConfig( config );
			HandleOpenWithCalls( config );

			var environment = new FileSystemEnvironment( config );
			ApplicationViewModel applicationViewModel = new ApplicationViewModel( config, environment );

			Application.Current.Dispatcher.BeginInvoke( () =>
			{
				Application.Current.MainWindow.DataContext = applicationViewModel;
			} );
		}

		private void HandleOpenWithCalls( LogAnalyzerConfiguration config )
		{
			var paths = GetPathsFromCommandArgs( CommandLineArgs );
			if ( paths.Count > 0 )
			{
				config.Directories.Clear();
				foreach ( var dir in paths.OfType<DirectoryInfo>() )
				{
					config.Directories.Add( new LogDirectoryConfigurationInfo( dir.FullName, "*", dir.Name ) );
				}

				var files = paths.OfType<FileInfo>().ToList();
				if ( files.Count > 0 )
				{
					LogDirectoryConfigurationInfo directoryForSeparateFiles = new LogDirectoryConfigurationInfo( "Files", "*", "Files" )
					{
						EncodingName = config.DefaultEncodingName
					};

					directoryForSeparateFiles.PredefinedFiles.AddRange( files.Select( f => f.FullName ) );

					config.Directories.Add( directoryForSeparateFiles );
				}
			}
		}

		private List<FileSystemInfo> GetPathsFromCommandArgs( IEnumerable<string> commandLineArgs )
		{
			List<FileSystemInfo> result = new List<FileSystemInfo>();

			foreach ( string arg in commandLineArgs )
			{
				if ( File.Exists( arg ) )
				{
					result.Add( new FileInfo( arg ) );
				}
				else if ( Directory.Exists( arg ) )
				{
					result.Add( new DirectoryInfo( arg ) );
				}
			}

			return result;
		}
	}
}
