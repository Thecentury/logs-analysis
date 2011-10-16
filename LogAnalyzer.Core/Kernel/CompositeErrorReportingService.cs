using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogAnalyzer.Extensions;

namespace LogAnalyzer.Kernel
{
	public sealed class CompositeErrorReportingService : ErrorReportingServiceBase
	{
		private readonly List<ErrorReportingServiceBase> children = new List<ErrorReportingServiceBase>();

		public CompositeErrorReportingService() { }
		public CompositeErrorReportingService( params ErrorReportingServiceBase[] children )
		{
			this.children.AddRange( children );
		}

		protected override void ReportErrorCore( Exception exc, string message )
		{
			foreach ( var child in children )
			{
				child.ReportError( exc, message );
			}
		}
	}
}
