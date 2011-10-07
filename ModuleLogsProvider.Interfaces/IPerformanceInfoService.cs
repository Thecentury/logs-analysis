using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ModuleLogsProvider.Interfaces
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
