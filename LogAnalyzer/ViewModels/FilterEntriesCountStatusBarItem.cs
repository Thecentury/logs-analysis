using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.ViewModels
{
	[IgnoreAllMissingProperties]
	public sealed class FilterEntriesCountStatusBarItem : EntriesCountStatusBarItem
	{
		private readonly FilterTabViewModel parent;
		public FilterEntriesCountStatusBarItem( FilterTabViewModel parent ) : base( parent )
		{
			this.parent = parent;
		}

		public int SourceCount
		{
			get { return parent.SourceCount; }
		}
	}
}
