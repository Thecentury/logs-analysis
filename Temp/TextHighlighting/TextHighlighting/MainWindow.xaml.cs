﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextHighlighting
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		void MainWindow_Loaded( object sender, RoutedEventArgs e )
		{
			var start = textBlock1.Document.ContentStart;
			var textrange = new TextRange( start.GetPositionAtOffset( 0, LogicalDirection.Forward ), start.GetPositionAtOffset( 10 ) );
			textrange.ApplyPropertyValue( TextElement.BackgroundProperty, Brushes.LightGreen );
		}
	}
}
