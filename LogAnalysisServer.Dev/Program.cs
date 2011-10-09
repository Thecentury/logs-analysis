using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using ModuleLogsProvider.Interfaces;

namespace LogAnalysisServer.Dev
{
	class Program
	{
		static void Main( string[] args )
		{
			const string logServiceUri = "net.tcp://127.0.0.1:9999/MostLogSourceService/";
			const string performanceServiceUri = "net.tcp://127.0.0.1:9999/PerformanceService/";

			ServiceHost logServiceHost = new ServiceHost( typeof( LogServer ) );
			logServiceHost.AddServiceEndpoint( typeof( ILogSourceService ), new NetTcpBinding(), logServiceUri );
			logServiceHost.Open();

			ServiceHost performanceServiceHost = new ServiceHost( typeof( CurrentProcessPerformanceService ) );
			performanceServiceHost.AddServiceEndpoint( typeof( IPerformanceInfoService ), new NetTcpBinding(), performanceServiceUri );
			performanceServiceHost.Open();

			while ( true )
			{
				Thread.Sleep( 100 );
			}
		}
	}
}
