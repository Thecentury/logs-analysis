using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.GUI.OverviewGui;
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

		private void OnOverviewItemLeftMouseButtonDown( object sender, MouseEventArgs e )
		{
			FrameworkElement fe = (FrameworkElement)sender;
			OverviewInfo info = (OverviewInfo)fe.DataContext;
			info.ScrollToItemCommand.Execute();
		}

		private void LogEntriesListView_Loaded( object sender, RoutedEventArgs e )
		{
			var border = VisualTreeHelper.GetChild( entriesDataGrid, 0 );
			ScrollViewer viewer = VisualTreeHelper.GetChild( border, 0 ) as ScrollViewer;
			LogEntriesListViewModel vm = (LogEntriesListViewModel)DataContext;
			vm.ScrollViewer = viewer;
			vm.DataGrid = entriesDataGrid;
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
