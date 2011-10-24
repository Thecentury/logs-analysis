using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LogAnalyzer.Config;
using LogAnalyzer.Logging;
using LogAnalyzer.Misc;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Источник уведомлений об изменениях в файлах, который принимает список файлов, не обязательно находящихся в одной папке, и уведомляет об изменениях в них.
	/// </summary>
	internal sealed class PredefinedFilesDirectoryInfo : FileSystemDirectoryInfo
	{
		public PredefinedFilesDirectoryInfo( LogDirectoryConfigurationInfo config ) : base( config ) { }

		public override IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			return DirectoryConfig.PredefinedFiles.Select( CreateFile );
		}

		private IFileInfo CreateFile( string fileName )
		{
			string fullPath = System.IO.Path.GetFullPath( fileName );
			return GetFileInfo( fullPath );
		}

		protected override LogNotificationsSourceBase CreateNotificationSource( string path, string filesFilter )
		{
			ListMultiDictionary<string, string> dirToFileNames = new ListMultiDictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );

			foreach ( var fileName in DirectoryConfig.PredefinedFiles )
			{
				try
				{
					string fullPath = System.IO.Path.GetFullPath( fileName );
					string dirName = System.IO.Path.GetDirectoryName( fullPath );

					dirToFileNames.Append( dirName, fileName );
				}
				catch ( IOException exc )
				{
					Logger.Instance.WriteLine( MessageType.Error, string.Format( "PredefinedFilesDirectoryInfo.CreateNotificationSource: '{0}'; Exc = {1}", fileName, exc ) );
				}
			}

			List<LogNotificationsSourceBase> notificationsSources = new List<LogNotificationsSourceBase>( dirToFileNames.Count );
			foreach ( var pair in dirToFileNames )
			{
				string dirName = pair.Key;
				var files = pair.Value;
				FileSystemNotificationsSource notificationsSource = new FileSystemNotificationsSource( dirName, "*", NotifyFilters.Size | NotifyFilters.FileName );
				FileNameFilteringNotificationSource filteringSource = new FileNameFilteringNotificationSource( notificationsSource, files );
				notificationsSources.Add( filteringSource );
			}

			CompositeLogNotificationsSource compositeSource = new CompositeLogNotificationsSource( notificationsSources );

			DelayedLogRecordsSource result = new DelayedLogRecordsSource( compositeSource );
			return result;
		}
	}
}
