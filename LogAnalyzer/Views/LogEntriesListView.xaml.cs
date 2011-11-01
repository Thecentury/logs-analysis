using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.Views
{
	/// <summary>
	/// Interaction logic for LogEntriesListView.xaml
	/// </summary>
	public partial class LogEntriesListView : UserControl
	{
		public LogEntriesListView()
		{
			InitializeComponent();
		}

		private void DataGrid_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			LogEntriesListViewModel vm = e.NewValue as LogEntriesListViewModel;
			if ( vm != null )
			{
				directoryColumn.Visibility = vm.DirectoriesColumnVisibility;
			}

			entriesDataGrid.SelectionChanged += OnEntriesDataGrid_SelectionChanged;
		}

		private void OnEntriesDataGrid_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( e.AddedItems.Count > 0 )
			{
				var firstSelected = e.AddedItems[0];
				if ( firstSelected != null )
				{
					entriesDataGrid.ScrollIntoView( firstSelected );
				}
			}
		}
	}
}
