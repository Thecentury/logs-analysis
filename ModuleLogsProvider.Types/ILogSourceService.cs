using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Awad.Eticket.ModuleLogsProvider.Types
{
	// todo brinchuk document me
	[ServiceContract]
	public interface ILogSourceService
	{
		[OperationContract]
		void ClearMessagesList();

		[OperationContract]
		void StartListening();

		[OperationContract]
		void StopListening();

		[OperationContract]
		bool GetIsListening();

		[OperationContract]
		LogMessageInfo[] GetMessages( int startingIndex );

		// todo brinchuk add GetCount method
	}
}
