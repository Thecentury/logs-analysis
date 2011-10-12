using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Logging
{
	public static class MostLogAnalyzerConfigurationExtensions
	{
		public static T WithLogsUpdateTimer<T>( this T config, ITimer logsUpdateTimer ) where T : MostLogAnalyzerConfiguration
		{
			if ( logsUpdateTimer == null ) throw new ArgumentNullException( "logsUpdateTimer" );

			config.LogsUpdateTimer = logsUpdateTimer;
			return config;
		}

		public static T WithPerformanceDataUpdateTimer<T>( this T config, ITimer performanceDataUpdateTimer ) where T : MostLogAnalyzerConfiguration
		{
			if ( performanceDataUpdateTimer == null ) throw new ArgumentNullException( "performanceDataUpdateTimer" );

			config.PerformanceDataUpdateTimer = performanceDataUpdateTimer;
			return config;
		}

		public static T SetSelectedUrls<T>( this T config, MostServerUrls urls ) where T : MostLogAnalyzerConfiguration
		{
			if (urls == null) throw new ArgumentNullException("urls");

			config.SelectedUrls = urls;
			return config;
		}
	}
}
