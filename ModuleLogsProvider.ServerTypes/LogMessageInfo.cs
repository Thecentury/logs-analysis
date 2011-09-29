using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Awad.Eticket.ModuleLogsProvider.Types
{
	// todo brinchuk doucment me
	[DataContract]
	public sealed class LogMessageInfo
	{
		[DataMember]
		public string LoggerName { get; set; }
		[DataMember]
		public string Message { get; set; }
		[DataMember]
		public string MessageType { get; set; }

		[DataMember]
		public int IndexInAllMessagesList { get; set; }
	}
}
