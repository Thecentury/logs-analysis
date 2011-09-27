using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Text;

namespace LogAnalyzer.Config
{
	public static class LogAnalyzerConfigurationExtensions
	{
		public static IScheduler GetScheduler(this LogAnalyzerConfiguration config)
		{
			return config.Resolve<IScheduler>();
		}
	}
}
