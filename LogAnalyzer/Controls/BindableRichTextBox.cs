using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.GUI.Controls
{
	public sealed class BindableRichTextBox : RichTextBox
	{
		public Paragraph Paragraph
		{
			get { return (Paragraph)GetValue( ParagraphProperty ); }
			set { SetValue( ParagraphProperty, value ); }
		}

		public static readonly DependencyProperty ParagraphProperty = DependencyProperty.Register(
		  "Paragraph",
		  typeof( Paragraph ),
		  typeof( BindableRichTextBox ),
		  new FrameworkPropertyMetadata( null, OnParagraphReplaced ) );

		private static void OnParagraphReplaced( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			BindableRichTextBox textBox = (BindableRichTextBox)d;
			Paragraph paragraph = (Paragraph)e.NewValue;
			textBox.Dispatcher.BeginInvoke( () => textBox.ReplaceParagraph( paragraph ), DispatcherPriority.Normal );
		}

		private void ReplaceParagraph( Paragraph paragraph )
		{
			Document.Blocks.Clear();
			if ( paragraph != null )
			{
				Document.Blocks.Add( paragraph );
			}
		}
	}
}
