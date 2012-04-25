using System;
using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class DelegateBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly DelegateBuilderProxy _delegateBuilder;

		public DelegateBuilderViewModel( BuilderContext<DelegateBuilderProxy> ctx  )
			: base( ctx )
		{
			this._delegateBuilder = ctx.TypedBuilder;
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			_delegateBuilder.Inner = builder;
		}

		protected override Type GetResultType()
		{
			return _delegateBuilder.GetPropertyType();
		}
	}
}
