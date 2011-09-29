using System.Windows;
using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.Views
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
