using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;
using LogAnalyzer.Extensions;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Controls;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LogFileViewModel : LogEntriesListViewModel
	{
		private readonly LogFile logFile;
		private readonly LogDirectoryViewModel parentDirectory;

		public LogDirectoryViewModel ParentDirectory
		{
			get { return parentDirectory; }
		}

		public LogFile LogFile
		{
			get { return logFile; }
		}

		public LogFileViewModel( LogFile logFile, LogDirectoryViewModel parent )
			: base( parent.CoreViewModel.ApplicationViewModel )
		{
			if ( logFile == null )
				throw new ArgumentNullException( "logFile" );

			this.logFile = logFile;
			this.parentDirectory = parent;

			Init( logFile.LogEntries );
		}

		#region Commands

		private DelegateCommand<RoutedEventArgs> selectFileCommand;
		public ICommand SelectFileCommand
		{
			get
			{
				if ( selectFileCommand == null )
				{
					selectFileCommand = new DelegateCommand<RoutedEventArgs>( _ =>
					{
						WindowsInterop.SelectInExplorer( logFile.FullPath );
					} );
				}

				return selectFileCommand;
			}
		}

		#endregion

		#region Properties

		public string Name { get { return logFile.Name; } }
		public long LinesCount { get { return logFile.LinesCount; } }

		#endregion

		public string ShowInExplorerCommandHeader
		{
			get { return "Show \"{0}\" in Explorer".Format2( Name ); }
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			return this;
		}

		public override LogEntriesListViewModel Clone()
		{
			LogFileViewModel clone = new LogFileViewModel( logFile, parentDirectory );
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
