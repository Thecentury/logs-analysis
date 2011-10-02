using System.Collections.Generic;
using System.Linq;
using Awad.Eticket.ModuleLogsProvider.Types;
using ModuleLogsProvider.Logging.MostLogsServices;

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

		public LogMessageInfo[] GetMessages( int startingIndex )
		{
			var result = messages.Skip( startingIndex ).ToArray();
			return result;
		}
	}
}
