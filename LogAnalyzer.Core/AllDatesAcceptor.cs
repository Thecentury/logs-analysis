using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public sealed class AllDatesAcceptor : DateAcceptorBase
	{
		public override bool Accept(DateTime date)
		{
			return true;
		}
	}
}
