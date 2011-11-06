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
			Loaded += LogEntriesListView_Loaded;
		}

		private void LogEntriesListView_Loaded( object sender, RoutedEventArgs e )
		{
			var border = VisualTreeHelper.GetChild( entriesDataGrid, 0 );
			ScrollViewer viewer = VisualTreeHelper.GetChild( border, 0 ) as ScrollViewer;
			LogEntriesListViewModel vm = (LogEntriesListViewModel)DataContext;
			vm.ScrollViewer = viewer;
		}

		private void DataGrid_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			LogEntriesListViewModel vm = e.NewValue as LogEntriesListViewModel;
			if ( vm != null )
			{
				directoryColumn.Visibility = vm.DirectoriesColumnVisibility;
			}
		}

		private void OnEntriesDataGrid_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			//if ( e.AddedItems.Count > 0 )
			//{
			//    var firstSelected = e.AddedItems[0];
			//    if ( firstSelected != null )
			//    {
			//        entriesDataGrid.ScrollIntoView( firstSelected );
			//    }
			//}
		}
	}
}
