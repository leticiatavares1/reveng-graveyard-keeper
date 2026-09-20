public abstract class UniqueCommandHolderDataWrapper<T> : UniqueCommandHolder
{
	public abstract void RegisterData(T data);

	public abstract void UnregisterData();
}
