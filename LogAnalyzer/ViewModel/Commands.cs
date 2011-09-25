using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.ViewModel
{
	public static class Commands
	{
		static Commands()
		{
			cancelOperationCommand = new RoutedUICommand( "CancelOperation", "CancelOperation", typeof( Commands ) );
			cancelOperationCommand.InputGestures.Add( new KeyGesture( Key.Escape ) );
		}

		private static readonly RoutedUICommand cancelOperationCommand = null;
		public static RoutedUICommand CancelOperation
		{
			get { return cancelOperationCommand; }
		}
	}
}
