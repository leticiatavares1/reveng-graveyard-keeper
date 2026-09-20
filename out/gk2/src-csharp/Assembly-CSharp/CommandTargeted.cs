using UnityEngine;

public abstract class CommandTargeted<T> : Command
{
	protected T target;

	public CommandTargeted()
	{
	}

	public CommandTargeted(T target)
	{
		this.target = target;
		Debug.Log($"CommandTargeted Constructor: {target}");
	}
}
