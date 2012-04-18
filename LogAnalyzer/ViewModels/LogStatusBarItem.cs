using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace LogAnalyzer.GUI.ViewModels
{
	internal sealed class LogStatusBarItem : BindingObject
	{
		private readonly StatusBarLogWriter _writer;

		public LogStatusBarItem( [NotNull] StatusBarLogWriter writer )
			: base( writer )
		{
			if ( writer == null ) throw new ArgumentNullException( "writer" );
			this._writer = writer;
		}

		public string Message
		{
			get { return _writer.Message; }
		}
	}
}
