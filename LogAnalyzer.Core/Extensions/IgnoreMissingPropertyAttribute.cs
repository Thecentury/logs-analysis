using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Extensions
{
	[AttributeUsage( AttributeTargets.Class, Inherited = true, AllowMultiple = true )]
	public sealed class IgnoreMissingPropertyAttribute : Attribute
	{
		public IgnoreMissingPropertyAttribute( string propertyName )
		{
			this.PropertyName = propertyName;
		}

		public string PropertyName { get; private set; }
	}

	[AttributeUsage( AttributeTargets.Class, Inherited = true, AllowMultiple = true )]
	public sealed class IgnoreAllMissingPropertiesAttribute : Attribute
	{
	}
}
