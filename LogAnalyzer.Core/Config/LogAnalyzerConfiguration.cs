using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Reactive.Concurrency;
using System.Windows;
using System.Windows.Threading;
using System.Xaml;
using System.ComponentModel;
using LogAnalyzer.Extensions;
using LogAnalyzer.Filters;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;

namespace LogAnalyzer.Config
{
	public class LogAnalyzerConfiguration
	{
		public LogAnalyzerConfiguration()
		{
			RegisterCommonDependencies();
		}

		private string _defaultEncodingName = "utf-8";
		public string DefaultEncodingName
		{
			get { return _defaultEncodingName; }
			set { _defaultEncodingName = value; }
		}

		private readonly IDependencyInjectionContainer _container = DependencyInjectionContainer.Instance;
		public IDependencyInjectionContainer Container
		{
			get { return _container; }
		}

		private void RegisterCommonDependencies()
		{
			Container.RegisterInstance<OperationScheduler>( OperationScheduler.TaskScheduler );

			Dispatcher currentDispatcher = DispatcherHelper.GetDispatcher();

			IScheduler scheduler = new DispatcherScheduler( currentDispatcher );
			Container.RegisterInstance<IScheduler>( scheduler );
		}

		private readonly WpfViewManager _viewManager = new WpfViewManager();
		public IViewManager<FrameworkElement> ViewManager
		{
			get { return _viewManager; }
		}

		private readonly ObservableCollection<LogDirectoryConfigurationInfo> _directories = new ObservableCollection<LogDirectoryConfigurationInfo>();
		
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public ObservableCollection<LogDirectoryConfigurationInfo> Directories
		{
			get { return _directories; }
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public IEnumerable<LogDirectoryConfigurationInfo> EnabledDirectories
		{
			get { return _directories.Where( dir => dir.Enabled ); }
		}

		private readonly Logger _logger = Logger.Instance;
		public Logger Logger
		{
			get { return _logger; }
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<MessageType> LoggerAcceptedTypes
		{
			get { return _logger.AcceptedTypes; }
		}

		[DesignerSerializationVisibility( DesignerSerializationVisibility.Content )]
		public List<LogWriter> LoggerWriters
		{
			get { return _logger.Writers; }
		}

		private readonly ExpressionFilter<LogEntry> _globalEntryFilter = new ExpressionFilter<LogEntry>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public ExpressionFilter<LogEntry> GlobalLogEntryFilter
		{
			get { return _globalEntryFilter; }
		}

		public ExpressionBuilder GlobalLogEntryFilterBuilder
		{
			get { return _globalEntryFilter.ExpressionBuilder; }
			set { _globalEntryFilter.ExpressionBuilder = value; }
		}

		#region Files filter

		private readonly ExpressionFilter<IFileInfo> _globalFilesFilter = new ExpressionFilter<IFileInfo>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public ExpressionFilter<IFileInfo> GlobalFilesFilter
		{
			get { return _globalFilesFilter; }
		}

		public ExpressionBuilder GlobalFilesFilterBuilder
		{
			get { return _globalFilesFilter.ExpressionBuilder; }
			set { _globalFilesFilter.ExpressionBuilder = value; }
		}

		private readonly ExpressionFilter<string> _globalFileNamesFilter = new ExpressionFilter<string>();
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public ExpressionFilter<string> GlobalFileNamesFilter
		{
			get { return _globalFileNamesFilter; }
		}

		public ExpressionBuilder GlobalFileNamesFilterBuilder
		{
			get { return _globalFileNamesFilter.ExpressionBuilder; }
			set { _globalFileNamesFilter.ExpressionBuilder = value; }
		}

		#endregion

		#region Methods

		public static LogAnalyzerConfiguration CreateNew()
		{
			return new LogAnalyzerConfiguration();
		}

		public void SaveToStream( Stream stream )
		{
			XamlServices.Save( stream, this );
		}

		public static LogAnalyzerConfiguration LoadFromStream( Stream stream )
		{
			LogAnalyzerConfiguration config = (LogAnalyzerConfiguration)XamlServices.Load( stream );
			return config;
		}

		public static LogAnalyzerConfiguration LoadFromFile( string fileName )
		{
			LogAnalyzerConfiguration result;

			using ( FileStream fs = new FileStream( fileName, FileMode.Open, FileAccess.Read, FileShare.Read ) )
			{
				result = LoadFromStream( fs );
			}

			return result;
		}

		public static LogAnalyzerConfiguration CreateSampleConfiguration()
		{
			LogAnalyzerConfiguration config = new LogAnalyzerConfiguration();

			config._directories.Add( new LogDirectoryConfigurationInfo
			{
				DisplayName = "SampleDir",
				FileNameFilter = "*.log",
				Path = @"C:\Logs"
			} );


			//config.DateAcceptor = new AllDatesAcceptor();

			//DateTime start = new DateTime( 2011, 05, 13, 0, 2, 54 );
			//DateTime end = new DateTime( 2011, 05, 14, 17, 16, 55 );
			//config.DateAcceptor = new FixedDatesAcceptor { Start = start, End = end };

			//config.DateAcceptor = new LastNMinutesAcceptor { MinutesCount = 10 };

			config.Logger.AcceptedTypes.Add( MessageType.Debug );
			config.Logger.Writers.Add( new FileLogWriter( @"C:\Logs\1.log" ) );

			return config;
		}

		#endregion
	}
}
