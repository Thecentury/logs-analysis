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
			rootBuilder.PropertyChanged += OnRootBuilder_PropertyChanged;
		}

		private void OnRootBuilder_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			CoerceValue( SelectedBuilderProperty );
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

		public ExpressionBuilder SelectedBuilder
		{
			get { return (ExpressionBuilder)GetValue( SelectedBuilderProperty ); }
			set { SetValue( SelectedBuilderProperty, value ); }
		}

		public static readonly DependencyProperty SelectedBuilderProperty = DependencyProperty.Register(
		  "SelectedBuilder",
		  typeof( ExpressionBuilder ),
		  typeof( FilterEditor ),
		  new FrameworkPropertyMetadata( null, OnSelectedBuilderChanged, CoerceSelectedBuilder ) );

		private static void OnSelectedBuilderChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{

		}

		private static object CoerceSelectedBuilder( DependencyObject d, object baseValue )
		{
			FilterEditor editor = (FilterEditor)d;
			return editor.rootBuilder.Inner;
		}
	}
}
