using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer;

namespace ModuleLogsProvider.Logging.Most
{
	public sealed class MostFileInfo : IFileInfo
	{
		#region IFileInfo Members

		public void Refresh()
		{
			throw new NotImplementedException();
		}

		public System.IO.Stream OpenStream( int startPosition )
		{
			throw new NotImplementedException();
		}

		public int Length
		{
			get { throw new NotImplementedException(); }
		}

		public string Name
		{
			get { throw new NotImplementedException(); }
		}

		public string FullName
		{
			get { throw new NotImplementedException(); }
		}

		public string Extension
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LastWriteTime
		{
			get { throw new NotImplementedException(); }
		}

		public DateTime LoggingDate
		{
			get { throw new NotImplementedException(); }
		}

		#endregion
	}
}
