using System.Linq.Expressions;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class GetPropertyBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel targetViewModel;
		private readonly GetProperty getPropertyBuilder;

		public GetPropertyBuilderViewModel( GetProperty builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			this.getPropertyBuilder = builder;
			targetViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Target" ), parameter );
		}

		public ExpressionBuilderViewModel Target
		{
			get { return targetViewModel; }
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			getPropertyBuilder.Target = builder;
		}
	}
}
