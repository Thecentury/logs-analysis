using System;
using System.Collections.Generic;
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

namespace AppForTests
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += new RoutedEventHandler( MainWindow_Loaded );
		}

		void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			IDictionary<string, object> o = new ExpandoObject();
			o["p1"] = "p1";
			o["p2"] = "p2";

			DataContext = o;

			var m = Regex.Match("123", @"(?<a>\d)");
		}
	}

}
