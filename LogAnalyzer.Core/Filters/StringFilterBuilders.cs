using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Markup;

namespace LogAnalyzer.Filters
{
	[ContentProperty( "Inner" )]
	public abstract class StringFilterBuilder : ExpressionBuilder
	{
		[FilterParameter( typeof( ExpressionBuilder ), "Substring", ParameterReturnType = typeof( string ) )]
		public ExpressionBuilder Substring
		{
			get { return GetExpressionBuilder( "Substring" ); }
			set { Set( "Substring", value ); }
		}

		/// <summary>
		/// То, где мы ищем.
		/// </summary>
		[FilterParameter( typeof( ExpressionBuilder ), "Inner", ParameterReturnType = typeof( string ) )]
		public ExpressionBuilder Inner
		{
			get { return GetExpressionBuilder( "Inner" ); }
			set { Set( "Inner", value ); }
		}

		protected override Expression CreateExpressionCore( ParameterExpression argument )
		{
			MethodInfo methodInfo = GetMethod( GetMethod() );
			Expression target = Inner.CreateExpression( argument );

			Expression[] arguments = GetParameters( argument ).ToArray();
			return Expression.Call( target, methodInfo, arguments );
		}

		public sealed override Type GetResultType( ParameterExpression target )
		{
			return typeof( bool );
		}

		protected virtual IEnumerable<Expression> GetParameters( ParameterExpression target )
		{
			Expression substringExpression = Substring.CreateExpression( target );
			yield return substringExpression;
		}

		protected virtual Expression<Func<string, bool>> GetMethod()
		{
			throw new NotImplementedException();
		}
	}

	public abstract class StringComparisonFilterBuilder : StringFilterBuilder
	{
		[FilterParameter( typeof( StringComparison ), "Comparison" )]
		public StringComparison Comparison
		{
			get { return Get<StringComparison>( "Comparison" ); }
			set { Set( "Comparison", value ); }
		}

		protected sealed override IEnumerable<Expression> GetParameters( ParameterExpression target )
		{
			foreach ( var expr in base.GetParameters( target ) )
			{
				yield return expr;
			}

			yield return CreateConstantExpression( Comparison );
		}
	}

	public sealed class StringContains : StringFilterBuilder
	{
		protected override Expression<Func<string, bool>> GetMethod()
		{
			return s => s.Contains( "" );
		}
	}

	public sealed class StringContainsIgnoreCase : StringFilterBuilder
	{
		protected override Expression<Func<string, bool>> GetMethod()
		{
			return s => String.Compare( s, "", StringComparison.InvariantCultureIgnoreCase ) == 0;
		}
	}


	public sealed class StringStartsWith : StringComparisonFilterBuilder
	{
		protected override Expression<Func<string, bool>> GetMethod()
		{
			return s => s.StartsWith( "", Comparison );
		}
	}

	public sealed class StringEndsWith : StringComparisonFilterBuilder
	{
		protected override Expression<Func<string, bool>> GetMethod()
		{
			return s => s.EndsWith( "", Comparison );
		}
	}
}
