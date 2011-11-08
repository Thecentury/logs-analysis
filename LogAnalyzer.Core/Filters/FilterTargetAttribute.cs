using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.Filters
{
	/// <summary>
	/// Указывает, параметры какого типа ожидаются построителем фильтров.
	/// </summary>
	[AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
	public sealed class FilterTargetAttribute : Attribute
	{
		private readonly Type targetType;

		public FilterTargetAttribute( [NotNull] Type targetType )
		{
			if ( targetType == null ) throw new ArgumentNullException( "targetType" );
			this.targetType = targetType;
		}

		/// <summary>
		/// Тип ожидаемого параметра.
		/// </summary>
		public Type TargetType
		{
			get { return targetType; }
		}
	}
}
