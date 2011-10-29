using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class MenuItemViewModel : BindingObject
	{
		public string Header { get; set; }
		
		public string Tooltip { get; set; }
		
		public string IconSource { get; set; }
		
		public ICommand Command { get; set; }
	}
}
