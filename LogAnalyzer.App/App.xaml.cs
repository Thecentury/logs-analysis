using System.Diagnostics;
using System.Windows;
using LogAnalyzer.Extensions;
using LogAnalyzer.Logging;

namespace LogAnalyzer.App
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		private readonly AppBootstrapper _bootstrapper = new AppBootstrapper();

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			_bootstrapper.Start( e.Args );
		}

		protected override void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			_bootstrapper.Logger.WriteInfo( "Exiting" );
		}
	}
}
