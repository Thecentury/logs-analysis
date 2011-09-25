namespace ModuleLogsProvider.Logging.Auxilliary
{
	public interface IFactory<out T>
	{
		T CreateObject();
	}
}
