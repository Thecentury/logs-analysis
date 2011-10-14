using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AdTech.Common.WPF;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using LogAnalyzer.Logging;
using LogAnalyzer.Operations;
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Most;
using Windows7.DesktopIntegration;
using LogAnalyzer.Extensions;

namespace ModuleLogsProvider.GUI.ViewModels
{
	public sealed class MostApplicationViewModel : ApplicationViewModel
	{
		private readonly MostLogAnalyzerConfiguration config;
		private readonly MostServiceFactory<IPerformanceInfoService> performanceInfoServiceFactory;
		private readonly OperationScheduler operationScheduler;

		public MostApplicationViewModel( MostLogAnalyzerConfiguration config, IEnvironment environment )
			: base( config, environment )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );

			this.config = config;

			config.PerformanceDataUpdateTimer.Tick += OnPerformanceDataUpdateTimerTick;
			var bindingFactory = new NetTcpBindingFactory();

			performanceInfoServiceFactory = new MostServiceFactory<IPerformanceInfoService>( bindingFactory,
				config.SelectedUrls.PerformanceDataServiceUrl );

			operationScheduler = config.ResolveNotNull<OperationScheduler>();
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

					double cpuLoad = client.Service.GetCPULoad();
					double cpuLoadRounded = Math.Round( cpuLoad );
					CpuLoad = cpuLoadRounded;

					int cpuLoadInt = (int)cpuLoadRounded;
					if ( cpuLoadRounded < 1 )
						cpuLoadInt = 0;

					WindowService.SetProgressValue( cpuLoadInt );

					var progressState = GetProgressState( cpuLoadInt );
					WindowService.SetProgressState( progressState );
				}
				catch ( CommunicationException exc )
				{
					// todo brinchuk тут использовать IErrorReportingService
					Logger.Instance.WriteLine( MessageType.Error, exc.ToString() );
				}
			}
		}

		protected override void OnCoreLoaded()
		{
			base.OnCoreLoaded();

			config.LogsUpdateTimer.Invoke();
			config.PerformanceDataUpdateTimer.Invoke();
		}

		private static Windows7Taskbar.ThumbnailProgressState GetProgressState( int cpuLoadInt )
		{
			if ( cpuLoadInt <= 0 )
				return Windows7Taskbar.ThumbnailProgressState.NoProgress;
			else
				return Windows7Taskbar.ThumbnailProgressState.Normal;
		}

		#region Properties

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

		#endregion

		#region Commands

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

		#endregion
	}
}
