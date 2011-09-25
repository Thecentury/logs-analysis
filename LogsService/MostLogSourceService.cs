using System;
using Awad.Eticket.ModuleLogsProvider.Types;

namespace LogsService
{
	/// <summary>
	/// Реализация сервера логов, в основном, для времени разработки.
	/// </summary>
	public sealed class MostLogSourceService : ILogSourceService
	{
		private bool isListening = true;

		public void ClearMessagesList()
		{
			throw new NotImplementedException();
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

		public LogMessageInfo[] GetMessages( int index )
		{
			throw new NotImplementedException();
		}
	}
}
