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
			Filter.ExpressionBuilder = _textContainsFilter;
			Brush = Brushes.RoyalBlue.MakeTransparent( 0.5 ).AsFrozen();
		}

		private readonly TextContains _textContainsFilter = new TextContains();

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
			_textContainsFilter.Substring = _substring;
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
