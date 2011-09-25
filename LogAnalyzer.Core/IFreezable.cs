using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer
{
	public interface IFreezable
	{
		void Freeze();
		bool IsFrozen { get; }
	}
}
