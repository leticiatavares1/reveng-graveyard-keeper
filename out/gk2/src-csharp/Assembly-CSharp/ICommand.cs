public interface ICommand
{
	void Execute(ulong senderClientId, GameSave gameSave);

	byte[] Serialize();

	void Deserialize(byte[] data);
}
