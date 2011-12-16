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
			Loaded += OnLoaded;
		}

		private void OnOverviewItemLeftMouseButtonDown( object sender, MouseEventArgs e )
		{
			FrameworkElement fe = (FrameworkElement)sender;
			OverviewInfo info = (OverviewInfo)fe.DataContext;
			info.ScrollToItemCommand.Execute();
		}

		private void OnLoaded( object sender, RoutedEventArgs e )
		{
			var border = VisualTreeHelper.GetChild( entriesDataGrid, 0 );
			ScrollViewer viewer = VisualTreeHelper.GetChild( border, 0 ) as ScrollViewer;
			LogEntriesListViewModel vm = (LogEntriesListViewModel)DataContext;
			vm.ScrollViewer = viewer;

			vm.DataGrid = entriesDataGrid;
		}

		private void DataGridDataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			LogEntriesListViewModel vm = e.NewValue as LogEntriesListViewModel;
			if ( vm != null )
			{
				directoryColumn.Visibility = vm.DirectoriesColumnVisibility;
			}
		}
	}
}
