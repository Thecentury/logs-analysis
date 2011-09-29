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
using System.Windows.Shapes;
using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.View
{
	/// <summary>
	/// Interaction logic for FilterEditorWindow.xaml
	/// </summary>
	public partial class FilterEditorWindow : Window
	{
		public FilterEditorWindow()
		{
			InitializeComponent();
		}

		public FilterEditorWindow( Window owner )
			: this()
		{
			Owner = owner;
		}

		public ExpressionBuilder Builder
		{
			get { return filterEditor.Builder; }
		}
	}
}
