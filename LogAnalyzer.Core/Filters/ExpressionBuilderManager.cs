using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace LogAnalyzer.Filters
{
	public static class ExpressionBuilderManager
	{
		private static readonly Type[] expressionBuilderTypes;
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

		public static ExpressionBuilder[] GetBuilders( Type returnType, Type inputType )
		{
			if ( returnType == null )
			{
				throw new ArgumentNullException( "returnType" );
			}

			ParameterExpression target = Expression.Parameter( inputType );

			var result = (from builderType in expressionBuilderTypes
						  let builder = CreateExpressionBuilder( builderType, returnType )
						  let resultType = builder.GetResultType( target )
						  where IsAppropriateType( returnType, resultType )
						  where AcceptsInput( builderType, inputType )
						  select builder)
					.ToArray();

			return result;
		}

		private static ExpressionBuilder CreateExpressionBuilder( Type builderType, Type desiredType )
		{
			Type actualType = builderType;
			if ( builderType.ContainsGenericParameters )
			{
				actualType = builderType.MakeGenericType( desiredType );
			}

			ExpressionBuilder result = (ExpressionBuilder)Activator.CreateInstance( actualType );
			return result;
		}

		private static bool IsAppropriateType( Type desiredType, Type actualType )
		{
			bool result = actualType == desiredType || actualType.IsSubclassOf( desiredType ) || actualType == typeof( object );
			return result;
		}

		private static bool AcceptsInput( Type filterType, Type inputType )
		{
			var attributes = filterType.GetCustomAttributes( typeof( FilterTargetAttribute ), true );
			if ( attributes.Length == 0 )
			{
				return true;
			}
			else
			{
				return attributes.Cast<FilterTargetAttribute>().Any( attr => attr.TargetType.IsAssignableFrom( inputType ) );
			}
		}
	}
}
