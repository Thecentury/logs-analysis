using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using ModuleLogsProvider.Logging.Auxilliary;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class NetTcpBindingFactory : IFactory<Binding>
	{
		public Binding Create()
		{

			var binding = new NetTcpBinding
							{
								MaxReceivedMessageSize = Int32.MaxValue,
								ReaderQuotas =
									{
										MaxStringContentLength = Int32.MaxValue,
										MaxArrayLength = Int32.MaxValue,
										MaxBytesPerRead = Int32.MaxValue,
										MaxNameTableCharCount = Int32.MaxValue
									}
							};


			return binding;
		}
	}
}
