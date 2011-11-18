using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels.Collections
{
	internal sealed class LogEntryViewModelLight : IAwareOfIndex
	{
		private readonly int indexInParentCollection;

		public LogEntryViewModelLight(int indexInParentCollection)
		{
			this.indexInParentCollection = indexInParentCollection;
		}

		public int IndexInParentCollection
		{
			get { return indexInParentCollection; }
		}
	}
}
