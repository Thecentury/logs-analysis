using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;

namespace ModuleLogsProvider.Tests
{
	/// <summary>
	/// Assert с особенностями: если он проваливается, то автоматически выдается осмысленный message.
	/// </summary>
	public sealed class ExpressionAssert
	{
		public static void That<T>( T obj, Expression<Func<T, bool>> expression )
		{
			var compiled = expression.Compile();
			bool success = compiled( obj );

			if ( !success )
			{
				string typeName = typeof( T ).Name;

				string message = String.Format( "{0}: {1} failed.", typeName, expression.Body.ToString() );
				Assert.Fail( message );
			}
		}
	}

	public static class ExpressionAssertExtensions
	{
		/// <summary>
		/// Выполнить проверку некоторого выражения на истинность.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="obj"></param>
		/// <param name="expression"></param>
		public static void Assert<T>( this T obj, Expression<Func<T, bool>> expression )
		{
			ExpressionAssert.That( obj, expression );
		}
	}

}
