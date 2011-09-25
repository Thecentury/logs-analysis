using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging
{
	/// <summary>
	/// Возвращает боевую реализацию.
	/// </summary>
	public sealed class MostServerLogSourceFactory : ILogSourceServiceFactory
	{
		private static readonly MostServerLogSourceFactory instance = new MostServerLogSourceFactory();
		public static MostServerLogSourceFactory Instance
		{
			get { return instance; }
		}

		public IOptionalDisposable<ILogSourceService> CreateClient()
		{
			return new OptionalDisposable<ILogSourceService>( new LogSourceServiceClient() );
		}

		#region IFactory<IOptionalDisposable<ILogSourceService>> Members

		IOptionalDisposable<ILogSourceService> IFactory<IOptionalDisposable<ILogSourceService>>.CreateObject()
		{
			return CreateClient();
		}

		#endregion
	}

	public interface ILogSourceServiceFactory : IOptionalDisposablesFactory<ILogSourceService>
	{
	}
}
