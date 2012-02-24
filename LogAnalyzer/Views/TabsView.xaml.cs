using System.IO;
using System.Windows.Controls;
using AvalonDockMVVM;

namespace LogAnalyzer.GUI.Views
{
	/// <summary>
	/// Interaction logic for TabsView.xaml
	/// </summary>
	public partial class TabsView : UserControl
	{
		public TabsView()
		{
			InitializeComponent();
		}

		private void AvalonDockHost_AvalonDockLoaded( object sender, System.EventArgs e )
		{
			string layout =
			"<DockingManager version=\"1.3.0\">" +
				"<ResizingPanel ResizeWidth=\"*\" ResizeHeight=\"*\" EffectiveSize=\"0,0\" Orientation=\"Horizontal\">" +
					"<DocumentPane IsMain=\"true\" ResizeWidth=\"*\" ResizeHeight=\"*\" />" +
				"</ResizingPanel>" +
				"<Hidden />" +
				"<Windows />" +
			"</DockingManager>";

			AvalonDockHost host = (AvalonDockHost)sender;

			host.DockingManager.RestoreLayout( new StringReader( layout ) );
		}
	}
}
