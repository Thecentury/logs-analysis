using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.Regions
{
	public interface IRegion
	{
		string Name { get; }
		void Add( object child );
	}
}
