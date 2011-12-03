using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Regions;
using LogAnalyzer.GUI.ViewModels.Colorizing;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Misc;
using LogAnalyzer.Operations;

namespace LogAnalyzer.GUI
{
	public abstract class Bootstrapper
	{
		private Logger logger;
		public Logger Logger
		{
			get { return logger; }
		}

		private string[] commandLineArgs;
		public string[] CommandLineArgs
		{
			get { return commandLineArgs; }
		}

		private CommandLineArgumentsParser argsParser;
		protected CommandLineArgumentsParser ArgsParser
		{
			get { return argsParser; }
		}

		protected readonly DirectoryManager DirectoryManager = new DirectoryManager();

		public void Start( string[] commandLineArgs )
		{
			if ( commandLineArgs == null ) throw new ArgumentNullException( "commandLineArgs" );
			this.commandLineArgs = commandLineArgs;

			argsParser = new CommandLineArgumentsParser( commandLineArgs );

			Thread.CurrentThread.Name = "UIThread";

			Task.Factory
				.StartNew( Init )
				.ContinueWith( OnInitException, TaskContinuationOptions.OnlyOnFaulted );
		}

		protected abstract void Init();

		private void OnInitException( Task parentTask )
		{
			var exception = parentTask.Exception;

			LogAnalyzer.Extensions.Condition.BreakIfAttached();
			Logger.WriteError( "Crash. Exception: {0}", exception );

			MessageBox.Show( "Unhandled exception: " + exception, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			Environment.Exit( -1 );
		}

		protected void InitConfig( LogAnalyzerConfiguration config )
		{
			this.logger = config.Logger;

			var operationsQueue = new WorkerThreadOperationsQueue( logger );

			DirectoryManager.RegisterCommonFactories();

			string templatesDir = Path.Combine(
				Path.GetDirectoryName( Assembly.GetExecutingAssembly().Location ),
				"Templates" );

			ColorizationManager colorizationManager = null;
			DispatcherHelper.GetDispatcher().Invoke( () =>
			{
				ColorizationLoader colorizationLoader = new ColorizationLoader( templatesDir );
				var templates = colorizationLoader.Load();
				colorizationManager = new ColorizationManager( templates );
			}, DispatcherPriority.Send );

			config
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance( OperationScheduler.TaskScheduler )
				.RegisterInstance<IErrorReportingService>( new ErrorReportingService() )
				.RegisterInstance( new RegionManager() )
				.RegisterInstance<IDirectoryFactory>( DirectoryManager )
				.RegisterInstance<ITimeService>( new NeverOldTimeService() )
				.RegisterInstance<IFileSystem>( new RealFileSystem() )
				.RegisterInstance( colorizationManager );
		}
	}
}
