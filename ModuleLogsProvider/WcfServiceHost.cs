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
	internal sealed class WcfServiceHost : IDisposable
	{
		private ServiceHost serviceHost;

		private readonly ILogger logger;
		public WcfServiceHost( ILogger logger )
		{
			if ( logger == null ) throw new ArgumentNullException( "logger" );

			this.logger = logger;
		}

		public void Start( object serviceInstance, string uri )
		{
			if (serviceInstance == null) throw new ArgumentNullException("serviceInstance");
			try
			{
				serviceHost = new ServiceHost( serviceInstance );

				serviceHost.Faulted += HostFaulted;
				serviceHost.UnknownMessageReceived += HostUnknownMessageReceived;
				serviceHost.Opening += HostOpening;
				serviceHost.Opened += HostOpened;
				serviceHost.Closing += HostClosing;
				serviceHost.Closed += HostClosed;

				serviceHost.AddServiceEndpoint( typeof( ILogSourceService ), new BasicHttpBinding(), uri );

				serviceHost.Open();
			}
			catch ( Exception exc )
			{
				logger.WriteLine( MessageType.Error, string.Format( "WcfServiceHost.Start: Exc = {0}", exc ) );
				throw;
			}
		}

		public MostLogSourceService GetInstance()
		{
			MostLogSourceService service = (MostLogSourceService)serviceHost.SingletonInstance;
			return service;
		}

		void HostClosed( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Closed()" );
		}

		void HostClosing( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Closing()" );
		}

		void HostOpened( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Opened()" );
		}

		void HostOpening( object sender, EventArgs e )
		{
			logger.WriteLine( MessageType.Info, "WcfServiceHost.Opening()" );
		}

		void HostUnknownMessageReceived( object sender, UnknownMessageReceivedEventArgs e )
		{
			logger.WriteLine( MessageType.Warning, "WcfServiceHost.UnknownMessageReceived()" );
		}

		void HostFaulted( object sender, EventArgs e )
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
		}
	}
}
