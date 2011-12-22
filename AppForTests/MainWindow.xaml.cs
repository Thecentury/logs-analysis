using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AppForTests
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DispatcherTimer timer;

		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindowLoaded;

			timer = new DispatcherTimer( TimeSpan.FromMilliseconds( 100 ), DispatcherPriority.Background, OnTick, Dispatcher.CurrentDispatcher );
		}

		void MainWindowLoaded( object sender, RoutedEventArgs e )
		{
			grid.ItemsSource = collection;
			var border = VisualTreeHelper.GetChild( grid, 0 );
			viewer = VisualTreeHelper.GetChild( border, 0 ) as ScrollViewer;
			collection.CollectionChanged += collection_CollectionChanged;
		}

		void collection_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
		}

		private readonly ObservableCollection<Data> collection = new ObservableCollection<Data>();
		private ScrollViewer viewer;

		void OnTick( object sender, EventArgs args )
		{
			for ( int i = 0; i < 10; i++ )
			{
				collection.Add( new Data { String = collection.Count.ToString() } );
			}

			viewer.ScrollToEnd();
		}
	}

	public class Data
	{
		public string String { get; set; }
	}
}
