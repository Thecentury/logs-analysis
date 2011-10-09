using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Logging
{
	public sealed class MostServerUrls
	{
		public string DisplayName { get; set; }

		public string LogsSourceServiceUrl { get; set; }

		public string LogsSinkServiceUrl { get; set; }

		public string PerformanceDataServiceUrl { get; set; }
	}
}
