using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public interface ITransformer<T>
	{
		T Transform( T obj );
	}
}
