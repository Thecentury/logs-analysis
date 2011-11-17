using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.Extensions
{
	public static class ICommandExtensions
	{
		[DebuggerStepThrough]
		public static void Execute( this ICommand command )
		{
			command.Execute( null );
		}

		[DebuggerStepThrough]
		public static bool CanExecute( this ICommand command )
		{
			return command.CanExecute( null );
		}

		[DebuggerStepThrough]
		public static void ExecuteIfCan(this ICommand command)
		{
			if ( command.CanExecute() )
			{
				command.Execute();
			}
		}
	}
}
