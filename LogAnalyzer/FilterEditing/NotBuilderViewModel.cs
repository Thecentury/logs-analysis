using LogAnalyzer.Filters;
using System.Linq.Expressions;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal sealed class NotBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly Not notBuilder;
		private readonly ExpressionBuilderViewModel innerViewModel;

		public NotBuilderViewModel( Not notBuilder, ParameterExpression parameter )
			: base( notBuilder, parameter )
		{
			this.notBuilder = notBuilder;
			innerViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( notBuilder, "Inner" ), parameter );

			if ( notBuilder.Inner != null )
			{
				innerViewModel.SelectedChild = ExpressionBuilderViewModelFactory.CreateViewModel( notBuilder.Inner, parameter );
			}
		}

		public ExpressionBuilderViewModel Inner
		{
			get { return innerViewModel; }
		}

		protected override void OnSelectedChildChanged( ExpressionBuilder builder )
		{
			notBuilder.Inner = builder;
		}
	}
}
