using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Windows;
using LogAnalyzer.GUI.ViewModels.Collections;
using LogAnalyzer.Tests.Mocks;

namespace TestApp
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private TestBootstrapper _bootstrapper;
		private readonly List<MockLogWriter> _writers = new List<MockLogWriter>();
		private const int Millis = 100;

		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindowLoaded;
		}

		private void MainWindowLoaded( object sender, RoutedEventArgs e )
		{
			App app = (App)Application.Current;
			_bootstrapper = app.Bootstrapper;

			dataGrid.ItemsSource = new DispatcherObservableCollection( _bootstrapper.ApplicationViewModel.Core.MergedEntries,
																	  new DispatcherScheduler( Dispatcher ) ) { Name = "1" };
		}

		private void StartButtonClick( object sender, RoutedEventArgs e )
		{
			MockDirectoryInfo dir1 = (MockDirectoryInfo)_bootstrapper.Environment.GetDirectory( "Dir1" );
			string fileName = "File" + _writers.Count;
			MockFileInfo file = dir1.AddFile( fileName );
			file.DateFormat = @"dd.MM.yyyy H:mm:ss\.fff";

			MockLogRecordsSource notificationSource = dir1.MockNotificationSource;

			MockLogWriter writer = new MockLogWriter( notificationSource, file, TimeSpan.FromMilliseconds( Millis ) );
			_writers.Add( writer );
			writer.Start();
		}

		private void PauseAllButtonClick( object sender, RoutedEventArgs e )
		{
			SetSleepDurationForAll( TimeSpan.FromSeconds( 1000 ) );
		}

		private void SetSleepDurationForAll( TimeSpan sleepDuration )
		{
			foreach ( var writer in _writers )
			{
				writer.SleepDuration = sleepDuration;
			}
		}

		private void ResumeAllButtonClick( object sender, RoutedEventArgs e )
		{
			SetSleepDurationForAll( TimeSpan.FromMilliseconds( Millis ) );
		}
	}
}
