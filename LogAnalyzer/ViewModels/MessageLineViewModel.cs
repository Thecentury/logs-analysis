using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace LogAnalyzer.GUI.ViewModels
{
	public class MessageLineViewModel
	{
		private readonly string text;

		public MessageLineViewModel( string text )
		{
			if ( text == null )
				throw new ArgumentNullException( "text" );

			this.text = text;
		}

		public string Text
		{
			get { return text; }
		}
	}
}
