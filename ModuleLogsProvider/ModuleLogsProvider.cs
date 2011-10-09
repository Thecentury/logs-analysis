using System;
using ModuleLogsProvider.Interfaces;
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
		private WcfServiceHost<ILogSourceService> logServiceHost;
		private WcfServiceHost<ILogSinkService> logSinkServiceHost;

		public override string SystemName
		{
			get { return "ModuleLogsProvider"; }
		}

		public override void Init( XmlNode configXml )
		{
			base.Init( configXml );

			logServiceHost = new WcfServiceHost<ILogSourceService>( Logger, new MostLogSourceService( Logger ), "http://127.0.0.1:9999/MostLogSourceService/" );
			logSinkServiceHost = new WcfServiceHost<ILogSinkService>( Logger, new MostLogSinkService( Logger ), "http://127.0.0.1:9999/MostLogSinkService/" );
		}

		public override void Stop()
		{
			logServiceHost.SafeDispose();
			logSinkServiceHost.SafeDispose();
		}

		[KernelCommand( "StartListening" )]
		public IKernelCommandResults CommandStartListening( IKernelCommandParams args )
		{
			logServiceHost.ServiceInstance.StartListening();
			return new KernelCommandResults();
		}

		[KernelCommand( "StopListening" )]
		public IKernelCommandResults CommandStopListening( IKernelCommandParams args )
		{
			logServiceHost.ServiceInstance.StopListening();
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
