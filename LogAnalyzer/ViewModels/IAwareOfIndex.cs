using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	internal interface IAwareOfIndex
	{
		int IndexInParentCollection { get; }
	}
}
