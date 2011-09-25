using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Filters
{
	/// <summary>
	/// Описывает объект, предоставляющий информацию о допустимом типе своего свойства по его имени.
	/// </summary>
	public interface IOverridePropertyTypeInfo
	{
		Type GetPropertyType( string propertyName );
	}
}
