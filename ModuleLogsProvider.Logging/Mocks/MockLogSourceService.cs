using System.Collections.Generic;
using System.Linq;
using Awad.Eticket.ModuleLogsProvider.Types;
using ILogSourceService = ModuleLogsProvider.Logging.MostLogsServices.ILogSourceService;

namespace ModuleLogsProvider.Logging.Mocks
{
	public sealed class MockLogsSourceService : ILogSourceService
	{
		private readonly List<LogMessageInfo> messages = new List<LogMessageInfo>();
		private bool isListening = true;

		public void AddMessage( LogMessageInfo message )
		{
			messages.Add( message );
		}

		#region ILogSourceService Members

		public void ClearMessagesList()
		{
			messages.Clear();
		}

		public void StartListening()
		{
			isListening = true;
		}

		public void StopListening()
		{
			isListening = false;
		}

		public bool GetIsListening()
		{
			return isListening;
		}

		public LogMessageInfo[] GetLinesStartingWithIndex( int index )
		{
			var result = messages.Skip( index ).ToArray();
			return result;
		}

		#endregion
	}
}
