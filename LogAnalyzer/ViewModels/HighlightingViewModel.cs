﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class HighlightingViewModel : BindingObject
	{
		internal HighlightingViewModel()
		{
			filter.Changed += new EventHandler( OnFilter_Changed );
		}

		private Brush brush;
		public Brush Brush
		{
			get { return brush; }
			set
			{
				brush = value;
				RaisePropertyChanged( "Brush" );
			}
		}

		private readonly ExpressionFilter<LogEntry> filter = new ExpressionFilter<LogEntry>();
		public ExpressionFilter<LogEntry> Filter
		{
			get { return filter; }
		}

		private void OnFilter_Changed( object sender, EventArgs e )
		{
			RaisePropertyChanged( "Description" );
		}

		public string Description
		{
			get { return filter.ExpressionBuilder.ToExpressionString(); }
		}

		private int highlightedCount = 0;
		public int HighlightedCount
		{
			get { return highlightedCount; }
			set
			{
				if ( highlightedCount == value )
					return;

				highlightedCount = value;
				RaisePropertyChanged( "HighlightedCount" );
			}
		}
	}
}