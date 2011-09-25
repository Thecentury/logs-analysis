using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public class LastNMinutesAcceptor : DateAcceptorBase
	{
		public int MinutesCount { get; set; }

		public override bool Accept( DateTime date )
		{
			DateTime now = DateTime.Now;
			TimeSpan delta = now - date;

			return delta.TotalMinutes < MinutesCount;
		}
	}
}
