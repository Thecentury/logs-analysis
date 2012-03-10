using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.ViewModels;

namespace LogAnalyzer.GUI.OverviewGui
{
	public sealed class OverviewInfo : BindingObject
	{
		private readonly LogEntriesListViewModel _parent;

		public OverviewInfo( [NotNull] LogEntry logEntry, double coordinate, [NotNull] LogEntriesListViewModel parent )
		{
			if ( logEntry == null ) throw new ArgumentNullException( "logEntry" );
			if ( parent == null ) throw new ArgumentNullException( "parent" );

			this._parent = parent;

			LogEntry = logEntry;
			Coordinate = coordinate;
		}

		public LogEntry LogEntry { get; private set; }

		public double Coordinate { get; private set; }

		private DelegateCommand _scrollToItemCommand;
		public ICommand ScrollToItemCommand
		{
			get
			{
				if ( _scrollToItemCommand == null )
				{
					_scrollToItemCommand = new DelegateCommand( () => _parent.ScrollToItemCommand.Execute( LogEntry ) );
				}

				return _scrollToItemCommand;
			}
		}
	}
}
