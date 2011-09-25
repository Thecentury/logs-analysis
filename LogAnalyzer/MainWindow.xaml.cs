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
using LogAnalyzer.GUI;
using LogAnalyzer.GUI.ViewModel;

namespace LogAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			// todo что-то не то с виртуализацией списков!
			Loaded += new RoutedEventHandler(MainWindow_Loaded);
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			EventManager.RegisterClassHandler(typeof(DataGridCell),
				UIElement.MouseLeftButtonDownEvent,
				new MouseButtonEventHandler(OnCellMouseLeftButtonDown), true);
		}

		private void OnCellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			// do nothing here
		}


		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			// todo простой способ все время показывать низ. Но он не работает для прокруток, инициированных пользователем
			// scrollViewer.ScrollToBottom();
		}

		private void TextBlock_Loaded(object sender, RoutedEventArgs e)
		{
			TextBlock textBlock = (TextBlock)sender;
			string text = textBlock.Text;
			if (text.Contains(Environment.NewLine))
			{
				string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
				textBlock.Inlines.Clear();

				foreach (var line in lines)
				{
					textBlock.Inlines.Add(new Run(line));
					textBlock.Inlines.Add(new Hyperlink(new Run("Open")));
					//textBlock.Inlines.Add(new InlineUIContainer(new Button { Content = "Click me!" }));
					textBlock.Inlines.Add(new LineBreak());
				}
			}
		}
	}
}
