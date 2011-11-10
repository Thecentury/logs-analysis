using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.ViewModels
{
	[IgnoreAllMissingProperties]
	internal sealed class LogEntryListToolbarViewModel : BindingObject
	{
		private readonly LogEntriesListViewModel parent;

		public LogEntryListToolbarViewModel([NotNull] LogEntriesListViewModel parent )
			: base( parent )
		{
			if (parent == null) throw new ArgumentNullException("parent");
			this.parent = parent;
		}

		public ICommand ScrollToBottomCommand
		{
			get { return parent.ScrollToBottomCommand; }
		}

		public ICommand ScrollToTopCommand
		{
			get { return parent.ScrollToTopCommand; }
		}

		public bool AutoScrollToBottom
		{
			get { return parent.AutoScrollToBottom; }
			set { parent.AutoScrollToBottom = value; }
		}
	}
}
