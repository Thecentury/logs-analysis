using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Regions;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;

namespace LogAnalyzer.GUI.Views
{
	public abstract class Bootstrapper<TConfig> where TConfig : LogAnalyzerConfiguration
	{
		private string[] commandLineArgs;
		public string[] CommandLineArgs
		{
			get { return commandLineArgs; }
		}

		private Logger logger;
		public Logger Logger
		{
			get { return logger; }
		}

		public void Start( string[] commandLineArgs )
		{
			if ( commandLineArgs == null ) throw new ArgumentNullException( "commandLineArgs" );

			this.commandLineArgs = commandLineArgs;
		}

		public void InitConfig( TConfig config )
		{
			this.logger = config.Logger;

			var operationsQueue = new WorkerThreadOperationsQueue( logger );

			RegionManager regionManager = new RegionManager();

			config
				.AcceptAllLogTypes()
				.RegisterInstance<IOperationsQueue>( operationsQueue )
				.RegisterInstance<IWindowService>( new RealWindowService() )
				.RegisterInstance<OperationScheduler>( OperationScheduler.TaskScheduler )
				.RegisterInstance<IErrorReportingService>( new ErrorReportingService() )
				.RegisterInstance<RegionManager>( regionManager );
		}
	}
}
