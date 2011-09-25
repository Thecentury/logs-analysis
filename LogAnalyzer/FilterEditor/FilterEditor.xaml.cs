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
using LogAnalyzer.Filters;
using System.ComponentModel;
using LogAnalyzer;

namespace ExpressionBuilderSample
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
