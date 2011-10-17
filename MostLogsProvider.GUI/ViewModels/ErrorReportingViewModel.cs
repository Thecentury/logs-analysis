using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Kernel;

namespace ModuleLogsProvider.GUI.ViewModels
{
	public sealed class ErrorReportingViewModel : BindingObject
	{
		private readonly ErrorReportingServiceBase errorReportingService;
		private readonly DispatcherTimer autoHideTimer;

		public ErrorReportingViewModel( ErrorReportingServiceBase errorReportingService )
		{
			if ( errorReportingService == null ) throw new ArgumentNullException( "errorReportingService" );

			this.errorReportingService = errorReportingService;
			errorReportingService.ErrorOccured += OnErrorReportingService_ErrorOccured;

			autoHideTimer = new DispatcherTimer( TimeSpan.FromSeconds( 2.5 ), DispatcherPriority.Background, OnAutoHideTimer_Tick, DispatcherHelper.GetDispatcher() );
			autoHideTimer.Start();
		}

		private void OnErrorReportingService_ErrorOccured( object sender, ErrorOccuredEventArgs e )
		{
			ErrorMessage = e.Message;
			autoHideTimer.Stop();
			autoHideTimer.Start();
			Visibility = Visibility.Visible;
		}

		public ErrorReportingServiceBase ErrorReportingService
		{
			get { return errorReportingService; }
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
			autoHideTimer.Stop();
		}
	}
}
