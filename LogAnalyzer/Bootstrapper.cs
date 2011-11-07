using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LogAnalyzer.Config;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Regions;
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

			Extensions.Condition.BreakIfAttached();
			Logger.WriteError( "Crash. Exception: {0}", exception );

			MessageBox.Show( "Unhandled exception: " + exception, "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error );

			Environment.Exit( -1 );
		}

		protected void InitConfig( LogAnalyzerConfiguration config )
		{
			this.logger = config.Logger;

			var operationsQueue = new WorkerThreadOperationsQueue( logger );

			DirectoryManager.RegisterCommonFactories();

			config
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<OperationScheduler>( OperationScheduler.TaskScheduler )
				.RegisterInstance<IErrorReportingService>( new ErrorReportingService() )
				.RegisterInstance<RegionManager>( new RegionManager() )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<IDirectoryFactory>( DirectoryManager )
				.RegisterInstance<ITimeService>( new NeverOldTimeService() );
		}
	}
}
