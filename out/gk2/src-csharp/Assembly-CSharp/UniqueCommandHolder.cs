public abstract class UniqueCommandHolder
{
	public bool isQueued;

	public abstract Command GetCommand();
}
