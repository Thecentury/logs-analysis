using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	[AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
	public sealed class IgnoreBuilderAttribute : Attribute
	{
		public IgnoreBuilderAttribute() { }
	}
}
