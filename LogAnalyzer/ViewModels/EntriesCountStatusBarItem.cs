using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	public class EntriesCountStatusBarItem : BindingObject, ITypeName
	{
		private readonly LogEntriesListViewModel parent;
		internal EntriesCountStatusBarItem( LogEntriesListViewModel parent )
			: base( parent )
		{
			this.parent = parent;
		}

		public int TotalEntries
		{
			get { return parent.TotalEntries; }
		}

		public string Type
		{
			get { return GetType().Name; }
		}
	}
}
