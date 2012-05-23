using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Xaml;
using Microsoft.Windows.Controls;

namespace LogAnalyzer.GUI.ViewModels
{
	[DebuggerDisplay( "{Text}" )]
	public abstract class MessagePart
	{
		private readonly string _text;

		protected MessagePart( string text )
		{
			_text = text;
		}

		public string Text
		{
			get { return _text; }
		}
	}

	[DebuggerDisplay( "{Text}" )]
	public sealed class CommonMessagePart : MessagePart
	{
		public CommonMessagePart( string value )
			: base( value )
		{
		}
	}

	[DebuggerDisplay( "Group {GroupNumber}: {Text}" )]
	public sealed class GroupMessagePart : MessagePart
	{
		private readonly int _groupNumber;

		public GroupMessagePart( string text, int groupNumber )
			: base( text )
		{
			_groupNumber = groupNumber;
		}

		public int GroupNumber
		{
			get { return _groupNumber; }
		}
	}
}
