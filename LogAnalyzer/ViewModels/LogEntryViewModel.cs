﻿//#define MEASURE_CREATED_COUNT

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using LogAnalyzer.Collections;
using LogAnalyzer.Extensions;
using System.Windows.Input;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.GUI.ViewModels.Collections;
using LogAnalyzer.Logging;
using LogAnalyzer.LoggingTemplates;
using Microsoft.Research.DynamicDataDisplay;
using ColorHelper = LogAnalyzer.GUI.Common.ColorHelper;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class LogEntryViewModel : BindingObject, IAwareOfIndex, IEquatable<LogEntryViewModel>
	{
		private readonly LogEntry _logEntry;
		private readonly LogFileViewModel _parentFile;
		private readonly ILogEntryHost _host;
		private readonly int _indexInParentCollection = ParallelHelper.IndexNotFound;
		private readonly LogEntriesListViewModel _parentViewModel;

		/// <summary>
		/// Индекс в коллекции host.
		/// </summary>
		internal int IndexInParentCollection
		{
			get { return _indexInParentCollection; }
		}

		internal ILogEntryHost ParentSparseCollection
		{
			get { return _host; }
		}

#if MEASURE_CREATED_COUNT
		private static int createdCount;
#endif

		internal LogEntryViewModel( LogEntry logEntry, LogFileViewModel parentFile, ILogEntryHost host,
			LogEntriesListViewModel parentViewModel, int indexInParentCollection )
			: base( logEntry )
		{
			if ( logEntry == null )
			{
				throw new ArgumentNullException( "logEntry" );
			}
			if ( parentFile == null )
			{
				throw new ArgumentNullException( "parentFile" );
			}
			if ( host == null )
			{
				throw new ArgumentNullException( "host" );
			}
			if ( parentViewModel == null )
			{
				throw new ArgumentNullException( "parentViewModel" );
			}

			this._logEntry = logEntry;
			this._parentFile = parentFile;
			this._host = host;
			this._indexInParentCollection = indexInParentCollection;
			this._parentViewModel = parentViewModel;

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

		private static readonly Brush[] templateGroupBrushes = new Brush[]
		                                              	{
		                                              		Brushes.Gold,
		                                              		Brushes.LightGreen.MakeTransparent(0.5).AsFrozen(),
		                                              		Brushes.DeepSkyBlue.MakeTransparent(0.3).AsFrozen()
		                                              	};

		private Paragraph _paragraph;
		public Paragraph Paragraph
		{
			get
			{
				if ( _paragraph == null )
				{
					_paragraph = new Paragraph( new Run( _logEntry.UnitedText ) );

					Task.Factory.StartNew( () =>
											{
												Stopwatch timer = Stopwatch.StartNew();

												var formatRecognizer =
													_parentViewModel.ApplicationViewModel.Config.ResolveNotNull<ILogEntryFormatRecognizer>();

												var gotFormatRecognizerTime = timer.ElapsedMilliseconds;
												timer.Restart();

												var format = formatRecognizer.FindFormat( _logEntry );

												var foundFormat = timer.ElapsedMilliseconds;

												Logger.Instance.WriteVerbose( "Got recognizer in {0} ms, found format in {1} ms", gotFormatRecognizerTime, foundFormat );

												return format;
											} ).ContinueWith( t =>
																{
																	var format = t.Result;
																	if ( format == null )
																	{
																		return;
																	}

																	var regex = format.Usage.Regex;
																	string[] parts = regex.Split( _logEntry.UnitedText );
																	var match = regex.Match( _logEntry.UnitedText );
																	var groupNames = regex.GetGroupNames();

																	List<string> groupValues = groupNames.Select( groupName => match.Groups[groupName].Value ).ToList();

																	List<MessagePart> messageParts = new List<MessagePart>( parts.Length );
																	foreach ( string part in parts )
																	{
																		if ( String.IsNullOrEmpty( part ) )
																		{
																			continue;
																		}

																		int groupValueIndex = groupValues.IndexOf( part );
																		if ( groupValueIndex >= 0 )
																		{
																			string groupName = groupNames[groupValueIndex];
																			if ( groupName[0] == LoggerUsage.TemplatedPartStart )
																			{
																				int groupIndex = Int32.Parse( groupName.Substring( 1 ) );
																				messageParts.Add( new GroupMessagePart( part, groupIndex ) );
																			}
																			else
																			{
																				messageParts.Add( new CommonMessagePart( part ) );
																			}
																		}
																		else
																		{
																			messageParts.Add( new CommonMessagePart( part ) );
																		}
																	}

																	var paragraph = new Paragraph();
																	foreach ( var messagePart in messageParts )
																	{
																		Run run = new Run( messagePart.Text );
																		GroupMessagePart groupPart = messagePart as GroupMessagePart;
																		if ( groupPart != null )
																		{
																			run.Background = templateGroupBrushes[groupPart.GroupNumber % templateGroupBrushes.Length];
																		}

																		paragraph.Inlines.Add( run );
																	}

																	Paragraph = paragraph;
																}, TaskScheduler.FromCurrentSynchronizationContext() );
				}

				return _paragraph;
			}
			private set
			{
				_paragraph = value;
				RaisePropertyChanged( () => Paragraph );
			}
		}

		public void OnSelectionChanged( TextSelection range )
		{
			_parentViewModel.OnSelectedTextChanged( range.Text );
		}

		public string SelectedText { get; set; }

		public bool Equals( LogEntryViewModel other )
		{
			if ( ReferenceEquals( null, other ) )
			{
				return false;
			}
			if ( ReferenceEquals( this, other ) )
			{
				return true;
			}
			bool equals = Equals( other._logEntry, _logEntry );
			return equals;
		}

		public override bool Equals( object obj )
		{
			if ( ReferenceEquals( null, obj ) )
			{
				return false;
			}
			if ( ReferenceEquals( this, obj ) )
			{
				return true;
			}
			if ( obj.GetType() != typeof( LogEntryViewModel ) )
			{
				return false;
			}
			bool equals = Equals( (LogEntryViewModel)obj );
			return equals;
		}

		public override int GetHashCode()
		{
			return _logEntry.GetHashCode();
		}

		protected override void OnPropertyChangedUnsubscribe()
		{
			_host.Release( this );
		}

		#region Properties

		public string Type { get { return _logEntry.Type; } }
		public int ThreadId { get { return _logEntry.ThreadId; } }
		public DateTime Time { get { return _logEntry.Time; } }
		public int IndexInFile { get { return _logEntry.LineIndex; } }
		public int LinesCount { get { return _logEntry.LinesCount; } }
		public LogFileViewModel File { get { return _parentFile; } }
		public LogDirectoryViewModel Directory { get { return _parentFile.ParentDirectory; } }
		public ApplicationViewModel ApplicationViewModel { get { return Directory.CoreViewModel.ApplicationViewModel; } }

		public LogEntriesListViewModel ParentViewModel
		{
			get { return _parentViewModel; }
		}

		public LogEntry LogEntry
		{
			get { return _logEntry; }
		}

		private string _highlightedColumnName;
		public string HighlightedColumnName
		{
			get { return _highlightedColumnName; }
			internal set
			{
				if ( _highlightedColumnName == value )
				{
					return;
				}

				_highlightedColumnName = value;
				RaisePropertyChanged( "HighlightedColumnName" );
			}
		}

		private bool _isDynamicHighlighted;
		public bool IsDynamicHighlighted
		{
			get { return _isDynamicHighlighted; }
			internal set
			{
				if ( _isDynamicHighlighted == value )
				{
					return;
				}

				_isDynamicHighlighted = value;
				RaisePropertyChanged( "IsDynamicHighlighted" );
			}
		}

		private string _timeDelta;
		public string TimeDelta
		{
			get { return _timeDelta; }
			set
			{
				_timeDelta = value;
				RaisePropertyChanged( "TimeDelta" );
			}
		}

		public string UnitedText
		{
			get { return _logEntry.UnitedText; }
		}

		public bool IsException
		{
			get
			{
				bool isException = LinesCount > 1 && UnitedText.Contains( ":line " );
				return isException;
			}
		}

		public Brush LogNameBackground
		{
			get
			{
				Brush logNameBackground = LogFileNameBrushesCache.Gradient.GetBrush( File.Name.GetHashCode() );
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

		#region Parent tab views

		public List<MenuItemViewModel> ParentTabViews
		{
			get
			{
				List<MenuItemViewModel> parents = new List<MenuItemViewModel>();

				var parent = _parentViewModel.ParentView;
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

		private DelegateCommand _lockTimeDeltaCommand;
		public ICommand LockTimeDeltaCommand
		{
			get
			{
				if ( _lockTimeDeltaCommand == null )
				{
					_lockTimeDeltaCommand = new DelegateCommand( LockTimeDeltaExecute );
				}
				return _lockTimeDeltaCommand;
			}
		}


		private void LockTimeDeltaExecute()
		{
			_parentViewModel.TimeDeltaLockedEntry = this;
		}

		public ICommand RemoveTimeDeltaLockCommand
		{
			get { return _parentViewModel.RemoveTimeDeltaLockCommand; }
		}

		public bool LockTimeDeltaCommandVisible
		{
			get { return !_parentViewModel.HasTimeDeltaLockedEntry; }
		}

		public bool RemoveTimeDeltaLockCommandVisible
		{
			get { return _parentViewModel.HasTimeDeltaLockedEntry; }
		}

		#region Highlight by commands

		private ICommand CreateHighlightCommand( ExpressionBuilder filter )
		{
			DelegateCommand command = new DelegateCommand( () =>
			{
				HighlightingViewModel vm = new HighlightingViewModel( _parentViewModel, filter, new SolidColorBrush( ColorHelper.GetRandomColor() ).AsFrozen() );
				_parentViewModel.HighlightingFilters.Add( vm );
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
						new Argument().GetProperty( "ParentLogFile" ).IsEqual( ExpressionBuilder.CreateConstant( _logEntry.ParentLogFile ) )
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
		private DelegateCommand _copyFileNameCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyFileNameCommand
		{
			get
			{
				if ( _copyFileNameCommand == null )
					_copyFileNameCommand = new DelegateCommand( () => Clipboard.SetText( File.Name ) );

				return _copyFileNameCommand;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand _copyFullPathCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyFullPathCommand
		{
			get
			{
				if ( _copyFullPathCommand == null )
					_copyFullPathCommand = new DelegateCommand( () => Clipboard.SetText( File.LogFile.FullPath ) );

				return _copyFullPathCommand;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand _copyFileLocationCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyFileLocationCommand
		{
			get
			{
				if ( _copyFileLocationCommand == null )
					_copyFileLocationCommand = new DelegateCommand( () =>
																	{
																		string location = Path.GetDirectoryName( File.LogFile.FullPath );
																		Clipboard.SetText( location );
																	} );

				return _copyFileLocationCommand;
			}
		}

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		private DelegateCommand _copyMessageCommand;

		[DebuggerBrowsable( DebuggerBrowsableState.Never )]
		public ICommand CopyMessageCommand
		{
			get
			{
				if ( _copyMessageCommand == null )
				{
					_copyMessageCommand = new DelegateCommand( () => Clipboard.SetText( UnitedText ) );
				}

				return _copyMessageCommand;
			}
		}

		#endregion

		#endregion

		public Visibility ShowInParentViewVisibility
		{
			get { return _parentViewModel is CoreViewModel ? Visibility.Collapsed : Visibility.Visible; }
		}

		#region IAwareOfIndex Members

		int IAwareOfIndex.IndexInParentCollection
		{
			get { return _indexInParentCollection; }
		}

		#endregion

	}
}
