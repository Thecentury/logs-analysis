using System;
using System.Linq;
using System.Net;
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

			const string defaultUrlPrefix = "net.tcp://127.0.0.1:9999/";

			var urlsNode = configXml.SelectSingleNode( "urls" );
			string logsSourceUrl = GetUrl( urlsNode, "logsSource" ) ?? defaultUrlPrefix + "MostLogSourceService/";
			string logsSinkUrl = GetUrl( urlsNode, "logsSink" ) ?? defaultUrlPrefix + "MostLogSinkService/";
			string performanceServiceUrl = GetUrl( urlsNode, "performanceService" ) ?? defaultUrlPrefix + "PerformanceService/";

			List<string> allowedHostsToStartAt = GetAllowedHosts( configXml );

			bool shouldStartServices = ShouldStartServices( allowedHostsToStartAt );
			if ( shouldStartServices )
			{
				logServiceHost = new WcfServiceHost<ILogSourceService>( Logger, new MostLogSourceService( Logger ), logsSourceUrl ).Start();
				logSinkServiceHost = new WcfServiceHost<ILogSinkService>( Logger, new MostLogSinkService( Logger ), logsSinkUrl ).Start();
				performanceServiceHost = new WcfServiceHost<IPerformanceInfoService>( Logger, new CurrentProcessPerformanceService(),
																					 performanceServiceUrl ).Start();
			}
			else
			{
				Logger.WriteLine( MessageType.Warning, 
					String.Format("ModuleLogsProvider: current machine's name \"{0}\" is not in list of allowed hosts.", Environment.MachineName) );
			}
		}

		private List<string> GetAllowedHosts( XmlNode configXml )
		{
			List<string> allowedHosts = new List<string>();

			XmlNode allowedHostsNode = configXml.SelectSingleNode( "AllowedHosts" );
			if ( allowedHostsNode == null )
				return allowedHosts;

			var hostNodes = allowedHostsNode.SelectNodes( "Host" );
			if ( hostNodes == null )
				return allowedHosts;

			foreach ( XmlNode hostNode in hostNodes )
			{
				string host = hostNode.InnerText;
				allowedHosts.Add( host );
			}

			return allowedHosts;
		}

		private static string NetworkUrlToIp( string url )
		{
			var uri = new Uri( url );
			var ipAddress = Dns.GetHostAddresses( uri.Host ).Where( a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ).First();
			return String.Format( "{0}{1}:{2}{3}", uri.GetLeftPart( UriPartial.Scheme ), ipAddress, uri.Port, uri.AbsolutePath );
		}

		private bool ShouldStartServices( List<string> allowedHosts )
		{
			string machineName = Environment.MachineName;
			bool shouldStart = allowedHosts.Contains( machineName );
			return shouldStart;
		}

		private static string GetUrl( XmlNode urlsNode, string serviceName )
		{
			if ( urlsNode == null )
				return null;

			var urlNode = urlsNode.SelectSingleNode( serviceName );
			if ( urlNode == null )
				return null;

			string url = urlNode.InnerText;
			string urlWithIps = NetworkUrlToIp( url );
			return urlWithIps;
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
