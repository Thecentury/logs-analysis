using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LogAnalyzer.Filters;
using System.Collections.ObjectModel;

namespace LogAnalyzer.GUI.ViewModel
{
	internal class HighlightManager
	{
		private readonly ObservableCollection<HighlightingViewModel> highlightList;

		public HighlightManager( ObservableCollection<HighlightingViewModel> highlightList )
		{
			if ( highlightList == null )
				throw new ArgumentNullException( "highlightList" );

			this.highlightList = highlightList;
		}

		public void Register( LogEntryViewModel entry ) { }

		public void Unregister( LogEntryViewModel entry ) { }

		public static void ProcessCellSelection( RoutedEventArgs e )
		{
			DataGridCell cell = e.OriginalSource as DataGridCell;
			if ( cell == null )
			{
				return;
			}

			LogEntryViewModel sourceEntry = (LogEntryViewModel)cell.DataContext;

			string bindingPath = null;
			DataGridTextColumn textColumn = cell.Column as DataGridTextColumn;
			if ( textColumn != null )
			{
				bindingPath = ((Binding)textColumn.Binding).Path.Path;
			}

			ExpressionBuilder highlightFilterBuilder = null;
			switch ( bindingPath )
			{
				case "Type":
					highlightFilterBuilder = new MessageTypeEquals( sourceEntry.Type );
					break;
				case "ThreadId":
					highlightFilterBuilder = new ThreadIdEquals( sourceEntry.ThreadId );
					break;
				case "File.Name":
					highlightFilterBuilder = new FileNameEquals( sourceEntry.File.Name );
					break;
				case "Directory.DisplayName":
					highlightFilterBuilder =
						new Equals(
							new GetProperty(
								new GetProperty(
									new GetProperty(
										new Argument(),
										"ParentLogFile" ),
									"ParentDirectory" ),
								"DisplayName" ),
							new StringConstant( sourceEntry.Directory.DisplayName )
							);
					break;
				case "Time":
					highlightFilterBuilder =
						new Equals(
							new GetProperty(
								new Argument(),
								"Time" ),
							new DateTimeConstant( sourceEntry.Time )
							);
					break;
				default:
					highlightFilterBuilder = new AlwaysFalse();
					break;
			}

			if ( highlightFilterBuilder != null )
			{
				var filter = highlightFilterBuilder.BuildLogEntriesFilter();

				SparseLogEntryViewModelList entriesList = (SparseLogEntryViewModelList)sourceEntry.ParentSparseCollection;

				LogEntriesListViewModel listViewModel = entriesList.Parent;
				listViewModel.DynamicHighlightingFilter = filter;
				listViewModel.HighlightedPropertyName = bindingPath;

				listViewModel.UpdateDynamicHighlighting();
			}
		}
	}
}
