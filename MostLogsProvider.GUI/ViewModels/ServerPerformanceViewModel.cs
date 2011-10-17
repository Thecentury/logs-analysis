using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using AdTech.Common.WPF;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Regions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Most;
using Windows7.DesktopIntegration;

namespace ModuleLogsProvider.GUI.ViewModels
{
	public sealed class ServerPerformanceViewModel : BindingObject
	{
		private readonly MostLogAnalyzerConfiguration config;
		private readonly MostServiceFactory<IPerformanceInfoService> performanceInfoServiceFactory;
		private readonly OperationScheduler operationScheduler;
		private readonly IWindowService windowService;
		private readonly ErrorReportingServiceBase errorReportingService;

		public ServerPerformanceViewModel( MostLogAnalyzerConfiguration config )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );

			this.config = config;
			config.PerformanceDataUpdateTimer.Tick += OnPerformanceDataUpdateTimerTick;
			var bindingFactory = new NetTcpBindingFactory();

			performanceInfoServiceFactory = new MostServiceFactory<IPerformanceInfoService>( bindingFactory,
				config.SelectedUrls.PerformanceDataServiceUrl );

			operationScheduler = config.ResolveNotNull<OperationScheduler>();
			windowService = config.ResolveNotNull<IWindowService>();
			errorReportingService = config.ResolveNotNull<ErrorReportingServiceBase>();

			var view = config.ViewManager.ResolveView( this );
			var regionManager = config.ResolveNotNull<RegionManager>();
			regionManager.Regions["StatusBar"].Add( view );
		}

		private void OnPerformanceDataUpdateTimerTick( object sender, EventArgs e )
		{
			operationScheduler.StartNewOperation( UpdatePerformanceData );
		}

		private void UpdatePerformanceData()
		{
			using ( var client = performanceInfoServiceFactory.Create() )
			{
				try
				{
					double memory = client.Service.GetMemoryConsumption();
					double mbs = Math.Round( memory / 1024.0 / 1024.0, 1 );
					WorkingSetMBs = mbs;

					double actualCpuLoad = client.Service.GetCPULoad();
					double cpuLoadRounded = Math.Round( actualCpuLoad );
					CpuLoad = cpuLoadRounded;

					int cpuLoadInt = (int)cpuLoadRounded;
					if ( cpuLoadRounded < 1 )
						cpuLoadInt = 0;

					windowService.SetProgressValue( cpuLoadInt );

					var progressState = GetTaskbarProgressState( cpuLoadInt );
					windowService.SetProgressState( progressState );
				}
				catch ( Exception exc )
				{
					errorReportingService.ReportError( exc, exc.Message );

					bool isExpected = client.IsExpectedException( exc );

					if ( !isExpected )
						throw;
				}
			}
		}

		private static Windows7Taskbar.ThumbnailProgressState GetTaskbarProgressState( int cpuLoadInt )
		{
			if ( cpuLoadInt <= 0 )
				return Windows7Taskbar.ThumbnailProgressState.NoProgress;
			else
				return Windows7Taskbar.ThumbnailProgressState.Normal;
		}

		private double workingSetMBs;
		public double WorkingSetMBs
		{
			get { return workingSetMBs; }
			private set
			{
				workingSetMBs = value;
				RaisePropertyChanged( "WorkingSetMBs" );
			}
		}

		private double cpuLoad;
		public double CpuLoad
		{
			get { return cpuLoad; }
			private set
			{
				cpuLoad = value;
				RaisePropertyChanged( "CpuLoad" );
			}
		}

		#region RefreshLogData command

		private DelegateCommand refreshLogDataCommand;

		/// <summary>
		/// Обновить записи логов.
		/// </summary>
		public ICommand RefreshLogDataCommand
		{
			get
			{
				if ( refreshLogDataCommand == null )
				{
					refreshLogDataCommand = new DelegateCommand( RefreshLogData );
				}

				return refreshLogDataCommand;
			}
		}

		private void RefreshLogData()
		{
			config.LogsUpdateTimer.Invoke();
			config.PerformanceDataUpdateTimer.Invoke();
		}

		#endregion
	}
}
