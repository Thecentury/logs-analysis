using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Extensions;
using Microsoft.Research.DynamicDataDisplay;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class SearchViewModel : HighlightingViewModel
	{
		public SearchViewModel( LogEntriesListViewModel parentList )
			: base( parentList, new AlwaysFalse() )
		{
			Brush = Brushes.RoyalBlue.MakeTransparent( 0.5 ).AsFrozen();
		}

		private readonly TextContains _textContainsFilter = new TextContains();
		private readonly TextMatchesRegex _regexMatchesFilter = new TextMatchesRegex();

		private bool _isRegexSearch;
		public bool IsRegexSearch
		{
			get { return _isRegexSearch; }
			set
			{
				_isRegexSearch = value;
				RaisePropertyChanged( "IsRegexSearch" );
			}
		}

		private string _substring;
		public string Substring
		{
			get { return _substring; }
			set
			{
				_substring = value;
				RaisePropertyChanged( "Substring" );
				HaveSearched = false;
			}
		}

		private bool _haveSearched;
		private bool HaveSearched
		{
			get { return _haveSearched; }
			set
			{
				_haveSearched = value;
				RaisePropertyChanged( "FoundBarVisibility" );
			}
		}

		public Visibility FoundBarVisibility
		{
			get { return !_haveSearched ? Visibility.Collapsed : Visibility.Visible; }
		}

		#region Commands

		protected override bool RemoveHighlightingCanExecute()
		{
			return false;
		}

		protected override bool ShowEditorCanExecute()
		{
			return false;
		}

		// Clear search pattern command

		private DelegateCommand _clearSearchBoxCommand;
		public ICommand ClearSearchBoxCommand
		{
			get
			{
				if ( _clearSearchBoxCommand == null )
					_clearSearchBoxCommand = new DelegateCommand( () => Substring = null, () => Substring != null );

				return _clearSearchBoxCommand;
			}
		}

		// LaunchSearch command

		private DelegateCommand _launchSearchCommand;
		public ICommand LaunchSearchCommand
		{
			get
			{
				if ( _launchSearchCommand == null )
					_launchSearchCommand = new DelegateCommand( LaunchSearchExecute, LaunchSearchCanExecute );

				return _launchSearchCommand;
			}
		}

		private void LaunchSearchExecute()
		{
			if ( _isRegexSearch )
			{
				_regexMatchesFilter.Pattern = _substring;
				Filter.ExpressionBuilder = _regexMatchesFilter;
			}
			else
			{
				_textContainsFilter.Substring = _substring;
				Filter.ExpressionBuilder = _textContainsFilter;
			}

			HaveSearched = true;
			MoveToFirstHighlightedCommand.ExecuteIfCan();
		}

		private bool LaunchSearchCanExecute()
		{
			return !String.IsNullOrEmpty( Substring );
		}

		#endregion
	}
}
