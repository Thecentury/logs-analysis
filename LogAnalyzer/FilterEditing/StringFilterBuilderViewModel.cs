using LogAnalyzer.Filters;
using System.Linq.Expressions;
using LogAnalyzer.GUI.FilterEditor;

namespace LogAnalyzer.GUI.FilterEditing
{
	internal class StringFilterBuilderViewModel : ExpressionBuilderViewModel
	{
		private readonly ExpressionBuilderViewModel _stringViewModel;
		private readonly ExpressionBuilderViewModel _substringViewModel;

		public StringFilterBuilderViewModel( BuilderContext<StringFilterBuilder> ctx )
			: base( ctx )
		{
			_stringViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( ctx.WithBuilder( new DelegateBuilderProxy( ctx.TypedBuilder, "Inner" ) ) );
			_substringViewModel = ExpressionBuilderViewModelFactory.CreateViewModel( ctx.WithBuilder( new DelegateBuilderProxy( ctx.TypedBuilder, "Substring" ) ) );
		}

		public ExpressionBuilderViewModel String
		{
			get { return _stringViewModel; }
		}

		public ExpressionBuilderViewModel Substring
		{
			get { return _substringViewModel; }
		}
	}
}
