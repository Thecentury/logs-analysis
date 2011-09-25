using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public sealed class FixedDatesAcceptor : DateAcceptorBase
	{
		public DateTime Start { get; set; }
		public DateTime End { get; set; }

		public override bool Accept( DateTime date )
		{
			return Start <= date && date <= End;
		}
	}
}
