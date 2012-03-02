﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Collections;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public abstract class OverviewViewModelBase : BindingObject
	{
		public abstract ObservableList<LogEntry> Items { get; }
	}
}