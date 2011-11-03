using System.Windows;
using System.Windows.Controls;
using LogAnalyzer.Filters;
using System.ComponentModel;

namespace LogAnalyzer.GUI.FilterEditing
{
	/// <summary>
	/// Interaction logic for FilterEditor.xaml
	/// </summary>
	public partial class FilterEditor : UserControl
	{
		private readonly TransparentBuilder rootBuilder = new TransparentBuilder();

		public FilterEditor()
		{
			InitializeComponent();
		}

		private void UserControl_Loaded( object sender, RoutedEventArgs e )
		{
			var parameterExpression = System.Linq.Expressions.Expression.Parameter( typeof( LogEntry ), "Input" );
			var vm = new ExpressionBuilderViewModel( rootBuilder, parameterExpression );
			if ( Builder != null )
			{
				vm.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( Builder, parameterExpression );
			}
			DataContext = vm;
		}

		public ExpressionBuilder Builder
		{
			get { return rootBuilder.Inner; }
			set { rootBuilder.Inner = value; }
		}
	}
}
