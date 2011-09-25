using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	public interface IFilter<in T>
	{
		bool Include( T entity );
		event EventHandler Changed;
	}
}
