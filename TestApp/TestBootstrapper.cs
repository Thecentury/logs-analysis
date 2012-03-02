using System;
using System.IO;
using System.Reflection;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Tests.Mocks;

namespace TestApp
{
	public sealed class TestBootstrapper : Bootstrapper
	{
		internal MockEnvironment Environment;
		internal ApplicationViewModel ApplicationViewModel;

		protected override void Init()
		{
			string exeLocation = Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location );

			var config = new LogAnalyzerConfiguration()
								.AddLoggerAcceptedMessageType( MessageType.Error )
								.AddLoggerAcceptedMessageType( MessageType.Warning )
								.AddLoggerAcceptedMessageType( MessageType.Info )
								.AddLogDirectory( new LogDirectoryConfigurationInfo( "Dir1", "*", "Dir1" )
								{
									EncodingName = "utf-16",
									LineParser = new ConfigurableLineParser
									{
										LogLineRegexText = @"^\[(?<Type>.)] \[(?<TID>.{3,4})] (?<Time>\d{2}\.\d{2}\.\d{4} \d{1,2}:\d{2}:\d{2}\.\d{3})\t(?<Text>.*)$",
										DateTimeFormat = "dd.MM.yyyy H:mm:ss.fff"
									}
								} )
								.AddLogWriter( new FileLogWriter( Path.Combine( exeLocation, "log.log" ) ) );

			InitConfig( config );

			Environment = new MockEnvironment( config );

			MockDirectoryInfo directory = (MockDirectoryInfo)Environment.GetDirectory( "Dir1" );
			directory.CreateRecordsSourceHandler = inner => inner;// new DelayedLogRecordsSource( inner, TimeSpan.FromSeconds( 1 ) );

			ApplicationViewModel = new ApplicationViewModel( config, Environment );

			Application.Current.Dispatcher.BeginInvoke( () =>
			{
				Application.Current.MainWindow.DataContext = ApplicationViewModel;
			} );
		}
	}
}
