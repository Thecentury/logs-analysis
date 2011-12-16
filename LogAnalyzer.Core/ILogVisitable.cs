namespace LogAnalyzer
{
	public interface ILogVisitable
	{
		void Accept( ILogVisitor visitor );
	}
}