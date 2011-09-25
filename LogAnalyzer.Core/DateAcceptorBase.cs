using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace LogAnalyzer
{
	// todo переделать это на интерфейс
	public abstract class DateAcceptorBase
	{
		public abstract bool Accept( DateTime date );
	}
}
