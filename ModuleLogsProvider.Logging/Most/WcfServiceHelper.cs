using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace ModuleLogsProvider.Logging.Most
{
	internal static class WcfServiceHelper
	{
		public static Binding CreateBasicHttpBinding()
		{
			return new BasicHttpBinding
					{
						MaxBufferPoolSize = Int32.MaxValue,
						MaxBufferSize = Int32.MaxValue,
						MaxReceivedMessageSize = Int32.MaxValue
					};
		}
	}
}
