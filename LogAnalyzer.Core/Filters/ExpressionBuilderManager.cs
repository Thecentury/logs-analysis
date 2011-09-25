using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace LogAnalyzer.Filters
{
	public sealed class ExpressionBuilderManager
	{
		private static readonly Type[] expressionBuilderTypes = null;
		static ExpressionBuilderManager()
		{
			expressionBuilderTypes = GetAllExpressionBuilderTypes();
		}

		private static Type[] GetAllExpressionBuilderTypes()
		{
			Type builderType = typeof( ExpressionBuilder );

			var types = from type in builderType.Assembly.GetTypes()
						where !type.IsAbstract
						where builderType.IsAssignableFrom( type )
						let ignoreAttributes = type.GetCustomAttributes( typeof( IgnoreBuilderAttribute ), inherit: false )
						where ignoreAttributes.Length == 0
						select type;

			return types.ToArray();
		}

		public static ExpressionBuilder[] GetBuildersReturningType( Type desiredType )
		{
			if ( desiredType == null )
				throw new ArgumentNullException( "desiredType" );

			ParameterExpression target = Expression.Parameter( typeof( LogEntry ) );

			var result = (from builderType in expressionBuilderTypes
						  let builder = CreateExpressionBuilder( builderType, desiredType )
						  let resultType = builder.GetResultType( target )
						  where IsAppropriateType( desiredType, resultType )
						  select builder)
					.ToArray();

			return result;
		}

		private static ExpressionBuilder CreateExpressionBuilder( Type builderType, Type desiredType )
		{
			ExpressionBuilder result = null;
			Type actualType = builderType;
			if ( builderType.ContainsGenericParameters )
			{
				actualType = builderType.MakeGenericType( desiredType );
			}

			result = (ExpressionBuilder)Activator.CreateInstance( actualType );
			return result;
		}

		private static bool IsAppropriateType( Type desiredType, Type actualType )
		{
			bool result = actualType == desiredType || actualType.IsSubclassOf( desiredType ) || actualType == typeof( object );
			return result;
		}
	}
}
