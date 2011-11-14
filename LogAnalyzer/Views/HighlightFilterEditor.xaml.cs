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

namespace LogAnalyzer.GUI.Views
{
	/// <summary>
	/// Interaction logic for HighlightFilterEditor.xaml
	/// </summary>
	public partial class HighlightFilterEditor : UserControl
	{
		public HighlightFilterEditor()
		{
			InitializeComponent();
		}

		// Selected color

		public Color SelectedColor
		{
			get { return (Color)GetValue( SelectedColorProperty ); }
			set { SetValue( SelectedColorProperty, value ); }
		}

		public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
		  "SelectedColor",
		  typeof( Color ),
		  typeof( HighlightFilterEditor ),
		  new FrameworkPropertyMetadata( Colors.LightBlue ) );

		// Selected filter

		public ExpressionBuilder SelectedBuilder
		{
			get { return (ExpressionBuilder)GetValue( SelectedBuilderProperty ); }
			set { SetValue( SelectedBuilderProperty, value ); }
		}

		public static readonly DependencyProperty SelectedBuilderProperty = DependencyProperty.Register(
		  "SelectedBuilder",
		  typeof( ExpressionBuilder ),
		  typeof( HighlightFilterEditor ),
		  new FrameworkPropertyMetadata( null ) );
	}
}
