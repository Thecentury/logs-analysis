using System.IO;

namespace LogAnalyzer.Kernel
{
	public interface IStreamProvider
	{
		Stream OpenStream( int startPosition );
	}
}