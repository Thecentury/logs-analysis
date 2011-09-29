using System;

namespace LogAnalyzer.GUI.ViewModels.Collections
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
				throw new ArgumentNullException( "logEntryViewModel" );

			this.LogEntryViewModel = logEntryViewModel;
		}

		public LogEntryViewModel LogEntryViewModel { get; private set; }
	}
}
