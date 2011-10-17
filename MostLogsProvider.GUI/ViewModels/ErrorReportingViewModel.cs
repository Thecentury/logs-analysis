using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;
using ModuleLogsProvider.Logging;

namespace ModuleLogsProvider.GUI.ViewModels
{
	public sealed class ErrorReportingViewModel : BindingObject
	{
		private readonly ErrorReportingServiceBase errorReportingService;
		private readonly ITimer timer;

		public ErrorReportingViewModel( ErrorReportingServiceBase errorReportingService, IDependencyInjectionContainer container )
		{
			if ( errorReportingService == null ) throw new ArgumentNullException( "errorReportingService" );
			if ( container == null ) throw new ArgumentNullException( "container" );

			this.errorReportingService = errorReportingService;
			errorReportingService.ErrorOccured += OnErrorReportingService_ErrorOccured;

			timer = container.ResolveNotNull<ITimer>();
			timer.Interval = TimeSpan.FromSeconds( 2.5 );
			timer.Tick += OnAutoHideTimer_Tick;
			timer.Start();
		}

		private void OnErrorReportingService_ErrorOccured( object sender, ErrorOccuredEventArgs e )
		{
			ErrorMessage = e.Message;
			timer.Stop();
			timer.Start();
			Visibility = Visibility.Visible;
			LastErrorTime = DateTime.Now;
		}

		public ErrorReportingServiceBase ErrorReportingService
		{
			get { return errorReportingService; }
		}

		private DateTime lastErrorTime;
		public DateTime LastErrorTime
		{
			get { return lastErrorTime; }
			private set
			{
				lastErrorTime = value;
				RaisePropertyChanged( "LastErrorTime" );
			}
		}

		private string errorMessage;
		public string ErrorMessage
		{
			get { return errorMessage; }
			private set
			{
				errorMessage = value;
				RaisePropertyChanged( "ErrorMessage" );
			}
		}

		private Visibility visibility = Visibility.Collapsed;
		public Visibility Visibility
		{
			get { return visibility; }
			private set
			{
				if ( visibility != value )
				{
					visibility = value;
					RaisePropertyChanged( "Visibility" );
				}
			}
		}

		private void OnAutoHideTimer_Tick( object sender, EventArgs e )
		{
			Visibility = Visibility.Collapsed;
			timer.Stop();
		}
	}
}
