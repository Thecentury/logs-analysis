using System;
using System.Collections.Generic;
using System.Windows;
using LogAnalyzer.Extensions;
using System.Windows.Input;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.OverviewGui;
using LogAnalyzer.Kernel.Notifications;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LogFileViewModel : LogEntriesListViewModel
	{
		private readonly LogFile _logFile;
		private readonly LogDirectoryViewModel _parentDirectory;

		public LogDirectoryViewModel ParentDirectory
		{
			get { return _parentDirectory; }
		}

		public LogFile LogFile
		{
			get { return _logFile; }
		}

		public LogFileViewModel( LogFile logFile, LogDirectoryViewModel parent )
			: base( parent.CoreViewModel.ApplicationViewModel )
		{
			if ( logFile == null )
				throw new ArgumentNullException( "logFile" );

			this._logFile = logFile;
			this._parentDirectory = parent;

			Init( logFile.LogEntries );
		}

		#region Commands

		private DelegateCommand<RoutedEventArgs> _selectFileCommand;
		public ICommand SelectFileCommand
		{
			get
			{
				if ( _selectFileCommand == null )
				{
					_selectFileCommand = new DelegateCommand<RoutedEventArgs>( _ => WindowsInterop.SelectInExplorer( _logFile.FullPath ));
				}

				return _selectFileCommand;
			}
		}

		#endregion

		#region Properties

		public string Name { get { return _logFile.Name; } }

		#endregion

		public string ShowInExplorerCommandHeader
		{
			get { return "Show \"{0}\" in Explorer".Format2( Name ); }
		}

		protected override void AddLogFileOverview( IList<IOverviewViewModel> overviewsList )
		{
			// do nothing
		}

		protected override LogNotificationsSourceBase GetNotificationSource()
		{
			return _logFile.ParentDirectory.NotificationsSource;
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			return this;
		}

		public override LogEntriesListViewModel Clone()
		{
			LogFileViewModel clone = new LogFileViewModel( _logFile, _parentDirectory );
			return clone;
		}

		public override string Header
		{
			get
			{
				return "File \"{0}\"".Format2( Name );
			}
		}

		public override string IconFile
		{
			get
			{
				return MakePackUri( "/Resources/document-text.png" );
			}
		}
	}
}
