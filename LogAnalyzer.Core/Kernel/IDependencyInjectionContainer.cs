using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public interface IDependencyInjectionContainer
	{
		void Register<T>(Func<object> createImplementationFunc);
		T Resolve<T>();
		bool CanResolve<T>();
	}
}
