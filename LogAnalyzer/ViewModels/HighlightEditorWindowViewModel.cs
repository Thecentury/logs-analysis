using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using JetBrains.Annotations;
using LogAnalyzer.Filters;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class HighlightEditorWindowViewModel : DialogWindowViewModel
	{
		public HighlightEditorWindowViewModel( [NotNull] Window window ) : base( window ) { }

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
