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
			const string logServiceUri = "http://127.0.0.1:9999/MostLogSourceService/";
			const string performanceServiceUri = "http://127.0.0.1:9999/MostPerformanceService/";

			ServiceHost logServiceHost = new ServiceHost( typeof( LogServer ), new Uri( logServiceUri ) );
			logServiceHost.Open();

			ServiceHost performanceServiceHost = new ServiceHost( typeof( CurrentProcessPerformanceService ), new Uri( performanceServiceUri ) );
			performanceServiceHost.Open();

			while ( true )
			{
				Thread.Sleep( 100 );
			}
		}
	}
}
