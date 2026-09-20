using System;
using System.Collections.Generic;

[Serializable]
public class CraftSystem : ICustomUpdatable
{
	private const float CRAFTS_CHECK_TIME = 1f;

	private float craftsCheckTimer;

	private static CraftSystemData CraftSystemData => MainGame.Instance.GameSave.craftSystemData;

	public void CustomUpdate(float deltaTime)
	{
		List<CraftComponent> activeCrafts = CraftSystemData.activeCrafts;
		if (activeCrafts != null)
		{
			craftsCheckTimer += deltaTime;
			if (craftsCheckTimer >= 1f)
			{
				foreach (CraftComponent item in activeCrafts)
				{
					if (item.Status != CraftComponentStatus.ReadyToFinishAutoCraft)
					{
						if (item.IsQueueDelayed || item.Status == CraftComponentStatus.ReadyToStartCraft || item.Status == CraftComponentStatus.FinishDelayed)
						{
							item.UpdateQueueElementsCraftStatus();
						}
						if (!item.IsAutoCraftable)
						{
							item.UpdateCanContinueManualCraftState(deltaTime);
						}
					}
				}
				craftsCheckTimer = 0f;
			}
			for (int i = 0; i < activeCrafts.Count; i++)
			{
				CraftComponent craftComponent = activeCrafts[i];
				if (craftComponent.IsAutoCraftable && !craftComponent.HasPreFinishUpdate && !craftComponent.IsDestroyingCraftActive)
				{
					craftComponent.Update(deltaTime);
				}
				if (craftComponent.HasPreFinishUpdate)
				{
					craftComponent.PreFinishUpdate(deltaTime);
				}
			}
		}
		for (int num = CraftSystemData.zombieCraftActivities.Count - 1; num >= 0; num--)
		{
			CraftSystemData.zombieCraftActivities[num].Update(deltaTime);
		}
		for (int num2 = CraftSystemData.zombieHPActivities.Count - 1; num2 >= 0; num2--)
		{
			CraftSystemData.zombieHPActivities[num2].Update(deltaTime);
		}
	}

	public void AddCraftObject(CraftComponent craftComponent)
	{
		CraftSystemData craftSystemData = CraftSystemData;
		if (craftSystemData.activeCrafts == null)
		{
			craftSystemData.activeCrafts = new List<CraftComponent>();
		}
		if (!CraftSystemData.activeCrafts.Contains(craftComponent))
		{
			CraftSystemData.activeCrafts.Add(craftComponent);
		}
	}

	public void RemoveCraftObject(CraftComponent craftComponent)
	{
		CraftSystemData.activeCrafts?.Remove(craftComponent);
	}

	public void AddWorker(ZombieCraftActivity craftActivity)
	{
		CraftSystemData.zombieCraftActivities.RemoveAll((ZombieCraftActivity a) => a.ZombieUniqueId.Guid == craftActivity.ZombieUniqueId.Guid);
		CraftSystemData.zombieCraftActivities.Add(craftActivity);
	}

	public void RemoveWorker(ZombieCraftActivity craftActivity)
	{
		CraftSystemData.zombieCraftActivities.RemoveAll((ZombieCraftActivity a) => a.ZombieUniqueId.Guid == craftActivity.ZombieUniqueId.Guid);
	}

	public void AddHPWorker(ZombieHPActivity hpActivity)
	{
		CraftSystemData.zombieHPActivities.Add(hpActivity);
	}

	public void RemoveHPWorker(ZombieHPActivity hpActivity)
	{
		CraftSystemData.zombieHPActivities.Remove(hpActivity);
	}

	public ZombieCraftActivity TryGetCraftActivity(ZombieWgoData zombieWgoData)
	{
		foreach (ZombieCraftActivity zombieCraftActivity in CraftSystemData.zombieCraftActivities)
		{
			if (zombieCraftActivity.ZombieUniqueId.Guid == zombieWgoData.UniqueId.Guid)
			{
				return zombieCraftActivity;
			}
		}
		return null;
	}

	public ZombieHPActivity TryGetHPActivity(ZombieWgoData zombieWgoData)
	{
		foreach (ZombieHPActivity zombieHPActivity in CraftSystemData.zombieHPActivities)
		{
			if (zombieHPActivity.Zombie.UniqueId.Guid == zombieWgoData.UniqueId.Guid)
			{
				return zombieHPActivity;
			}
		}
		return null;
	}
}
