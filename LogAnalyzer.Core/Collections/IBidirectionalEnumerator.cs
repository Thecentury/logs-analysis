using System.Collections.Generic;

namespace LogAnalyzer.Collections
{
	public interface IBidirectionalEnumerator<out T> : IEnumerator<T>
	{
		bool MoveBack();
	}

	public interface IRandomAccessEnumerator<out T> : IBidirectionalEnumerator<T>
	{
		long Position { get; set; }
	}
}