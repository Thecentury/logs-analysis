using System;
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
		private readonly TransparentBuilder _rootBuilder = new TransparentBuilder();
		private ExpressionBuilderViewModel _vm;

		public FilterEditor()
		{
			InitializeComponent();
			_rootBuilder.PropertyChanged += OnRootBuilderPropertyChanged;
		}

		private void OnRootBuilderPropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			SelectedBuilder = Builder;
		}

		private void UserControlLoaded( object sender, RoutedEventArgs e )
		{
			var parameterExpression = System.Linq.Expressions.Expression.Parameter( InputType, "Input" );
			_context = new BuilderContext( _rootBuilder, parameterExpression );
			_vm = new ExpressionBuilderViewModel( _context );
			if ( Builder != null )
			{
				_vm.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( _context.WithBuilder( Builder ) );
			}
			DataContext = _vm;
		}

		public ExpressionBuilder Builder
		{
			get { return _rootBuilder.Inner; }
			set
			{
				_rootBuilder.Inner = value;
				SelectedBuilder = value;
			}
		}

		#region InputType dependency property

		public Type InputType
		{
			get { return (Type)GetValue( InputTypeProperty ); }
			set { SetValue( InputTypeProperty, value ); }
		}

		public static readonly DependencyProperty InputTypeProperty = DependencyProperty.Register(
		  "InputType",
		  typeof( Type ),
		  typeof( FilterEditor ),
		  new FrameworkPropertyMetadata( typeof(LogEntry) ) );

		#endregion

		#region SelectedBuilder dependency property

		public ExpressionBuilder SelectedBuilder
		{
			get { return (ExpressionBuilder)GetValue( SelectedBuilderProperty ); }
			set { SetValue( SelectedBuilderProperty, value ); }
		}

		public static readonly DependencyProperty SelectedBuilderProperty = DependencyProperty.Register(
		  "SelectedBuilder",
		  typeof( ExpressionBuilder ),
		  typeof( FilterEditor ),
		  new FrameworkPropertyMetadata( null, OnSelectedBuilderChanged ) );

		private BuilderContext _context;

		private static void OnSelectedBuilderChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			FilterEditor editor = (FilterEditor)d;
			var newBuilder = (ExpressionBuilder)e.NewValue;
			if ( editor.Builder != newBuilder )
			{
				editor.Builder = newBuilder;
				if ( editor._vm != null )
				{
					editor._vm.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( editor._context.WithBuilder( newBuilder ) );
				}
			}
		}

		#endregion
	}
}
