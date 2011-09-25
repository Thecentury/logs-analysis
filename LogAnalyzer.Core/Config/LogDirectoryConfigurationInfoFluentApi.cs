using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public class LogDirectoryConfigurationInfoFluentApi : LogAnalyzerConfigurationFluentApi
	{
		private readonly LogDirectoryConfigurationInfo logDirectory = null;

		internal LogDirectoryConfigurationInfoFluentApi( LogDirectoryConfigurationInfo logDirectory, LogAnalyzerConfiguration config )
			: base( config )
		{
			this.logDirectory = logDirectory;
		}

		public LogDirectoryConfigurationInfoFluentApi WithEncoding( string encodingName )
		{
			logDirectory.EncodingName = encodingName;
			return this;
		}

		public LogDirectoryConfigurationInfoFluentApi AsDisabled()
		{
			logDirectory.Enabled = false;
			return this;
		}
	}
}
