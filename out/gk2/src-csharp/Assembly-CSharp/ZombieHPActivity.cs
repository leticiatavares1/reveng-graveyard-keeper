using System;
using UnityEngine;

[Serializable]
public class ZombieHPActivity : IWorkActivity
{
	[SerializeField]
	private float oneTickAnimationProgress;

	[SerializeField]
	private SGuid wgoUniqueId = SGuid.Empty;

	[SerializeField]
	private SGuid zombieUniqueId = SGuid.Empty;

	private WgoData wgoData;

	private ZombieWgoData zombie;

	private bool isActive;

	private WgoData WgoData
	{
		get
		{
			if (wgoData == null)
			{
				wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(wgoUniqueId);
			}
			return wgoData;
		}
	}

	public ZombieWgoData Zombie
	{
		get
		{
			if (zombie == null)
			{
				zombie = MainGame.ZombieSystemData.GetZombie(zombieUniqueId);
			}
			return zombie;
		}
	}

	public bool IsActive
	{
		get
		{
			return isActive;
		}
		private set
		{
			bool num = isActive;
			isActive = value;
			if (num != isActive)
			{
				this.OnActiveStateChanged?.Invoke();
			}
		}
	}

	public event Action OnActiveStateChanged;

	public ZombieHPActivity(WgoData wgoData, ZombieWgoData zombie)
	{
		wgoUniqueId.SetGuid(wgoData.UniqueId);
		this.wgoData = wgoData;
		this.zombie = zombie;
		zombieUniqueId.SetGuid(zombie.UniqueId);
		isActive = true;
	}

	public void Update(float deltaTime)
	{
		if (WgoData == null)
		{
			IsActive = false;
			return;
		}
		if (WgoData.HpComponent.Hp <= 0)
		{
			IsActive = false;
			return;
		}
		Item hand = Zombie.Hand;
		if (!CanUseTool(hand))
		{
			IsActive = false;
			return;
		}
		oneTickAnimationProgress += deltaTime;
		if (oneTickAnimationProgress >= 1f)
		{
			int num = Mathf.Max(1, Mathf.FloorToInt(oneTickAnimationProgress));
			oneTickAnimationProgress -= num;
			UseTool(hand, num);
		}
	}

	public bool IsEnoughDurability(Item tool)
	{
		if (tool.TryGetProperty<DurabilitySerializedItemProperty>(out var property))
		{
			return property.Durability > tool.Definition.durDecreaseOnUse;
		}
		return true;
	}

	public int GetActionDamage(Item tool)
	{
		bool flag = wgoData.GetGameResInt("seed_mastery_lock") > 0;
		int num = Zombie.GetMasteryLevelForTalentBranch(wgoData.Definition.talent);
		int num2 = (flag ? wgoData.GetGameResInt("seed_mastery_lock") : wgoData.Definition.MasteryLock);
		if (num <= num2 && flag)
		{
			return 1;
		}
		if (wgoData.Definition.noMasteryLock)
		{
			return 1;
		}
		if (num < num2)
		{
			return 0;
		}
		Debug.Log("HPActivity: Rolling HP Damage");
		if (num2 == 0)
		{
			num2 = 1;
			if (num == 0)
			{
				num = 1;
			}
		}
		return Math.Clamp(num / num2, -ConstDef.Get("max_cells_per_one_hit").IntValue, ConstDef.Get("max_cells_per_one_hit").IntValue);
	}

	public bool CanStartActivity()
	{
		if (WgoData != null)
		{
			return WgoData.HpComponent.Hp > 0;
		}
		return false;
	}

	public void OnStartActivity()
	{
	}

	public bool IsEnoughMastery()
	{
		if (wgoData.GetGameResInt("seed_mastery_lock") > 0)
		{
			return true;
		}
		return Zombie.GetMasteryLevelForTalentBranch(wgoData.Definition.talent) >= wgoData.Definition.MasteryLock;
	}

	public bool CanUseTool(Item tool)
	{
		return Zombie.CrafterCanUseTool(WgoData, null);
	}

	public void UseTool(Item tool, int deltaTick)
	{
		bool hasFullHp = WgoData.HpComponent.HasFullHp;
		int actionDamage = GetActionDamage(tool);
		WgoData.HpComponent.ApplyDamage(actionDamage);
		wgoData.NotifyApplyTool(hasFullHp);
	}

	public bool IsEnoughEnergy(Item tool, float energyPerTick)
	{
		return true;
	}

	public void ConsumeEnergy(Item tool, float energyPerTick)
	{
	}

	public float GetEnergyCostPerTick(Item tool)
	{
		return 0f;
	}

	public bool CanChangeInsanity(Item tool, float insanityPerTick)
	{
		return true;
	}

	public void ChangeInsanity(Item tool, float insanityPerTick)
	{
	}

	public float GetInsanityCostPerTick(Item tool)
	{
		return 0f;
	}
}
