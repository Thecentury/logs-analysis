using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using ModuleLogsProvider.Interfaces;
using Morqua.Logging;
using Awad.Eticket.ModuleLogsProvider.Types;

namespace Awad.Eticket.ModuleLogsProvider
{
	internal sealed class WcfServiceHost<T> : IDisposable where T : class
	{
		private readonly ServiceHost serviceHost;
		private readonly ILogger logger;
		private readonly T serviceInstance;

		public WcfServiceHost( ILogger logger, T serviceInstance, string uri )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );
			if ( serviceInstance == null ) throw new ArgumentNullException( "serviceInstance" );

			this.logger = logger;
			this.serviceInstance = serviceInstance;

			try
			{
				serviceHost = new ServiceHost( serviceInstance );

				serviceHost.Faulted += OnHostFaulted;
				serviceHost.UnknownMessageReceived += OnHostUnknownMessageReceived;
				serviceHost.Opening += OnHostOpening;
				serviceHost.Opened += OnHostOpened;
				serviceHost.Closing += OnHostClosing;
				serviceHost.Closed += OnHostClosed;

				var binding = new NetTcpBinding
				{
					MaxReceivedMessageSize = Int32.MaxValue,
					ReaderQuotas =
					{
						MaxStringContentLength = Int32.MaxValue,
						MaxArrayLength = Int32.MaxValue,
						MaxBytesPerRead = Int32.MaxValue,
						MaxNameTableCharCount = Int32.MaxValue
					}
				};

				serviceHost.AddServiceEndpoint( typeof( T ), binding, uri );
			}
			catch ( Exception exc )
			{
				logger.WriteLine( MessageType.Error, String.Format( "WcfServiceHost.Start( uri = {0} ): Exc = {1}", uri, exc ) );
				throw;
			}
		}

		public WcfServiceHost<T>  Start()
		{
			serviceHost.Open();
			return this;
		}

		public T ServiceInstance
		{
			get { return serviceInstance; }
		}

		private void OnHostClosed( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Closed()" );
		}

		private void OnHostClosing( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Closing()" );
		}

		private void OnHostOpened( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Opened()" );
		}

		private void OnHostOpening( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Opening()" );
		}

		private void OnHostUnknownMessageReceived( object sender, UnknownMessageReceivedEventArgs e )
		{
			logger.WriteLine( MessageType.Warning, "WcfServiceHost.UnknownMessageReceived()" );
		}

		private void OnHostFaulted( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Warning, "WcfServiceHost.Faulted()" );
		}

		public void Dispose()
		{
			try
			{
				if ( serviceHost != null )
				{
					if ( serviceHost.State != CommunicationState.Faulted )
					{
						serviceHost.Close();
					}
				}
			}
			catch ( Exception exc )
			{
				logger.WriteLine( MessageType.Warning, "WcfServiceHost.Dispose(): Exception {0}", exc );
			}

			IDisposable disposableService = ServiceInstance as IDisposable;
			if ( disposableService != null )
			{
				disposableService.Dispose();
			}
		}
	}
}
