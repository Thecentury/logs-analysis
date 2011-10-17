using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;

namespace LogAnalyzer.Extensions
{
	public static class LogAnalyzerConfigurationExtensions
	{
		public static T Resolve<T>( this LogAnalyzerConfiguration config )
		{
			return config.Container.Resolve<T>();
		}

		public static T ResolveNotNull<T>( this LogAnalyzerConfiguration config )
		{
			return config.Container.ResolveNotNull<T>();
		}
	}
}
