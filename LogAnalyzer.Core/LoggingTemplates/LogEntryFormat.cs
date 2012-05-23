using System;
using JetBrains.Annotations;

namespace LogAnalyzer.LoggingTemplates
{
	public sealed class LogEntryFormat
	{
		public LogEntryFormat( [NotNull] LoggerUsage usage, [NotNull] string assemblyName )
		{
			if ( usage == null )
			{
				throw new ArgumentNullException( "usage" );
			}
			if ( assemblyName == null )
			{
				throw new ArgumentNullException( "assemblyName" );
			}

			Usage = usage;
			AssemblyName = assemblyName;
		}

		public LoggerUsage Usage { get; set; }
		public string AssemblyName { get; set; }
	}
}