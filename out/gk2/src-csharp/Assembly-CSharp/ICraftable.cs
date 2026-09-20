using JetBrains.Annotations;

public interface ICraftable
{
	string CraftableObjectId { get; }

	Inventory CraftableObjectCraftInventory { get; }

	Inventory CraftableObjectInventory { get; }

	[CanBeNull]
	IWorker CraftableAttachedWorker { get; }

	float AutoCraftTickDuration { get; }

	CraftableType CraftableType { get; }

	void OnAddToQueue(CraftElementBase craftElement)
	{
	}

	void OnCraftStart(CraftElementBase craftElement)
	{
	}

	void OnCraftEnd(CraftElementBase craftElement)
	{
	}

	void OnCraftCancel(CraftElementBase craftElement)
	{
	}

	MultiInventory GetCraftableMultiInventory(bool excludeWorkerInventory = false);

	void OnSuccessfulTicksChange(int startTick, int endTick, CraftElementBase craftElement)
	{
	}

	void ProcessReadyToFinishCraft()
	{
	}

	void MakeDrop(Item item);
}
