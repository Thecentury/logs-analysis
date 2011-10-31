using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;

namespace LogAnalyzer.Kernel
{
	public interface IDirectoryFactory
	{
		IDirectoryInfo CreateDirectory( LogDirectoryConfigurationInfo config );
	}
}
