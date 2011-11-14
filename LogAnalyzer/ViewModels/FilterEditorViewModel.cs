using System;
using System.Windows.Input;
using JetBrains.Annotations;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;
using LogAnalyzer.GUI.Views;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class FilterEditorViewModel : DialogWindowViewModel
	{
		private readonly FilterEditorWindow window;

		public FilterEditorViewModel([NotNull] FilterEditorWindow window ) : base( window )
		{
			if (window == null) throw new ArgumentNullException("window");
			this.window = window;
		}

		protected override bool CanOkExecute()
		{
			var builder = window.Builder;

			bool isBuilderFull = false;
			if ( builder != null )
			{
				isBuilderFull = builder.ValidateProperties();
			}

			return isBuilderFull;
		}

		public ExpressionBuilder Builder
		{
			get { return window.Builder; }
			set { window.Builder = value; }
		}
	}
}
