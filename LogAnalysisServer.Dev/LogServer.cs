﻿using System;
using System.Collections.Generic;
using System.Linq;
using Awad.Eticket.ModuleLogsProvider.Types;

namespace LogAnalysisServer.Dev
{
	internal class LogServer : ILogSourceService
	{
		private static readonly List<LogMessageInfo> messages = new List<LogMessageInfo>();

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

		public LogMessageInfo[] GetMessages( int startingIndex )
		{
			LogMessageInfo newMessage = GenerateNewMessage();
			messages.Add( newMessage );

			//return messages.Skip( startingIndex ).ToArray();
			return new[] { newMessage };
		}

		private LogMessageInfo GenerateNewMessage()
		{
			string text = String.Format( "[I] [  4] {0}	Message #{1}", DateTime.Now.ToString( "G" ), messages.Count );

			LogMessageInfo message = new LogMessageInfo
			{
				IndexInAllMessagesList = messages.Count,
				LoggerName = "L1",
				Message = text,
				MessageType = "I"
			};

			return message;
		}
	}
}
