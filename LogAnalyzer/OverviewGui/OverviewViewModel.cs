using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class OverviewViewModel : BindingObject
	{
		private readonly ObservableList<LogEntry> itemsList = new ObservableList<LogEntry>( );
		public ObservableList<LogEntry> Items
		{
			get { return itemsList; }
		}
	}
}
