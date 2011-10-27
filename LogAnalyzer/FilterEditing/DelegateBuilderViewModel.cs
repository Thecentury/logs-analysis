using System;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class DelegateBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly DelegateBuilderProxy delegateBuilder;

		public DelegateBuilderViewModel( DelegateBuilderProxy builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			if ( builder == null )
				throw new ArgumentNullException( "builder" );

			this.delegateBuilder = builder;
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			delegateBuilder.Inner = builder;
		}

		protected override Type GetResultType()
		{
			return delegateBuilder.GetPropertyType();
		}
	}
}
