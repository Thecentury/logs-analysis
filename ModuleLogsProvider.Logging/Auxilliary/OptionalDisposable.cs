using System;

namespace ModuleLogsProvider.Logging.Auxilliary
{
	public sealed class OptionalDisposable<T> : IOptionalDisposable<T>
		where T : class
	{
		private readonly T inner;

		public OptionalDisposable( T inner )
		{
			if ( inner == null )
				throw new ArgumentNullException( "inner" );

			this.inner = inner;
		}

		public void Dispose()
		{
			IDisposable disposable = inner as IDisposable;
			if ( disposable != null )
			{
				disposable.Dispose();
			}
		}

		public T Inner
		{
			get { return inner; }
		}
	}
}