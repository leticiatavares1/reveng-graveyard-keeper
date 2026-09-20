public interface IOrderExecutor
{
	bool CanAddItem(Item item);

	bool HasItem(Item item);

	void AddItem(Item item);

	void RemoveItem(Item item);
}
