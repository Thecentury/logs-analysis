using System;
using LogAnalyzer.Extensions;
using LogAnalyzer.Kernel;

namespace LogAnalyzer.Config
{
	public sealed partial class LogDirectoryConfigurationInfo
	{
		public LogDirectoryConfigurationInfo()
		{
			UseCache = false;
		}

		public LogDirectoryConfigurationInfo( string path, string fileNameFilter, string displayName )
		{
			UseCache = false;
			if ( path.IsNullOrEmpty() )
				throw new ArgumentNullException( "path" );

			if ( fileNameFilter.IsNullOrEmpty() )
				throw new ArgumentNullException( "fileNameFilter" );

			if ( displayName.IsNullOrEmpty() )
				throw new ArgumentNullException( "displayName" );

			Path = path;
			FileNameFilter = fileNameFilter;
			DisplayName = displayName;
		}

		/// <summary>
		/// Path to directory.
		/// </summary>
		public string Path { get; set; }
		public string FileNameFilter { get; set; }
		public string DisplayName { get; set; }
		public string EncodingName { get; set; }

		private bool enabled = true;
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}

		public bool UseCache { get; set; }

		// todo brinchuk remove me?
		/// <summary>
		/// Позволяет внедряться в процесс создания IFileInfo в IDirectoryInfo.
		/// </summary>
		public Func<CreateFileInfoContext, IFileInfo> CustomFileCreator { get; set; }
	}
}
