using System;
using System.Collections.Generic;
using System.IO;
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
					LogDirectoryConfigurationInfo directoryForSeparateFiles = new LogDirectoryConfigurationInfo( "Files", "*", "Files" );
					directoryForSeparateFiles.PredefinedFiles.AddRange( files.Select( f => f.FullName ) );
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
