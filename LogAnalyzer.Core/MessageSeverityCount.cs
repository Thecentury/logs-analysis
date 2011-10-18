using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer
{
	public sealed class MessageSeverityCount : INotifyPropertyChanged
	{
		private int error;
		public int Error
		{
			get { return error; }
			set
			{
				if ( error == value )
					return;

				error = value;
				PropertyChanged.Raise( this, "Error" );
			}
		}

		private int warning;
		public int Warning
		{
			get { return warning; }
			set
			{
				if ( warning == value )
					return;

				warning = value;
				PropertyChanged.Raise( this, "Warning" );
			}
		}

		private int info;
		public int Info
		{
			get { return info; }
			set
			{
				if ( info == value )
					return;

				info = value;
				PropertyChanged.Raise( this, "Info" );
			}
		}

		private int debug;
		public int Debug
		{
			get { return debug; }
			set
			{
				if ( debug == value )
					return;

				debug = value;
				PropertyChanged.Raise( this, "Debug" );
			}
		}

		private int verbose;
		public int Verbose
		{
			get { return verbose; }
			set
			{
				if ( verbose == value )
					return;

				verbose = value;
				PropertyChanged.Raise( this, "Verbose" );
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		internal void Update( IEnumerable<LogEntry> addedEntries )
		{
			Error += addedEntries.Count( le => le.Type == "E" );
			Warning += addedEntries.Count( le => le.Type == "W" );
			Info += addedEntries.Count( le => le.Type == "I" );
			Debug += addedEntries.Count( le => le.Type == "D" );
			Verbose += addedEntries.Count( le => le.Type == "V" );
		}
	}
}
