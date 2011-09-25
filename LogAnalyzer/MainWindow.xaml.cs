using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.ViewModel;

namespace LogAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// todo что-то не то с виртуализацией списков!
			Loaded += new RoutedEventHandler( MainWindow_Loaded );
		}

		private void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			EventManager.RegisterClassHandler( typeof( DataGridCell ),
				UIElement.MouseLeftButtonDownEvent,
				new MouseButtonEventHandler( OnCellMouseLeftButtonDown ), true );
		}

		private void OnCellMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			// do nothing here
		}
	}
}
