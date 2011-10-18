using System.Windows.Input;

namespace LogAnalyzer.GUI.ViewModels
{
	public static class Commands
	{
		static Commands()
		{
			cancelOperationCommand = new RoutedUICommand( "CancelOperation", "CancelOperation", typeof( Commands ) );
			cancelOperationCommand.InputGestures.Add( new KeyGesture( Key.Escape ) );
		}

		private static readonly RoutedUICommand cancelOperationCommand;
		public static RoutedUICommand CancelOperation
		{
			get { return cancelOperationCommand; }
		}
	}
}
