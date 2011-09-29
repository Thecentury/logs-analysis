using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace LogAnalysisServer.Dev
{
	class Program
	{
		static void Main( string[] args )
		{
			string uri = "http://127.0.0.1:9999/MostLogSourceService/";

			ServiceHost host = new ServiceHost( typeof( LogServer ), new Uri( uri ) );
			host.Open();

			while ( true )
			{
				Thread.Sleep( 100 );
			}
		}
	}
}
