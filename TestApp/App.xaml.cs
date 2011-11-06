using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TestApp
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private readonly TestBootstrapper bootstrapper = new TestBootstrapper();
		public TestBootstrapper Bootstrapper
		{
			get { return bootstrapper; }
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			bootstrapper.Start( e.Args );
		}
	}
}
