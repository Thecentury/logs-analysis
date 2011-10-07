using System;
using Morqua.MOST.KernelInterfaces;
using System.Xml;
using Morqua.Logging;
using System.Collections.Generic;
using Awad.Eticket.ModuleLogsProvider.Types;

namespace Awad.Eticket.ModuleLogsProvider
{
	/// <summary>
	/// Модуль обеспечивает доступ к wcf-сервису записей логов.
	/// </summary>
	[KernelModule( "ModuleLogsProvider" )]
	public class ModuleLogsProvider : KernelModule
	{
		private WcfServiceHost wcfServiceHost;
		private MostLogSourceService service;

		public override string SystemName
		{
			get { return "ModuleLogsProvider"; }
		}

		public override void Init( XmlNode configXml )
		{
			base.Init( configXml );

			service = new MostLogSourceService( Logger );
			const string uri = "http://127.0.0.1:9999/MostLogSourceService/";
			try
			{
				wcfServiceHost = new WcfServiceHost( Logger );
				wcfServiceHost.Start( service, uri );
			}
			catch ( Exception exc )
			{
				Logger.WriteLine( MessageType.Warning, "ModuleLogsProvider.Init(): Exception occured {0}", exc.ToString() );
				throw;
			}
		}

		public override void Stop()
		{
			if ( wcfServiceHost != null )
			{
				wcfServiceHost.Dispose();
			}
			service.Unsubscribe();
		}

		[KernelCommand( "StartListening" )]
		public IKernelCommandResults CommandStartListening( IKernelCommandParams args )
		{
			service.StartListening();
			return new KernelCommandResults();
		}

		[KernelCommand( "StopListening" )]
		public IKernelCommandResults CommandStopListening( IKernelCommandParams args )
		{
			service.StopListening();
			return new KernelCommandResults();
		}

		[KernelCommand( "WriteError" )]
		public IKernelCommandResults CommandWriteErrorMessage( IKernelCommandParams args )
		{
			return LogTimeAndReturn( MessageType.Error );
		}

		[KernelCommand( "WriteWarning" )]
		public IKernelCommandResults CommandWriteWarningMessage( IKernelCommandParams args )
		{
			return LogTimeAndReturn( MessageType.Warning );
		}

		[KernelCommand( "WriteInfo" )]
		public IKernelCommandResults CommandWriteInfoMessage( IKernelCommandParams args )
		{
			return LogTimeAndReturn( MessageType.Info );
		}

		private void LogTime( MessageType messageType )
		{
			Logger.WriteLine( messageType, "ModuleLogsProvider: {0}", DateTime.Now.ToString( "hh:MM:ss" ) );
		}

		private KernelCommandResults LogTimeAndReturn( MessageType messageType )
		{
			LogTime( messageType );
			return new KernelCommandResults();
		}
	}
}
