using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace LogAnalyzer
{
	[Serializable]
	public class LogAnalyzerException : Exception
	{
		public LogAnalyzerException() : base( "Ошибка в анализаторе логов." ) { }
		public LogAnalyzerException( string message ) : base( message ) { }
		public LogAnalyzerException( string message, Exception inner ) : base( message, inner ) { }
		protected LogAnalyzerException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }
	}

	[Serializable]
	public class InvalidEncodingException : LogAnalyzerException
	{
		public InvalidEncodingException( LogFile logFile )
			: base( "Выбрана неверная кодировка для чтения файла лога." )
		{
			if ( logFile == null )
				throw new ArgumentNullException( "logFile" );

			this.LogFile = logFile;
			this.Encoding = logFile.Encoding;
		}

		protected InvalidEncodingException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string Message
		{
			get
			{
				return String.Format( "Ошибка при начальной загрузке файла \"{0}\": неверная кодировка (\"{1}\")", LogFile.FullPath, Encoding.WebName );
			}
		}

		public LogFile LogFile { get; private set; }
		public Encoding Encoding { get; private set; }
	}
}
