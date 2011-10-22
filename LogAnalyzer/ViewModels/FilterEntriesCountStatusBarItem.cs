using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class FilterEntriesCountStatusBarItem : EntriesCountStatusBarItem
	{
		private readonly FilterViewModel parent;
		public FilterEntriesCountStatusBarItem( FilterViewModel parent ) : base( parent )
		{
			this.parent = parent;
		}

		public int SourceCount
		{
			get { return parent.SourceCount; }
		}
	}
}
