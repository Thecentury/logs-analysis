using System.Diagnostics;
using System.Windows;
using System.Windows.Shell;
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
			var jumpList = JumpList.GetJumpList( this );
			if ( jumpList == null )
			{
				jumpList = new JumpList();
				JumpList.SetJumpList( this, jumpList );
			}
			jumpList.ShowRecentCategory = true;

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
