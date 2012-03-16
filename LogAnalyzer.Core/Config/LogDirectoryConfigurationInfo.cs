using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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

		public LogDirectoryConfigurationInfo( [NotNull]string path, string fileNameFilter, string displayName )
		{
			UseCache = false;
			if ( path.IsNullOrEmpty() )
			{
				throw new ArgumentNullException( "path" );
			}

			if ( displayName == null )
			{
				throw new ArgumentNullException( "displayName" );
			}

			Path = path;
			FileNameFilter = fileNameFilter;
			DisplayName = displayName;
		}

		/// <summary>
		/// Path to directory.
		/// </summary>
		public string Path { get; set; }

		private string _fileNameFilter = "*.log";
		public string FileNameFilter
		{
			get { return _fileNameFilter; }
			set { _fileNameFilter = value; }
		}

		public string DisplayName { get; set; }
		private string _encodingName = "windows-1251";
		public string EncodingName
		{
			get { return _encodingName; }
			set { _encodingName = value; }
		}

		private bool _enabled = true;
		public bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}

		public bool IncludeNestedDirectories { get; set; }

		public bool UseCache { get; set; }

		private readonly List<string> _predefinedFiles = new List<string>();
		/// <summary>
		/// Список из заданных наперед имен файлов. Используется при открытии набора файлов при вызове из системы.
		/// </summary>
		public List<string> PredefinedFiles
		{
			get { return _predefinedFiles; }
		}

		public ILogLineParser LineParser { get; set; }

		private bool _notificationsEnabled = true;
		public bool NotificationsEnabled
		{
			get { return _notificationsEnabled; }
			set { _notificationsEnabled = value; }
		}
	}
}
