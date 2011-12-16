using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public interface ISaveable
	{
		void Write( TextWriter writer );
	}
}
