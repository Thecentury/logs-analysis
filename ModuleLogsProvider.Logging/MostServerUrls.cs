using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Logging
{
	public sealed class MostServerUrls
	{
		public string Tag { get; set; }

		public string DisplayName { get; set; }

		public string LogsSourceServiceUrl { get; set; }

		public string LogsSinkServiceUrl { get; set; }

		public string PerformanceDataServiceUrl { get; set; }

		public static MostServerUrls Local
		{
			get { return new MostServerUrls
			             	{
								Tag = "local",
			             		DisplayName = "Local",
								LogsSinkServiceUrl = "net.tcp://127.0.0.1:9999/MostLogSinkService/",
								LogsSourceServiceUrl = "net.tcp://127.0.0.1:9999/MostLogSourceService/",
								PerformanceDataServiceUrl = "net.tcp://127.0.0.1:9999/PerformanceService/"
			             	}; }
		}
	}
}
