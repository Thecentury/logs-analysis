namespace LogAnalyzer.Collections
{
	public interface INavigatable<out T>
	{
		IBidirectionalEnumerable<T> GetNavigator();
	}
}