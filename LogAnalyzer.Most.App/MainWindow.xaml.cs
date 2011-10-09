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
using ModuleLogsProvider.Logging;
using ModuleLogsProvider.Logging.Auxilliary;
using ModuleLogsProvider.Logging.Most;

namespace LogAnalyzer.Most.App
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void RefreshButton_Click( object sender, RoutedEventArgs e )
		{
			var app = (App)Application.Current;

			app.Bootstrapper.Timer.Invoke();

			UpdateWorkingSet();
		}

		private void UpdateWorkingSet()
		{
			var bindingFactory = new NetTcpBindingFactory();
			var factory = new MostServiceFactory<IPerformanceInfoService>( bindingFactory, "net.tcp://127.0.0.1:9999/PerformanceService/" );
			using ( var client = factory.Create() )
			{
				double memory = client.Service.GetMemoryConsumption();
				double mbs = Math.Round( memory / 1024.0 / 1024.0 );
				workingSetTextBlock.Text = mbs.ToString();
			}
		}
	}
}
