using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAnalyzer.GUI.ViewModels
{
	public interface IHierarchyMember<out TParent, out TData>
	{
		TParent Parent { get; }
		TData Data { get; }
	}
}
