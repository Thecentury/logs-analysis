using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LogAnalyzer.GUI.Common
{
	public class AttachedCommandBindingCollection : FreezableCollection<AttachedCommandBinding>
	{
		public DependencyObject Owner { get; set; }
	}
}
