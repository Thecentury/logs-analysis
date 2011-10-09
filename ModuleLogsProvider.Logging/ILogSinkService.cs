using System.ServiceModel;

namespace ModuleLogsProvider.Logging
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

