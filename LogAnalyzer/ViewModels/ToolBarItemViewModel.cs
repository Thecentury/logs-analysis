using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class ToolBarItemViewModel : BindingObject
	{
		public string Tooltip { get; set; }

		public ICommand Command { get; set; }

		public string IconSource { get; set; }
	}
}
