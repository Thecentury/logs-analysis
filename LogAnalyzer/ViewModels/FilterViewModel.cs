﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AdTech.Common.WPF;
using System.Windows;
using LogAnalyzer.Filters;
using System.Threading;
using System.Threading.Tasks;
using LogAnalyzer.GUI.Views;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class FilterViewModel : LogEntriesListViewModel
	{
		private readonly IList<LogEntry> source;
		private List<LogEntry> filteredEntries = new List<LogEntry>();
		private readonly ThinListWrapper<LogEntry> observableFilteredEntries;
		private CancellationTokenSource cancellationSource = new CancellationTokenSource();

		public int SourceCount
		{
			get { return source.Count; }
		}

		public FilterViewModel( IList<LogEntry> source, ApplicationViewModel applicationViewModel, ExpressionBuilder builder )
			: this( source, applicationViewModel )
		{
			if ( builder == null )
				throw new ArgumentNullException( "builder" );

			filter.ExpressionBuilder = builder;
		}

		public FilterViewModel( IList<LogEntry> source, ApplicationViewModel applicationViewModel )
			: base( applicationViewModel )
		{
			if ( source == null )
				throw new ArgumentNullException( "source" );

			this.source = source;

			observableFilteredEntries = new ThinListWrapper<LogEntry>( filteredEntries );

			Init( observableFilteredEntries );

			filter.Changed += OnFilter_Changed;
		}

		public override string Header
		{
			get
			{
				// удаляем начальные и конечные скобки
				string trimmedStart = filter.ExpressionBuilder.ToExpressionString().Trim( '(' );
				int openBracketsCount = trimmedStart.Count( c => c == '(' );
				int closingBracketsCount = trimmedStart.Count( c => c == ')' );

				string expressionString = trimmedStart;
				if ( openBracketsCount < closingBracketsCount )
				{
					expressionString = expressionString.TrimEnd( ')' );
				}

				return expressionString;
			}
		}

		public override string IconFile
		{
			get
			{
				return MakePackUri( "/Resources/universal.png" );
			}
		}

		private void OnFilter_Changed( object sender, EventArgs e )
		{
			RaisePropertyChanged( "Header" );
			StartFiltration();
			// todo handle filter changes
		}

		protected override void OnClosing()
		{
			if ( IsFiltering && cancellationSource != null )
			{
				cancellationSource.Cancel();
			}
		}

		private readonly ExpressionFilter<LogEntry> filter = new ExpressionFilter<LogEntry>();
		public ExpressionFilter<LogEntry> Filter
		{
			get { return filter; }
		}

		const int FilteringProgressNotificationsCount = 20;
		public void StartFiltration()
		{
			// todo this is temp solution
			if ( IsFiltering )
				return;

			FilteringProgress = 0;
			IsFiltering = true;
			cancellationSource = new CancellationTokenSource();
			int count = source.Count;

			Task.Factory.StartNew( () =>
			{
				FilteringResult localResult = FilteringResult.NotStarted;

				int notificationStep = count / FilterViewModel.FilteringProgressNotificationsCount;
				if ( notificationStep == 0 )
				{
					// не уведомлять никогда
					notificationStep = Int32.MaxValue;
				}

				try
				{
					filteredEntries = ParallelEnumerable.Range( 0, count )
						.AsOrdered()
						.WithCancellation( cancellationSource.Token )
						.Select( i =>
						{
							if ( i % notificationStep == 0 )
							{
								BeginInvokeInUIDispatcher( () =>
								{
									FilteringProgress += 100 / FilterViewModel.FilteringProgressNotificationsCount;
								} );

								// для того, чтобы успевать увидеть изменение прогресса
								// Thread.Sleep( 100 );
							}

							return source[i];
						} )
						.Where( entry => filter.Include( entry ) )
						.ToList();

					observableFilteredEntries.List = filteredEntries;

					localResult = FilteringResult.Completed;
				}
				catch ( OperationCanceledException )
				{
					localResult = FilteringResult.Canceled;
				}

				BeginInvokeInUIDispatcher( () =>
				{
					Result = localResult;
					IsFiltering = false;
				} );
			} );
		}

		public void Cancel()
		{
			cancellationSource.Cancel();
		}

		private FilteringResult result = FilteringResult.NotStarted;
		public FilteringResult Result
		{
			get { return result; }
			private set
			{
				result = value;
				RaisePropertyChanged( "Result" );
			}
		}

		// todo notify about properties change
		private bool isFiltering = false;
		public bool IsFiltering
		{
			get { return isFiltering; }
			private set
			{
				if ( isFiltering == value )
					return;

				isFiltering = value;
				RaisePropertyChanged( "IsFiltering" );
			}
		}

		private int filteringProgress = 0;
		public int FilteringProgress
		{
			get { return filteringProgress; }
			private set
			{
				if ( filteringProgress == value )
					return;

				filteringProgress = value;
				RaisePropertyChanged( "FilteringProgress" );
			}
		}

		private DelegateCommand<RoutedEventArgs> cancelFilteringCommand;
		public ICommand CancelFilteringCommand
		{
			get
			{
				if ( cancelFilteringCommand == null )
					cancelFilteringCommand = new DelegateCommand<RoutedEventArgs>( ExecuteCancelFilterCommand, CanExecuteCancelFilterCommand );

				return cancelFilteringCommand;
			}
		}

		private void ExecuteCancelFilterCommand( object state )
		{
			Cancel();
		}

		private bool CanExecuteCancelFilterCommand( object state )
		{
			return isFiltering;
		}

		protected internal override LogFileViewModel GetFileViewModel( LogEntry logEntry )
		{
			var logFileViewModel = ApplicationViewModel.CoreViewModel.GetFileViewModel( logEntry );
			return logFileViewModel;
		}

		public override LogEntriesListViewModel Clone()
		{
			throw new NotImplementedException();
		}


		private DelegateCommand editFilterCommand;
		public ICommand EditFilterCommand
		{
			get
			{
				if ( editFilterCommand == null )
				{
					editFilterCommand = new DelegateCommand( EditFilterExecute );
				}

				return editFilterCommand;
			}
		}

		public void EditFilterExecute()
		{
			FilterEditorWindow editorWindow = new FilterEditorWindow( Application.Current.MainWindow );
			FilterEditorViewModel editorViewModel = new FilterEditorViewModel( editorWindow );
			bool? dialogResult = editorWindow.ShowDialog();
			if ( dialogResult == true )
			{
				// todo передавать информацию о "владельце" коллекции sourceEntries 
				// (напр., для команды ShowInParentEntriesList)
				ExpressionBuilder filterBuilder = editorViewModel.Builder;
				this.filter.ExpressionBuilder = filterBuilder;
			}
		}

		private DelegateCommand refreshCommand;
		public ICommand RefreshCommand
		{
			get
			{
				if ( refreshCommand == null )
				{
					refreshCommand = new DelegateCommand( RefreshExecute, RefreshCanExecute );
				}

				return refreshCommand;
			}
		}

		public void RefreshExecute()
		{
			StartFiltration();
		}

		public bool RefreshCanExecute()
		{
			return !IsFiltering;
		}
	}

	public enum FilteringResult
	{
		NotStarted,
		Completed,
		Canceled
	}
}