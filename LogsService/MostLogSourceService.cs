using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Awad.Eticket.ModuleLogsProvider.Types;

namespace LogsService
{
	public class MostLogSourceService : ILogSourceService
	{
		public void ClearMessagesList()
		{
			throw new NotImplementedException();
		}

		public void StartListening()
		{
			throw new NotImplementedException();
		}

		public void StopListening()
		{
			throw new NotImplementedException();
		}

		public bool GetIsListening()
		{
			throw new NotImplementedException();
		}

		public LogMessageInfo[] GetLinesStartingWithIndex( int index )
		{
			throw new NotImplementedException();
		}
	}
}
