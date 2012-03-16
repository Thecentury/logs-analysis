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
using LogAnalyzer.GUI.Extensions;
using LogAnalyzer.GUI.OverviewGui;

namespace LogAnalyzer.GUI.Views
{
	/// <summary>
	/// Interaction logic for OverviewControl.xaml
	/// </summary>
	public partial class OverviewControl : UserControl
	{
		public OverviewControl()
		{
			InitializeComponent();
		}

		private void OnOverviewItemLeftMouseButtonDown( object sender, MouseEventArgs e )
		{
			FrameworkElement fe = (FrameworkElement)sender;
			OverviewInfo info = fe.DataContext as OverviewInfo;

			if ( info != null )
			{
				info.ScrollToItemCommand.Execute();
			}
		}
	}
}
