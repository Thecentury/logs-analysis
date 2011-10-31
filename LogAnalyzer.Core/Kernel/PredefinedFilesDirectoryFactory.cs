using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	public sealed class PredefinedFilesDirectoryFactory : IDirectoryFactory
	{
		public IDirectoryInfo CreateDirectory( LogDirectoryConfigurationInfo config )
		{
			if ( config.PredefinedFiles.Count > 0 )
				return new PredefinedFilesDirectoryInfo( config );
			else
				return null;
		}
	}
}
