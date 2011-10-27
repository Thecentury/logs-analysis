using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		private readonly int indexInParentCollection = -1;

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

		internal LogEntryViewModel( LogEntry logEntry, LogFileViewModel parentFile, ILogEntryHost host, LogEntriesListViewModel parentList, int indexInParentCollection )
			: base( logEntry )
		{
			if ( logEntry == null )
				throw new ArgumentNullException( "logEntry" );
			if ( parentFile == null )
				throw new ArgumentNullException( "parentFile" );
			if ( host == null )
				throw new ArgumentNullException( "host" );
			if ( parentList == null )
				throw new ArgumentNullException( "parentList" );

			this.logEntry = logEntry;
			this.parentFile = parentFile;
			this.host = host;
			this.indexInParentCollection = indexInParentCollection;

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

		public ICommand SelectFileInExplorerCommand
		{
			get { return File.SelectFileCommand; }
		}

		public string SelectFileInExplorerCommandHeader
		{
			get { return File.ShowInExplorerCommandHeader; }
		}

		public ICommand OpenFolderInExplorerCommand
		{
			get { return Directory.OpenFolderCommand; }
		}

		public string OpenFolderInExplorerCommandHeader
		{
			get { return Directory.OpenFolderInExplorerCommandHeader; }
		}

		public string CreateViewForFileCommandHeader
		{
			get { return "File \"{0}\\{1}\"".Format2( Directory.DisplayName, File.Name ); }
		}

		public string CreateViewForDirectoryCommandHeader
		{
			get { return "Directory \"{0}\"".Format2( Directory.DisplayName ); }
		}

		public ICommand CreateFileViewCommand
		{
			get { return ApplicationViewModel.CreateAddFileViewCommand( File ); }
		}

		public ICommand CreateDirectoryViewCommand
		{
			get { return ApplicationViewModel.CreateAddDirectoryViewCommand( Directory ); }
		}

		public string CreateThreadViewCommandHeader
		{
			get { return "Thread Id={0}".Format2( ThreadId ); }
		}

		public ICommand CreateThreadViewCommand
		{
			get { return ApplicationViewModel.CreateAddThreadViewCommand( ThreadId ); }
		}

		public string CreateFileNameViewCommandHeader
		{
			get { return "All files with FileName = {0}".Format2( File.Name ); }
		}

		public ICommand CreateFileNameViewCommand
		{
			get { return ApplicationViewModel.CreateAddFileNameViewCommand( File.Name ); }
		}

		#endregion
	}
}
