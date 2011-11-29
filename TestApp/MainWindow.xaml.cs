using System;
using System.Collections.Generic;
using System.Windows;
using LogAnalyzer.Tests.Mocks;

namespace TestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private TestBootstrapper bootstrapper;
		private readonly List<LogAnalyzer.Tests.Mocks.MockLogWriter> writers = new List<LogAnalyzer.Tests.Mocks.MockLogWriter>();
		private const int millis = 100;

		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			App app = (App)Application.Current;
			bootstrapper = app.Bootstrapper;
		}

		private void StartButton_Click( object sender, RoutedEventArgs e )
		{
			MockDirectoryInfo dir1 = (MockDirectoryInfo)bootstrapper.Environment.GetDirectory( "Dir1" );
			string fileName = "File" + writers.Count;
			MockFileInfo file = dir1.AddFile( fileName );
			file.DateFormat = @"dd.MM.yyyy H:mm:ss\.fff";

			MockLogRecordsSource notificationSource = dir1.MockNotificationSource;

			MockLogWriter writer = new MockLogWriter( notificationSource, file, TimeSpan.FromMilliseconds( millis ) );
			writers.Add( writer );
			writer.Start();
		}

		private void PauseAllButton_Click( object sender, RoutedEventArgs e )
		{
			SetSleepDurationForAll( TimeSpan.FromSeconds( 1000 ) );
		}

		private void SetSleepDurationForAll( TimeSpan sleepDuration )
		{
			foreach ( var writer in writers )
			{
				writer.SleepDuration = sleepDuration;
			}
		}

		private void ResumeAllButton_Click( object sender, RoutedEventArgs e )
		{
			SetSleepDurationForAll( TimeSpan.FromMilliseconds( millis ) );
		}
	}
}
