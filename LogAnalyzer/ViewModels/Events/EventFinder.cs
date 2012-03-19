using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels
{
	public class EventFinder
	{
	}

	public sealed class ExceptionEvent : Event
	{
		public ExceptionEvent( [NotNull] LogEntry logEntry )
			: base( logEntry )
		{
		}
	}

	public abstract class Event
	{
		protected Event( [NotNull] LogEntry logEntry )
		{
			if ( logEntry == null )
			{
				throw new ArgumentNullException( "logEntry" );
			}
			_logEntry = logEntry;
		}

		private readonly LogEntry _logEntry;
		public LogEntry LogEntry
		{
			get { return _logEntry; }
		}
	}
}
