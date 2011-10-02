using System;
using System.Text;

namespace LogAnalyzer.Config
{
	public sealed partial class LogDirectoryConfigurationInfo
	{
		// string path, string fileNameFilter, string displayName
		public static LogDirectoryConfigurationInfo CreateNew()
		{
			return new LogDirectoryConfigurationInfo();
		}

		public LogDirectoryConfigurationInfo WithPath( string path )
		{
			Path = path;
			return this;
		}

		public LogDirectoryConfigurationInfo WithFileNameFilter( string fileNameFilter )
		{
			if ( fileNameFilter == null ) throw new ArgumentNullException( "fileNameFilter" );
			FileNameFilter = fileNameFilter;

			return this;
		}

		public LogDirectoryConfigurationInfo WithDisplayName( string displayName )
		{
			if ( displayName == null ) throw new ArgumentNullException( "displayName" );
			DisplayName = displayName;

			return this;
		}

		public LogDirectoryConfigurationInfo WithEncoding( string encodingName )
		{
			if ( encodingName == null ) throw new ArgumentNullException( "encodingName" );
			EncodingName = encodingName;

			return this;
		}

		public LogDirectoryConfigurationInfo AsDisabled()
		{
			Enabled = false;

			return this;
		}
	}
}
