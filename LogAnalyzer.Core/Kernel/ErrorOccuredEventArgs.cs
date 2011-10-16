using System;

namespace LogAnalyzer.Kernel
{
	public sealed class ErrorOccuredEventArgs : EventArgs
	{
		public ErrorOccuredEventArgs( Exception exception, string message )
		{
			Exception = exception;
			Message = message;
		}

		public Exception Exception { get; private set; }

		public string Message { get; private set; }
	}
}
