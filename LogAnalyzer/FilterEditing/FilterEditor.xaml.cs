using System.Windows;
using System.Windows.Controls;
using ExpressionBuilderSample;
using LogAnalyzer.Filters;
using System.ComponentModel;

namespace LogAnalyzer.GUI.FilterEditing
{
	/// <summary>
	/// Interaction logic for FilterEditor.xaml
	/// </summary>
	public partial class FilterEditor : UserControl
	{
		TransparentBuilder rootBuilder;

		public FilterEditor()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e )
		{
			rootBuilder = new TransparentBuilder();
			rootBuilder.PropertyChanged += OnRootBuilder_PropertyChanged;

			var vm = new ExpressionBuilderViewModel( rootBuilder, System.Linq.Expressions.Expression.Parameter( typeof( LogEntry ), "Input" ) );
			DataContext = vm;
		}

		private void OnRootBuilder_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
		}

		public ExpressionBuilder Builder
		{
			get { return rootBuilder.Inner; }
		}
	}
}
