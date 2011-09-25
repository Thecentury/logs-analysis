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
		private readonly string fileName;
		private readonly Encoding encoding;

		public InvalidEncodingException( Encoding encoding, string fileName )
			: base( "Выбрана неверная кодировка для чтения файла лога." )
		{
			this.encoding = encoding;
			this.fileName = fileName;
		}

		protected InvalidEncodingException( SerializationInfo info, StreamingContext context ) : base( info, context ) { }

		public override string Message
		{
			get
			{
				return String.Format( "Ошибка при начальной загрузке файла \"{0}\": неверная кодировка (\"{1}\")", fileName, encoding.WebName );
			}
		}

		public string FileName
		{
			get { return fileName; }
		}

		public Encoding Encoding
		{
			get { return encoding; }
		}
	}
}
