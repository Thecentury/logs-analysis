using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Extensions;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class SearchViewModel : HighlightingViewModel
	{
		public SearchViewModel( LogEntriesListViewModel parentList )
			: base( parentList, new AlwaysFalse() )
		{
			base.Filter.ExpressionBuilder = textContainsFilter;
			Brush = Brushes.RoyalBlue;
		}

		private readonly TextContains textContainsFilter = new TextContains();

		private string substring;
		public string Substring
		{
			get { return substring; }
			set
			{
				substring = value;
				RaisePropertyChanged( "Substring" );
			}
		}

		#region Commands

		// LaunchSearch command

		private DelegateCommand launchSearchCommand;
		public ICommand LaunchSearchCommand
		{
			get
			{
				if ( launchSearchCommand == null )
					launchSearchCommand = new DelegateCommand( LaunchSearchExecute, LaunchSearchCanExecute );

				return launchSearchCommand;
			}
		}

		private void LaunchSearchExecute()
		{
			textContainsFilter.Substring = substring;
			if ( MoveToFirstHighlightedCommand.CanExecute() )
			{
				MoveToFirstHighlightedCommand.Execute();
			}
		}

		private bool LaunchSearchCanExecute()
		{
			return !String.IsNullOrEmpty( Substring );
		}

		#endregion
	}
}
