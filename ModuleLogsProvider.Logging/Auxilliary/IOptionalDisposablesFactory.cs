namespace ModuleLogsProvider.Logging.Auxilliary
{
	public interface IOptionalDisposablesFactory<out T> : IFactory<IOptionalDisposable<T>>
	{
	}
}
