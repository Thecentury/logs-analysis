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
		private WcfServiceHost<IPerformanceInfoService> performanceServiceHost;

		public override string SystemName
		{
			get { return "ModuleLogsProvider"; }
		}

		public override void Init( XmlNode configXml )
		{
			base.Init( configXml );

			var urlsNode = configXml.SelectSingleNode( "urls" );
			string logsSourceUrl = GetUrl( urlsNode, "logsSource" ) ?? "net.tcp://127.0.0.1:9999/MostLogSourceService/";
			string logsSinkUrl = GetUrl( urlsNode, "logsSink" ) ?? "net.tcp://127.0.0.1:9999/MostLogSinkService/";
			string performanceServiceUrl = GetUrl( urlsNode, "performanceService" ) ?? "net.tcp://127.0.0.1:9999/PerformanceService/";

			// todo brinchuk url -> ip!

			logServiceHost = new WcfServiceHost<ILogSourceService>( Logger, new MostLogSourceService( Logger ), logsSourceUrl );
			logSinkServiceHost = new WcfServiceHost<ILogSinkService>( Logger, new MostLogSinkService( Logger ), logsSinkUrl );
			performanceServiceHost = new WcfServiceHost<IPerformanceInfoService>( Logger, new CurrentProcessPerformanceService(), performanceServiceUrl );
		}

		private static string GetUrl( XmlNode urlsNode, string serviceName )
		{
			if ( urlsNode == null )
				return null;

			var urlNode = urlsNode.SelectSingleNode( serviceName );
			if ( urlNode == null )
				return null;

			return urlNode.InnerText;
		}

		public override void Stop()
		{
			logServiceHost.SafeDispose();
			logSinkServiceHost.SafeDispose();
			performanceServiceHost.SafeDispose();
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
