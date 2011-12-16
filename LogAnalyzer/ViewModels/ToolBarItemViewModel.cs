using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class ToolBarItemViewModel : BindingObject
	{
		public ToolBarItemViewModel() { }

		public ToolBarItemViewModel( [NotNull] string tooltip, [NotNull] ICommand command, [NotNull] string iconSource )
		{
			if ( tooltip == null ) throw new ArgumentNullException( "tooltip" );
			if ( command == null ) throw new ArgumentNullException( "command" );
			if ( iconSource == null ) throw new ArgumentNullException( "iconSource" );

			Tooltip = tooltip;
			Command = command;
			IconSource = iconSource;
		}

		public string Tooltip { get; set; }

		public ICommand Command { get; set; }

		public string IconSource { get; set; }
	}
}
