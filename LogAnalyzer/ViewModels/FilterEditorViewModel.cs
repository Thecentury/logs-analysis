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
		private readonly FilterEditorWindow _window;

		public FilterEditorViewModel( [NotNull] FilterEditorWindow window, [NotNull] Type inputType )
			: base( window )
		{
			if ( window == null )
			{
				throw new ArgumentNullException( "window" );
			}
			if (inputType == null)
			{
				throw new ArgumentNullException("inputType");
			}

			this._window = window;
			_inputType = inputType;
		}

		protected override bool CanOkExecute()
		{
			var builder = _window.Builder;

			bool isBuilderFull = false;
			if ( builder != null )
			{
				isBuilderFull = builder.ValidateProperties();
			}

			return isBuilderFull;
		}

		public ExpressionBuilder Builder
		{
			get { return _window.Builder; }
			set { _window.Builder = value; }
		}

		private Type _inputType = typeof( LogEntry );
		public Type InputType
		{
			get { return _inputType; }
			set { _inputType = value; }
		}
	}
}
