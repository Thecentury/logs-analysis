using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ModuleLogsProvider.Interfaces
{
	[ServiceContract]
	public interface ILogSinkService
	{
		[OperationContract]
		void WriteError( string message );

		[OperationContract]
		void WriteInfo( string message );
	}
}
