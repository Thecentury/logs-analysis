using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class BuilderContext<T> : BuilderContext
		where T : ExpressionBuilder
	{
		private readonly T _builder;

		public BuilderContext( [NotNull] T builder, [NotNull] ParameterExpression parameter )
			: base( builder, parameter )
		{
			_builder = builder;
		}

		public T TypedBuilder
		{
			get { return _builder; }
		}
	}

	internal class BuilderContext
	{
		public BuilderContext( [NotNull] ExpressionBuilder builder, [NotNull] ParameterExpression parameter )
		{
			if ( builder == null )
			{
				throw new ArgumentNullException( "builder" );
			}
			if ( parameter == null )
			{
				throw new ArgumentNullException( "parameter" );
			}
			Builder = builder;
			Parameter = parameter;
		}

		public Type InputType { get { return Parameter.Type; } }
		public ExpressionBuilder Builder { get; private set; }
		public ParameterExpression Parameter { get; private set; }

		public BuilderContext<T> WithBuilder<T>( T builder ) where T : ExpressionBuilder
		{
			return new BuilderContext<T>( builder, Parameter );
		}
	}
}