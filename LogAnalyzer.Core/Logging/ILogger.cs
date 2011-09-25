using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Logging
{
	public interface ILogger
	{
		void Write( string message, MessageType messageType = MessageType.Info );
	}
}
