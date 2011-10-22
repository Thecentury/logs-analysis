using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class SelectedEntryIndexStatusBarItem : BindingObject
	{
		private readonly LogEntriesListViewModel parent;
		public SelectedEntryIndexStatusBarItem( LogEntriesListViewModel parent )
			: base( parent )
		{
			this.parent = parent;
		}

		public int SelectedEntryIndex
		{
			get { return parent.SelectedEntryIndex; }
		}
	}
}
