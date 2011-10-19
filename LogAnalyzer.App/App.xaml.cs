using System.Diagnostics;
using System.Windows;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.App
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private readonly AppBootstrapper bootstrapper = new AppBootstrapper();

		protected override void OnStartup( StartupEventArgs e )
		{
			Debugger.Launch();
			base.OnStartup( e );

			bootstrapper.Start( e.Args );
		}

		protected override void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			bootstrapper.Logger.WriteInfo( "Exiting" );
		}
	}
}
