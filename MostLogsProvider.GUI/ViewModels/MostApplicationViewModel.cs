using System;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.GUI.ViewModels
{
	public sealed class MostApplicationViewModel : ApplicationViewModel
	{
		private readonly MostLogAnalyzerConfiguration config;

		public MostApplicationViewModel( MostLogAnalyzerConfiguration config, IEnvironment environment )
			: base( config, environment )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );

			this.config = config;

			var errorReportingService = config.ResolveNotNull<IErrorReportingService>();
			errorReportingViewModel = new ErrorReportingViewModel( errorReportingService, config.Container );
			performanceViewModel = new ServerPerformanceViewModel( config, this );
		}

		protected override void OnCoreLoaded()
		{
			base.OnCoreLoaded();

			config.LogsUpdateTimer.Invoke();
			config.PerformanceDataUpdateTimer.Invoke();
		}

		private readonly ErrorReportingViewModel errorReportingViewModel;
		public ErrorReportingViewModel ErrorReporting
		{
			get { return errorReportingViewModel; }
		}

		private readonly ServerPerformanceViewModel performanceViewModel;
		public ServerPerformanceViewModel Performance
		{
			get { return performanceViewModel; }
		}
	}
}
