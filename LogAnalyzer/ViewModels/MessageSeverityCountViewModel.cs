using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	public sealed class MessageSeverityCountViewModel : BindingObject
	{
		private readonly MessageSeverityCount source;

		public MessageSeverityCountViewModel( MessageSeverityCount source )
			: base( source )
		{
			if (source == null) throw new ArgumentNullException("source");
			this.source = source;
		}

		public int Error
		{
			get { return source.Error; }
		}

		public int Warning
		{
			get { return source.Warning; }
		}

		public int Info
		{
			get { return source.Info; }
		}

		public int Debug
		{
			get { return source.Debug; }
		}

		public int Verbose
		{
			get { return source.Verbose; }
		}
	}
}
