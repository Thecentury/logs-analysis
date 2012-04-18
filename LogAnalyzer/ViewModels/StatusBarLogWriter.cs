using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;
using LogAnalyzer.Logging;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class StatusBarLogWriter : LogWriter, INotifyPropertyChanged
	{
		private string _message;
		public string Message
		{
			get { return _message; }
			private set
			{
				_message = value;
				PropertyChanged.Raise( this, "Message" );
			}
		}

		public override void WriteLine( string message, MessageType messageType )
		{
			if ( messageType == MessageType.Error || messageType == MessageType.Warning || messageType == MessageType.Info )
			{
				Message = message;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
