using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LogAnalyzer.Auxilliary;
using LogAnalyzer.Extensions;
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.GUI.OverviewGui;
using LogAnalyzer.GUI.ViewModels;
using LogAnalyzer.Logging;
using Microsoft.Research.DynamicDataDisplay.ViewportRestrictions;

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

			plotter.Viewport.Restrictions.Add( new DataHeightRestriction() );

			Logger.Instance.WriteInfo( "LogEntriesListView.ctor()" );

			Loaded += OnLoaded;
		}

		protected override void OnKeyDown( KeyEventArgs e )
		{
			if ( e.Key.HasFlag( Key.F ) && (Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl )) )
			{
				searchControl.Focus();
				e.Handled = true;
			}

			base.OnKeyDown( e );
		}

		private void OnLoaded( object sender, RoutedEventArgs e )
		{
			var border = VisualTreeHelper.GetChild( entriesDataGrid, 0 );
			ScrollViewer viewer = VisualTreeHelper.GetChild( border, 0 ) as ScrollViewer;
			LogEntriesListViewModel vm = DataContext as LogEntriesListViewModel;
			if ( vm != null )
			{
				vm.ScrollViewer = viewer;
				vm.DataGrid = entriesDataGrid;
			}

			Logger.Instance.WriteInfo( "LogEntriesListView was loaded" );
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
