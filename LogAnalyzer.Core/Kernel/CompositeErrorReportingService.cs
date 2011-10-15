using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.Kernel
{
	public sealed class CompositeErrorReportingService : IErrorReportingService
	{
		private readonly List<IErrorReportingService> children = new List<IErrorReportingService>();

		public CompositeErrorReportingService() { }
		public CompositeErrorReportingService( params IErrorReportingService[] children )
		{
			this.children.AddRange( children );
		}

		public void ReportError( Exception exc, string message )
		{
			foreach ( var child in children )
			{
				child.ReportError( exc, message );
			}
		}
	}
}
