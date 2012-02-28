//#define MEASURE_CREATED_COUNT

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using System.Windows.Input;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels.Collections;
using LogAnalyzer.GUI.ViewModels.Colorizing;
using LogAnalyzer.Logging;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LogEntryViewModel : BindingObject, IAwareOfIndex, IEquatable<LogEntryViewModel>
	{
		private readonly LogEntry logEntry;
		private readonly LogFileViewModel parentFile;
		private readonly ILogEntryHost host;
		private readonly int indexInParentCollection = ParallelHelper.IndexNotFound;
		private readonly LogEntriesListViewModel parentViewModel;
		private readonly ColorizationManager colorizationManager;

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

#if MEASURE_CREATED_COUNT
		private static int createdCount;
#endif

		internal LogEntryViewModel( LogEntry logEntry, LogFileViewModel parentFile, ILogEntryHost host,
			LogEntriesListViewModel parentViewModel, int indexInParentCollection )
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

			colorizationManager = parentViewModel.ApplicationViewModel.Config.ResolveNotNull<ColorizationManager>();

#if MEASURE_CREATED_COUNT
			Interlocked.Increment( ref createdCount );
			Logger.Instance.WriteInfo( "LogEntryViewModel.ctor: CreatedCount = {0}", createdCount );
#endif

			// todo обдумать, как еще можно сделать
			if ( logEntry.IsFrozen )
			{
				// Freeze();
			}
		}

#if MEASURE_CREATED_COUNT
		~LogEntryViewModel()
		{
			Interlocked.Decrement(ref createdCount);
			Logger.Instance.WriteInfo( "LogEntryViewModel.~dtor: CreatedCount = {0}", createdCount );
		}
#endif

		// todo brinchuk remove me
		private ControlTemplate template;
		public ControlTemplate Template
		{
			get
			{
				if ( template == null )
				{
					UpdateColorization();
				}
				return template;
			}
		}

		// todo brinchuk remove me
		private void UpdateColorization()
		{
			var colorizingTemplate = colorizationManager.GetTemplateForEntry( logEntry );
			template = colorizingTemplate.Template;
			templateContext = colorizingTemplate.GetDataContext( logEntry );
		}

		// todo brinchuk remove me
		private object templateContext;
		public object TemplateContext
		{
			get
			{
				if ( templateContext == null )
				{
					UpdateColorization();
				}
				return templateContext;
			}
		}

		public FlowDocument Document { get; set; }

		public void OnSelectionChanged( TextSelection range )
		{
			parentViewModel.OnSelectedTextChanged( range.Text );
		}

		public string SelectedText { get; set; }

		public bool Equals( LogEntryViewModel other )
		{
			if ( ReferenceEquals( null, other ) )
				return false;
			if ( ReferenceEquals( this, other ) )
				return true;
			bool equals = Equals( other.logEntry, logEntry );
			return equals;
		}

		public override bool Equals( object obj )
		{
			if ( ReferenceEquals( null, obj ) )
				return false;
			if ( ReferenceEquals( this, obj ) ) return true;
			if ( obj.GetType() != typeof( LogEntryViewModel ) ) return false;
			bool equals = Equals( (LogEntryViewModel)obj );
			return equals;
		}

		public override int GetHashCode()
		{
			return logEntry.GetHashCode();
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
			get
			{
				string text = logEntry.UnitedText;
				return text;
			}
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

		private static readonly Dictionary<int, Brush> logFileNameBrushesCache = new Dictionary<int, Brush>();

		public Brush LogNameBackground
		{
			get
			{
				int hashCode = File.Name.GetHashCode();
				Brush logNameBackground;

				if ( !logFileNameBrushesCache.TryGetValue( hashCode, out logNameBackground ) )
				{
					double hue = (hashCode - (double)Int32.MinValue) / ((double)Int32.MaxValue - Int32.MinValue) * 360;
					HsbColor hsbColor = new HsbColor( hue, 0.2, 1 );
					HsbColor darkerColor = new HsbColor( hue, 0.2, 0.95 );
					logNameBackground = new LinearGradientBrush( hsbColor.ToArgbColor(), darkerColor.ToArgbColor(), 90 );
					logNameBackground.Freeze();

					logFileNameBrushesCache.Add( hashCode, logNameBackground );
				}

				return logNameBackground;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private readonly ObservableCollection<HighlightingViewModel> _highlightedByList = new ObservableCollection<HighlightingViewModel>();
		/// <summary>
		/// Список фильтров, которыми подсвечена данная запись.
		/// </summary>
		public ObservableCollection<HighlightingViewModel> HighlightedByList
		{
			get { return _highlightedByList; }
		}

		#endregion

		protected override void OnInnerPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			_linesViewModel = null;
			base.OnInnerPropertyChanged( sender, e );
		}

		// todo аналогично, избавиться от поля.
		private List<MessageLineViewModel> _linesViewModel;
		public List<MessageLineViewModel> LinesView
		{
			get
			{
				if ( _linesViewModel == null )
				{
					// todo тут не учитывается возможность, что LogEntry обновится
					_linesViewModel = new List<MessageLineViewModel>( logEntry.LinesCount );
					FillLinesViewModel();
				}
				return _linesViewModel;
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

				_linesViewModel.Add( lineViewModel );
			}
		}

		#region Parent tab views

		public List<MenuItemViewModel> ParentTabViews
		{
			get
			{
				List<MenuItemViewModel> parents = new List<MenuItemViewModel>();

				var parent = parentViewModel.ParentView;
				while ( parent != null )
				{
					MenuItemViewModel vm = new MenuItemViewModel
											{
												IconSource = parent.IconFile,
												Header = parent.Header,
												Tooltip = parent.Tooltip,
												Command = ApplicationViewModel.CreateShowInParentViewCommand( this, parent )
											};

					parents.Add( vm );

					parent = parent.ParentView;
				}

				return parents;
			}
		}

		#endregion

		#region Commands

		#region File operations

		// Select file in explorer

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand SelectFileInExplorerCommand
		{
			get { return File.SelectFileCommand; }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string SelectFileInExplorerCommandHeader
		{
			get { return File.ShowInExplorerCommandHeader; }
		}

		// Open folder in explorer

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand OpenFolderInExplorerCommand
		{
			get { return Directory.OpenFolderCommand; }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string OpenFolderInExplorerCommandHeader
		{
			get { return Directory.OpenFolderInExplorerCommandHeader; }
		}

		#endregion

		#region Create new view

		// Create view for this file

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string CreateViewForFileCommandHeader
		{
			get { return "File \"{0}\\{1}\"".Format2( Directory.DisplayName, File.Name ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CreateFileViewCommand
		{
			get { return ApplicationViewModel.CreateAddFileViewCommand( File ); }
		}

		// Create view for directory

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string CreateViewForDirectoryCommandHeader
		{
			get { return "Directory \"{0}\"".Format2( Directory.DisplayName ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CreateDirectoryViewCommand
		{
			get { return ApplicationViewModel.CreateAddDirectoryViewCommand( Directory ); }
		}

		// Create view for thread

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string CreateThreadViewCommandHeader
		{
			get { return "Thread Id={0}".Format2( ThreadId ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CreateThreadViewCommand
		{
			get { return ApplicationViewModel.CreateAddThreadViewCommand( ThreadId ); }
		}

		// Create view for file name

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string CreateFileNameViewCommandHeader
		{
			get { return "All files with FileName = \"{0}\"".Format2( File.Name ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CreateFileNameViewCommand
		{
			get { return ApplicationViewModel.CreateAddFileNameViewCommand( File.Name ); }
		}

		// Create view for message type

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string CreateMessageTypeViewCommandHeader
		{
			get { return string.Format( "MessageType \"{0}\"", Type ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CreateMessageTypeViewCommand
		{
			get { return ApplicationViewModel.CreateAddMessageTypeViewCommand( Type ); }
		}

		#endregion

		#region Exclude by filters

		// Exclude certain file

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string ExcludeByCertainFileCommandHeader
		{
			get { return "File \"{0}\\{1}\"".Format2( Directory.DisplayName, File.Name ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand ExcludeByCertainFileCommand
		{
			get { return ApplicationViewModel.CreateExcludeByCertainFileCommand( this ); }
		}

		// Exclude thread id

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string ExcludeByThreadIdCommandHeader
		{
			get { return "Thread Id={0}".Format2( ThreadId ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand ExcludeByThreadIdCommand
		{
			get { return ApplicationViewModel.CreateExcludeByThreadIdCommand( this ); }
		}

		// Exclude by filename

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string ExcludeByFilenameCommandHeader
		{
			get { return "All files \"{0}\"".Format2( File.Name ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand ExcludeByFilenameCommand
		{
			get { return ApplicationViewModel.CreateExcludeByFilenameCommand( this ); }
		}

		// Exclude directory

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string ExcludeDirectoryCommandHeader
		{
			get { return "Directory \"{0}\"".Format2( Directory.DisplayName ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand ExcludeDirectoryCommand
		{
			get { return ApplicationViewModel.CreateExcludeDirectoryCommand( this ); }
		}

		// Exclude by message severity

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string ExcludeByMessageTypeCommandHeader
		{
			get { return String.Format( "MessageType \"{0}\"", Type ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand ExcludeByMessageTypeCommand
		{
			get { return ApplicationViewModel.CreateExcludeByMessageTypeCommand( this ); }
		}

		#endregion

		#region Highlight by commands

		private ICommand CreateHighlightCommand( ExpressionBuilder filter )
		{
			DelegateCommand command = new DelegateCommand( () =>
			{
				HighlightingViewModel vm = new HighlightingViewModel( parentViewModel, filter, new SolidColorBrush( ColorHelper.GetRandomColor() ) );
				parentViewModel.HighlightingFilters.Add( vm );
			} );

			return command;
		}

		// Highlight by certain file

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string HighlightByCertainFileCommandHeader
		{
			get { return "File \"{0}\\{1}\"".Format2( Directory.DisplayName, File.Name ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand HighlightByCertainFileCommand
		{
			get
			{
				return CreateHighlightCommand(
						new Argument().GetProperty( "ParentLogFile" ).IsEqual( ExpressionBuilder.CreateConstant( logEntry.ParentLogFile ) )
					);
			}
		}

		// Highlight thread id

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string HighlightByThreadIdCommandHeader
		{
			get { return "Thread Id={0}".Format2( ThreadId ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand HighlightByThreadIdCommand
		{
			get { return CreateHighlightCommand( new ThreadIdEquals( ThreadId ) ); }
		}

		// Highlight by filename

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string HighlightByFilenameCommandHeader
		{
			get { return "All files \"{0}\"".Format2( File.Name ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand HighlightByFilenameCommand
		{
			get { return CreateHighlightCommand( new FileNameEquals( File.Name ) ); }
		}

		// Highlight directory

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string HighlightDirectoryCommandHeader
		{
			get { return "Directory \"{0}\"".Format2( Directory.DisplayName ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand HighlightDirectoryCommand
		{
			get
			{
				return CreateHighlightCommand(
					new Argument()
					.GetProperty( "ParentLogFile" )
					.GetProperty( "ParentDirectory" )
					.GetProperty( "Path" )
					.IsEqual( new StringConstant( Directory.Path ) ) );
			}
		}

		// Highlight by message severity

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public string HighlightByMessageTypeCommandHeader
		{
			get { return String.Format( "MessageType \"{0}\"", Type ); }
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand HighlightByMessageTypeCommand
		{
			get { return CreateHighlightCommand( new MessageTypeEquals( Type ) ); }
		}

		#endregion

		#region Copy to clipboard

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand copyFileNameCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyFileNameCommand
		{
			get
			{
				if ( copyFileNameCommand == null )
					copyFileNameCommand = new DelegateCommand( () => Clipboard.SetText( File.Name ) );

				return copyFileNameCommand;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand copyFullPathCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyFullPathCommand
		{
			get
			{
				if ( copyFullPathCommand == null )
					copyFullPathCommand = new DelegateCommand( () => Clipboard.SetText( File.LogFile.FullPath ) );

				return copyFullPathCommand;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand copyFileLocationCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyFileLocationCommand
		{
			get
			{
				if ( copyFileLocationCommand == null )
					copyFileLocationCommand = new DelegateCommand( () =>
																	{
																		string location = Path.GetDirectoryName( File.LogFile.FullPath );
																		Clipboard.SetText( location );
																	} );

				return copyFileLocationCommand;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand copyMessageCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyMessageCommand
		{
			get
			{
				if ( copyMessageCommand == null )
					copyMessageCommand = new DelegateCommand( () => Clipboard.SetText( UnitedText ) );

				return copyMessageCommand;
			}
		}

		#endregion

		#endregion

		public Visibility ShowInParentViewVisibility
		{
			get { return parentViewModel is CoreViewModel ? Visibility.Collapsed : Visibility.Visible; }
		}

		#region IAwareOfIndex Members

		int IAwareOfIndex.IndexInParentCollection
		{
			get { return indexInParentCollection; }
		}

		#endregion

	}
}
