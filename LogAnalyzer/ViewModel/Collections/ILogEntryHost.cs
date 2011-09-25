using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModel
{
	internal interface ILogEntryHost
	{
		/// <summary>
		/// Вернуть LogEntryViewModel в пул.
		/// </summary>
		/// <param name="vm"></param>
		void Release( LogEntryViewModel vm );

		event EventHandler<LogEntryHostChangedEventArgs> ItemCreated;
		event EventHandler<LogEntryHostChangedEventArgs> ItemRemoved;
	}

	public class LogEntryHostChangedEventArgs : EventArgs
	{
		internal LogEntryHostChangedEventArgs( LogEntryViewModel logEntryViewModel )
		{
			if ( logEntryViewModel == null )
				throw new ArgumentNullException( "viewModel" );

			this.LogEntryViewModel = logEntryViewModel;
		}

		public LogEntryViewModel LogEntryViewModel { get; private set; }
	}
}
