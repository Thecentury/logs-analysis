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
using LogAnalyzer.GUI.ViewModels.Collections;

namespace LogAnalyzer.GUI.ViewModels
{
	internal class DynamicHighlightManager
	{
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

			ExpressionBuilder highlightFilterBuilder;
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

			var filter = highlightFilterBuilder.BuildLogEntriesFilter();

			SparseLogEntryViewModelList entriesList = (SparseLogEntryViewModelList)sourceEntry.ParentSparseCollection;

			LogEntriesListViewModel listViewModel = entriesList.Parent;
			listViewModel.DynamicHighlightingFilter = filter;
			listViewModel.HighlightedPropertyName = bindingPath;

			listViewModel.UpdateDynamicHighlighting();
		}
	}
}
