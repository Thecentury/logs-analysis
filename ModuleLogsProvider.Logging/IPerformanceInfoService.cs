using System.ServiceModel;

namespace ModuleLogsProvider.Logging
{
	[ServiceContract]
	public interface IPerformanceInfoService
	{
		[OperationContract]
		double GetCPULoad();

		[OperationContract]
		double GetMemoryConsumption();
	}
}
