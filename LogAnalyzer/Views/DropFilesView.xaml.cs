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
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.Views
{
	/// <summary>
	/// Interaction logic for DropFilesView.xaml
	/// </summary>
	public partial class DropFilesView : UserControl
	{
		public DropFilesView()
		{
			InitializeComponent();
		}

		protected override void OnDragEnter( DragEventArgs e )
		{
			base.OnDragEnter( e );
		}

		protected override void OnGiveFeedback( GiveFeedbackEventArgs e )
		{
			base.OnGiveFeedback( e );
		}

		protected override void OnDrop( DragEventArgs e )
		{
			base.OnDrop( e );
			
			DropFilesViewModel vm = (DropFilesViewModel)DataContext;
			vm.DropCommand.Execute( e.Data );

			e.Handled = true;
		}
	}
}
