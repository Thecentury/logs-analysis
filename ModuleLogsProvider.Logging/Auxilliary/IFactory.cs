using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModuleLogsProvider.Logging
{
	public interface IFactory<out T>
	{
		T CreateObject();
	}
}
