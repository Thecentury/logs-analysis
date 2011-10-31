using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Zip
{
	public sealed class ZipDirectoryFactory : IDirectoryFactory
	{
		public IDirectoryInfo CreateDirectory( LogDirectoryConfigurationInfo config )
		{
			string fileName = null;
			string pathInsideZip = null;
			string path = config.Path;

			if ( path.Contains( "|" ) )
			{
				string[] parts = path.Split( '|' );
				fileName = parts[0];
				pathInsideZip = parts[1];
			}

			if ( path.EndsWith( ".zip", StringComparison.InvariantCultureIgnoreCase ) )
			{
				fileName = path;
			}

			if ( String.IsNullOrEmpty( fileName ) )
				return null;

			return new ZipDirectoryInfo( config, fileName, pathInsideZip );
		}
	}
}
