using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	public sealed class DefaultDirectoryFactory : IDirectoryFactory
	{
		public IDirectoryInfo CreateDirectory( LogDirectoryConfigurationInfo config )
		{
			return new FileSystemDirectoryInfo( config );
		}
	}
}
