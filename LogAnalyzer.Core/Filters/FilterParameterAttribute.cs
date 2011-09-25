using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	[AttributeUsage( AttributeTargets.Property, Inherited = false, AllowMultiple = false )]
	public sealed class FilterParameterAttribute : Attribute
	{
		public FilterParameterAttribute( Type parameterType, string parameterName )
		{
			this.ParameterType = parameterType;
			this.ParameterName = parameterName;
		}

		public Type ParameterType { get; private set; }
		public string ParameterName { get; private set; }

		public Type ParameterReturnType { get; set; }
	}
}
