using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class StatusBarLogWriter : LogWriter, INotifyPropertyChanged
	{
		private string message;
		public string Message
		{
			get { return message; }
			private set
			{
				message = value;
				PropertyChanged.Raise( this, "Message" );
			}
		}

		public override void WriteLine( string message )
		{
			Message = message;
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
