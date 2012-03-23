using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using LogAnalyzer.Config;
using LogAnalyzer.Kernel.Notifications;
using LogAnalyzer.Logging;
using LogAnalyzer.Misc;

namespace LogAnalyzer.Kernel
{
	/// <summary>
	/// Источник уведомлений об изменениях в файлах, который принимает список файлов, не обязательно находящихся в одной папке, 
	/// и уведомляет об изменениях в них.
	/// <para>
	/// Используется при открытии набора файлов, переданных как параметры командной строки.
	/// </para>
	/// </summary>
	internal sealed class PredefinedFilesDirectoryInfo : FileSystemDirectoryInfo
	{
		private readonly IEnumerable<string> _fileNames;

		public PredefinedFilesDirectoryInfo( [NotNull] LogDirectoryConfigurationInfo config ) : this( config, config.PredefinedFiles ) { }

		public PredefinedFilesDirectoryInfo( [NotNull] LogDirectoryConfigurationInfo config, [NotNull] IEnumerable<string> fileNames )
			: base( config )
		{
			if ( config == null ) throw new ArgumentNullException( "config" );
			if ( fileNames == null ) throw new ArgumentNullException( "fileNames" );

			this._fileNames = fileNames;
		}

		public override IEnumerable<IFileInfo> EnumerateFiles( string searchPattern )
		{
			return _fileNames.Select( CreateFile );
		}

		public IFileInfo CreateFile( string fileName )
		{
			string fullPath = System.IO.Path.GetFullPath( fileName );
			return GetFileInfo( fullPath );
		}

		protected override LogNotificationsSourceBase CreateNotificationSource( string filesFilter )
		{
			return new FutureNotificationSource( CreateNotificationSource );
		}

		private LogNotificationsSourceBase CreateNotificationSource()
		{
			ListMultiDictionary<string, string> dirToFileNamesMap =
				new ListMultiDictionary<string, string>( StringComparer.InvariantCultureIgnoreCase );

			foreach ( var fileName in _fileNames )
			{
				try
				{
					string fullPath = System.IO.Path.GetFullPath( fileName );
					string dirName = System.IO.Path.GetDirectoryName( fullPath );

					dirToFileNamesMap.Append( dirName, fileName );
				}
				catch ( IOException exc )
				{
					Logger.Instance.WriteLine( MessageType.Error,
											  string.Format( "PredefinedFilesDirectoryInfo.CreateNotificationSource: '{0}'; Exc = {1}",
															fileName, exc ) );
				}
			}

			List<LogNotificationsSourceBase> notificationsSources = new List<LogNotificationsSourceBase>( dirToFileNamesMap.Count );
			foreach ( var pair in dirToFileNamesMap )
			{
				string dirName = pair.Key;
				var files = pair.Value;
				var notificationsSource = new FileSystemNotificationsSource( dirName, "*",
																			NotifyFilters.Size | NotifyFilters.FileName |
																			NotifyFilters.LastWrite, includeSubdirectories: false );

				var pollingNotificationSource = new PollingFileSystemNotificationSource( dirName, "*", includeSubdirectories: false );

				var composite = new CompositeLogNotificationsSource( notificationsSource, pollingNotificationSource );

				var filteringSource = new FileNameFilteringNotificationSource( composite, files );

				notificationsSources.Add( filteringSource );
			}

			PausableNotificationSource pausableSource =
				new PausableNotificationSource(
					new DelayedLogRecordsSource(
						new CompositeLogNotificationsSource( notificationsSources ) ) );

			return pausableSource;
		}
	}
}
