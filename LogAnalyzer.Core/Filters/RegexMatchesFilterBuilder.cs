using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace LogAnalyzer.Filters
{
	public sealed class RegexMatchesFilterBuilder : StringFilterBuilder
	{
		public RegexMatchesFilterBuilder() { }

		public RegexMatchesFilterBuilder([NotNull] string regex, [NotNull] ExpressionBuilder target)
		{
			if (regex == null)
			{
				throw new ArgumentNullException("regex");
			}
			if (target == null)
			{
				throw new ArgumentNullException("target");
			}
			Substring = new StringConstant(regex);
			Inner = target;
		}

		protected override bool ValidatePropertiesCore()
		{
			if ( !base.ValidatePropertiesCore() )
			{
				return false;
			}

			string pattern = null;
			try
			{
				var func = Substring.CompileToFunc<string>();
				pattern = func();
			}
			catch ( Exception exc )
			{

			}

			if ( pattern == null )
			{
				return false;
			}

			try
			{
				Regex regex = new Regex( pattern );
				return true;
			}
			catch ( Exception exc )
			{
				return false;
			}
		}

		protected override Expression CreateExpressionCore( ParameterExpression argument )
		{
			var patternExpression = Substring.CreateExpression( argument );
			string pattern = Expression.Lambda<Func<string>>( patternExpression ).Compile()();

			Regex regex = new Regex( pattern, RegexOptions.Compiled | RegexOptions.Singleline );

			var sourceStringExpression = Inner.CreateExpression( argument );
			return Expression.Call( Expression.Constant( regex ), GetMethod<Regex, string, bool>( ( r, s ) => r.IsMatch( s ) ),
								   sourceStringExpression );
		}
	}
}