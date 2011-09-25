using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace ExpressionBuilderSample
{
	internal sealed class DelegateBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly DelegateBuilderProxy delegateBuilder = null;

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
