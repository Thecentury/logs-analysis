using System.Collections.Generic;

namespace LogAnalyzer.Collections
{
	public interface IBidirectionalEnumerator<out T> : IEnumerator<T>
	{
		bool MoveBack();
	}
}