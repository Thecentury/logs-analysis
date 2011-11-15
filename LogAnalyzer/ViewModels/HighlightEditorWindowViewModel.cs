using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;
using LogAnalyzer.Filters;
using LogAnalyzer.GUI.Common;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class HighlightEditorWindowViewModel : DialogWindowViewModel
	{
		public HighlightEditorWindowViewModel( [NotNull] Window window ) : base( window )
		{
			selectedColor = ColorHelper.GetRandomColor();
		}

		protected override bool CanOkExecute()
		{
			var builder = SelectedBuilder;

			bool isBuilderFull = false;
			if ( builder != null )
			{
				isBuilderFull = builder.ValidateProperties();
			}

			return isBuilderFull;
		}

		private ExpressionBuilder selectedBuilder;
		public ExpressionBuilder SelectedBuilder
		{
			get { return selectedBuilder; }
			set
			{
				if ( selectedBuilder == value )
					return;

				selectedBuilder = value;
				RaisePropertyChanged( "SelectedBuilder" );
			}
		}

		private Color selectedColor;
		public Color SelectedColor
		{
			get { return selectedColor; }
			set
			{
				if ( selectedColor == value )
					return;

				selectedColor = value;
				RaisePropertyChanged( "SelectedColor" );
			}
		}
	}
}
