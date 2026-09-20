using System;

[Serializable]
public class SpawnDelayedObject
{
	public CraftElement craftElement;

	public WgoData wgoData;

	public SGuid wgoUniqueId;

	public WgoData ResolvedWgoData
	{
		get
		{
			SGuid uniqueId = wgoUniqueId;
			if (SGuid.IsNullOrEmpty(uniqueId) && wgoData != null)
			{
				uniqueId = wgoData.UniqueId;
			}
			if (SGuid.IsNullOrEmpty(uniqueId))
			{
				return null;
			}
			return MainGame.Instance.GameSave.worldData.GetWgoData(uniqueId);
		}
	}

	public SpawnDelayedObject()
	{
	}

	public SpawnDelayedObject(CraftElement craftElement, WgoData wgoData)
	{
		this.craftElement = craftElement;
		wgoUniqueId = SGuid.Empty;
		wgoUniqueId.SetGuid(wgoData.UniqueId);
	}
}
