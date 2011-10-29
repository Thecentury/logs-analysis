using System;
using System.Collections.Generic;
using System.ComponentModel;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using AdTech.Common.WPF;
using System.Windows.Input;
using LogAnalyzer.GUI.ViewModels.Collections;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LogEntryViewModel : BindingObject
	{
		private readonly LogEntry logEntry;
		private readonly LogFileViewModel parentFile;
		private readonly ILogEntryHost host;
		private readonly int indexInParentCollection = ParallelHelper.IndexNotFound;
		private readonly LogEntriesListViewModel parentViewModel;

		/// <summary>
		/// Индекс в коллекции host.
		/// </summary>
		internal int IndexInParentCollection
		{
			get { return indexInParentCollection; }
		}

		internal ILogEntryHost ParentSparseCollection
		{
			get { return host; }
		}

		internal LogEntryViewModel( LogEntry logEntry, LogFileViewModel parentFile, ILogEntryHost host, LogEntriesListViewModel parentViewModel, int indexInParentCollection )
			: base( logEntry )
		{
			if ( logEntry == null )
				throw new ArgumentNullException( "logEntry" );
			if ( parentFile == null )
				throw new ArgumentNullException( "parentFile" );
			if ( host == null )
				throw new ArgumentNullException( "host" );
			if ( parentViewModel == null )
				throw new ArgumentNullException( "parentViewModel" );

			this.logEntry = logEntry;
			this.parentFile = parentFile;
			this.host = host;
			this.indexInParentCollection = indexInParentCollection;
			this.parentViewModel = parentViewModel;

			// todo обдумать, как еще можно сделать
			if ( logEntry.IsFrozen )
			{
				// Freeze();
			}
		}

		protected override void OnPropertyChangedUnsubscribe()
		{
			host.Release( this );
		}

		#region Properties

		public string Type { get { return logEntry.Type; } }
		public int ThreadId { get { return logEntry.ThreadId; } }
		public DateTime Time { get { return logEntry.Time; } }
		public int IndexInFile { get { return logEntry.LineIndex; } }
		public int LinesCount { get { return logEntry.LinesCount; } }
		public IList<string> Text { get { return logEntry.TextLines; } }
		public LogFileViewModel File { get { return parentFile; } }
		public LogDirectoryViewModel Directory { get { return parentFile.ParentDirectory; } }
		public ApplicationViewModel ApplicationViewModel { get { return Directory.CoreViewModel.ApplicationViewModel; } }

		public LogEntriesListViewModel ParentViewModel
		{
			get { return parentViewModel; }
		}

		public LogEntry LogEntry
		{
			get { return logEntry; }
		}

		private string highlightedColumnName;
		public string HighlightedColumnName
		{
			get { return highlightedColumnName; }
			internal set
			{
				if ( highlightedColumnName == value )
					return;

				highlightedColumnName = value;
				RaisePropertyChanged( "HighlightedColumnName" );
			}
		}

		private bool isDynamicHighlighted;
		public bool IsDynamicHighlighted
		{
			get { return isDynamicHighlighted; }
			internal set
			{
				if ( isDynamicHighlighted == value )
					return;

				isDynamicHighlighted = value;
				RaisePropertyChanged( "IsDynamicHighlighted" );
			}
		}

		public string UnitedText
		{
			get { return logEntry.UnitedText; }
		}

		public bool IsException
		{
			get
			{
				// todo probably optimize for frozen state
				bool isException = LinesCount > 1 && UnitedText.Contains( ":line " );
				return isException;
			}
		}

		#endregion

		protected override void OnInnerPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			linesViewModel = null;
			base.OnInnerPropertyChanged( sender, e );
		}

		// todo аналогично, избавиться от поля.
		private List<MessageLineViewModel> linesViewModel;
		public List<MessageLineViewModel> LinesView
		{
			get
			{
				if ( linesViewModel == null )
				{
					// todo тут не учитывается возможность, что LogEntry обновится
					linesViewModel = new List<MessageLineViewModel>( logEntry.LinesCount );
					FillLinesViewModel();
				}
				return linesViewModel;
			}
		}

		private void FillLinesViewModel()
		{
			foreach ( string line in logEntry.TextLines )
			{
				MessageLineViewModel lineViewModel;
				FileLineInfo lineInfo = logEntry.GetExceptionLine( line );
				// в строке нет информации о методе, вызвавшем исключение
				if ( lineInfo == null )
				{
					lineViewModel = new MessageLineViewModel( line );
				}
				else
				{
					lineViewModel = new ExceptionLineViewModel( lineInfo, line );
				}

				linesViewModel.Add( lineViewModel );
			}
		}

		#region Commands

		private DelegateCommand openFileViewCommand;
		public ICommand OpenFileViewCommand
		{
			get
			{
				if ( openFileViewCommand == null )
				{
					throw new NotImplementedException();
				}

				return openFileViewCommand;
			}
		}

		#region File operations

		// Select file in explorer

		public ICommand SelectFileInExplorerCommand
		{
			get { return File.SelectFileCommand; }
		}

		public string SelectFileInExplorerCommandHeader
		{
			get { return File.ShowInExplorerCommandHeader; }
		}

		// Open folder in explorer

		public ICommand OpenFolderInExplorerCommand
		{
			get { return Directory.OpenFolderCommand; }
		}

		public string OpenFolderInExplorerCommandHeader
		{
			get { return Directory.OpenFolderInExplorerCommandHeader; }
		}

		#endregion

		#region Create new view

		// Create view for this file

		public string CreateViewForFileCommandHeader
		{
			get { return "File \"{0}\\{1}\"".Format2( Directory.DisplayName, File.Name ); }
		}

		public ICommand CreateFileViewCommand
		{
			get { return ApplicationViewModel.CreateAddFileViewCommand( File ); }
		}

		// Create view for directory

		public string CreateViewForDirectoryCommandHeader
		{
			get { return "Directory \"{0}\"".Format2( Directory.DisplayName ); }
		}

		public ICommand CreateDirectoryViewCommand
		{
			get { return ApplicationViewModel.CreateAddDirectoryViewCommand( Directory ); }
		}

		// Create view for thread

		public string CreateThreadViewCommandHeader
		{
			get { return "Thread Id={0}".Format2( ThreadId ); }
		}

		public ICommand CreateThreadViewCommand
		{
			get { return ApplicationViewModel.CreateAddThreadViewCommand( ThreadId ); }
		}

		// Create view for file name

		public string CreateFileNameViewCommandHeader
		{
			get { return "All files with FileName = \"{0}\"".Format2( File.Name ); }
		}

		public ICommand CreateFileNameViewCommand
		{
			get { return ApplicationViewModel.CreateAddFileNameViewCommand( File.Name ); }
		}

		#endregion

		#region Exclude by filters

		// Exclude certain file

		public string ExcludeByCertainFileCommandHeader
		{
			get { return "File \"{0}\\{1}\"".Format2( Directory.DisplayName, File.Name ); }
		}

		public ICommand ExcludeByCertainFileCommand
		{
			get { return ApplicationViewModel.CreateExcludeByCertainFileCommand( this ); }
		}

		// Exclude thread id

		public string ExcludeByThreadIdCommandHeader
		{
			get { return "Thread Id={0}".Format2( ThreadId ); }
		}

		public ICommand ExcludeByThreadIdCommand
		{
			get { return ApplicationViewModel.CreateExcludeByThreadIdCommand( this ); }
		}

		// Exclude by filename

		public string ExcludeByFilenameCommandHeader
		{
			get { return "All files \"{0}\"".Format2( File.Name ); }
		}

		public ICommand ExcludeByFilenameCommand
		{
			get { return ApplicationViewModel.CreateExcludeByFilenameCommand( this ); }
		}

		// Exclude directory

		public string ExcludeDirectoryCommandHeader
		{
			get { return "Directory \"{0}\"".Format2( Directory.DisplayName ); }
		}

		public ICommand ExcludeDirectoryCommand
		{
			get { return ApplicationViewModel.CreateExcludeDirectoryCommand( this ); }
		}

		#endregion

		#endregion
	}
}
