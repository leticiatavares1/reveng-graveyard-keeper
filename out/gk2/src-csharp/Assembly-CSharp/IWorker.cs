public interface IWorker
{
	SGuid Id { get; }

	IWorkActivity WorkerActivity { get; }

	MultiInventory WorkerMultiInventory { get; }

	Inventory WorkerInventory { get; }

	Inventory WorkerToolInventory { get; }

	int GetMasteryLevelForTalentBranch(string talentId, CraftDefBase craftDef = null);

	bool HasToolForWork(WgoData wgoData, CraftDefBase craftDef);

	bool HasToolForWork(WgoData wgoData, out ItemType itemType);

	int GetPerksCraftMasteryBonusValue(CraftDefBase craftDef);

	int GetPerksCraftStartTicksBonusValue(CraftDefBase craftDef);

	int GetPerksCraftAddTotalProgressTicksValue(CraftDefBase craftDef);

	float GetPerksEnergyBonusValue(CraftDefBase craftDef);

	float GetPerksInsanityBonusValue(CraftDefBase craftDef);

	Item GetToolForWorkOnCraft(WgoData wgoData, CraftDefBase craftDef);

	CraftStatus CheckWorkerDependentValues(CraftElement craftElement, float deltaTime = 1f, bool skipEnergyCheck = false, bool skipInsanityCheck = false);

	static IWorker FromId(SGuid id)
	{
		if (id == null || id.IsEmpty)
		{
			return null;
		}
		if (id == MainGame.PlayerData.Guid)
		{
			return MainGame.PlayerController;
		}
		return MainGame.ZombieSystemData.GetZombie(id);
	}

	void AddRes(string type, float value);

	void MultiplyRes(string type, float value);

	void SetRes(string type, float value);

	float GetRes(string type, float defaultValue = 0f);
}
