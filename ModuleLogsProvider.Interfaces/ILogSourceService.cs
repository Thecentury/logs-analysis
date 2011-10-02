using System.ServiceModel;
using Awad.Eticket.ModuleLogsProvider.Types;

namespace ModuleLogsProvider.Interfaces
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
