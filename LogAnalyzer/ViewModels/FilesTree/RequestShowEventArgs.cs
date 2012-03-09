using System;

namespace LogAnalyzer.GUI.ViewModels.FilesTree
{
	public sealed class RequestShowEventArgs : EventArgs
	{
		private readonly object _source;

		public RequestShowEventArgs( object source )
		{
			_source = source;
		}

		public object Source
		{
			get { return _source; }
		}
	}
}