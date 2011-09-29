using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModel
{
	public interface IHierarchyMember<TParent, TData>
	{
		TParent Parent { get; }
		TData Data { get; }
	}
}
