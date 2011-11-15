using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.Extensions
{
	public static class ICommandExtensions
	{
		public static void Execute( this ICommand command )
		{
			command.Execute( null );
		}

		public static bool CanExecute( this ICommand command )
		{
			return command.CanExecute( null );
		}
	}
}
