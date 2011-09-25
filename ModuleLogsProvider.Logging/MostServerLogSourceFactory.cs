using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awad.Eticket.ModuleLogsProvider.Types.Auxilliary;
using ModuleLogsProvider.Logging.MostLogsServices;

namespace ModuleLogsProvider.Logging
{
	/// <summary>
	/// Возвращает боевую реализацию сервиса ILogSourceService.
	/// </summary>
	public sealed class MostServerLogSourceFactory : ILogSourceServiceFactory
	{
		//public IOptionalDisposable<ILogSourceService> CreateObject()
		//{
		//    return new OptionalDisposable<ILogSourceService>( new LogSourceServiceClient() );
		//}
		#region IFactory<IOptionalDisposable<ILogSourceService>> Members

		//public IOptionalDisposable<ILogSourceService> CreateObject()
		//{
		//    return new OptionalDisposable<ILogSourceService>( new LogSourceServiceClient() );
		//}

		#endregion
		#region IFactory<IOptionalDisposable<ILogSourceService>> Members

		public IOptionalDisposable<ILogSourceService> CreateObject()
		{
			return new OptionalDisposable<ILogSourceService>( new LogSourceServiceClient() );
		}

		#endregion
	}
}
