using ExpressionBuilderSample;
using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal class StringFilterBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel stringViewModel;
		private readonly ExpressionBuilderViewModel substringViewModel;

		public StringFilterBuilderViewModel( StringFilterBuilder builder, ParameterExpression parameter )
			: base( builder, parameter )
		{
			stringViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Inner" ), parameter );
			substringViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( new DelegateBuilderProxy( builder, "Substring" ), parameter );
		}

		public ExpressionBuilderViewModel String
		{
			get { return stringViewModel; }
		}

		public ExpressionBuilderViewModel Substring
		{
			get { return substringViewModel; }
		}
	}
}
