using System.Collections.ObjectModel;
using System.Reflection;
using LazyBearTechnology;

public abstract class Command : ICommand
{
	public abstract void Execute(ulong senderClientId, GameSave gameSave);

	public virtual byte[] Serialize()
	{
		return LazySerializer.Serialize(this);
	}

	public virtual void Deserialize(byte[] data)
	{
		LazySerializer.DeserializeInto(this, data);
	}

	protected int GetCommandSerializedFieldsSize<T>(T obj) where T : Command
	{
		return CommandFactory.GetCommandTypeFieldsSize(obj);
	}

	protected ReadOnlyCollection<FieldInfo> GetCommandSerializedFields<T>(T obj) where T : Command
	{
		return CommandFactory.GetCommandTypeSerializedFields(obj);
	}

	protected virtual void HandleCommand(Command command)
	{
		LazyNetwork.ConnectionManager.CurrentState.SendNetworkData(command);
	}
}
