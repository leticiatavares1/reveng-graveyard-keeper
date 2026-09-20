using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class ZombieCraftActivity : IWorkActivity
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

	public SGuid WgoUniqueId => wgoUniqueId;

	public SGuid ZombieUniqueId => zombieUniqueId;

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

	private CraftComponent CraftComponent => WgoData?.CraftComponent;

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

	public ZombieCraftActivity(WgoData wgoData, ZombieWgoData zombie)
	{
		wgoUniqueId.SetGuid(wgoData.UniqueId);
		this.wgoData = wgoData;
		this.zombie = zombie;
		zombieUniqueId.SetGuid(zombie.UniqueId);
		isActive = true;
	}

	public void Update(float deltaTime)
	{
		if (Zombie == null || CraftComponent == null)
		{
			IsActive = false;
			return;
		}
		if (Zombie.CrafterOrders.Count > 0)
		{
			IsActive = false;
			return;
		}
		if (CraftComponent.Status == CraftComponentStatus.WaitingForOutputDrop)
		{
			IsActive = false;
			return;
		}
		Item hand = Zombie.Hand;
		if (!CanUseTool(hand))
		{
			IsActive = false;
		}
		else
		{
			if (CraftComponent.CurrentCraftElement != null && (CraftComponent.CurrentCraftElement.ParamsData.customRes.GetInt("wait_for_zombie_at_sawmill") == 1 || CraftComponent.CurrentCraftElement.ParamsData.customRes.GetInt("wait_for_zombie_at_mine") == 1 || CraftComponent.CurrentCraftElement.ParamsData.customRes.GetInt("wait_for_zombie_at_sand") == 1 || CraftComponent.CurrentCraftElement.ParamsData.customRes.GetInt("wait_for_zombie_at_clay") == 1))
			{
				return;
			}
			oneTickAnimationProgress += deltaTime;
			float num = 0f;
			if (hand != null && hand.Definition.isTool && CraftComponent.CurrentCraftElement != null)
			{
				if (!CraftComponent.CurrentCraftElement.Def.isAuto && CraftComponent.CurrentCraftElement.Def is CraftDef craftDef)
				{
					for (int i = 0; i < craftDef.zombieSpeedItemModificators.List.Count; i++)
					{
						GameResAtom gameResAtom = craftDef.zombieSpeedItemModificators.List[i];
						if (gameResAtom.type == hand.id)
						{
							num = gameResAtom.value;
							break;
						}
					}
				}
				IsActive = true;
			}
			oneTickAnimationProgress += num * oneTickAnimationProgress;
			if (!(oneTickAnimationProgress >= 1f))
			{
				return;
			}
			int num2 = Mathf.FloorToInt(oneTickAnimationProgress);
			oneTickAnimationProgress -= num2;
			if (CraftComponent.CurrentCraftElement != null)
			{
				if (!CraftComponent.CurrentCraftElement.Def.isAuto)
				{
					UseTool(null, num2);
				}
				IsActive = true;
			}
			else
			{
				IsActive = false;
			}
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

	public int GetActionDamage(Item item)
	{
		if (CraftComponent.CurrentCraftElement == null)
		{
			return 0;
		}
		int num = WgoData.Worker.GetMasteryLevelForTalentBranch(WgoData.Definition.talent, CraftComponent.CurrentCraftElement.Def);
		int num2 = CraftComponent.CurrentCraftElement.Def.talentLock;
		if (num < num2)
		{
			num = num2;
		}
		if (!CraftComponent.CurrentCraftElement.Def.isStarCraft)
		{
			Debug.Log("CraftActivity: Rolling Craft Damage");
			if (num2 == 0)
			{
				num2 = 1;
				if (num == 0)
				{
					num = 1;
				}
			}
			return Math.Clamp(num / num2, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
		}
		Debug.Log("StarCraftActivity: Rolling Craft Damage");
		float num3 = 100f / (float)num2;
		if (num < num2)
		{
			float num4 = num3 * (float)num;
			int num5 = (((float)UnityEngine.Random.Range(1, 100) <= num4) ? 1 : 0);
			Debug.Log($"StarCraftActivity: Final Damage [{num5}]");
			return num5;
		}
		return Math.Clamp(num / num2, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
	}

	public bool CanStartActivity()
	{
		if (CraftComponent != null && !CraftComponent.IsAutoCraftable)
		{
			if (CraftComponent.CurrentCraftElement == null)
			{
				return CraftComponent.IsQueueDelayed;
			}
			return true;
		}
		return false;
	}

	public void OnStartActivity()
	{
	}

	public bool IsEnoughMastery()
	{
		return Zombie.CrafterIsEnoughMastery(WgoData);
	}

	public bool CanUseTool(Item tool)
	{
		if (CraftComponent != null && CraftComponent.CurrentCraftElement != null)
		{
			return Zombie.CrafterCanUseTool(WgoData, CraftComponent.CurrentCraftElement.Def);
		}
		return false;
	}

	public void UseTool(Item tool, int deltaTick)
	{
		int intValue = ConstDef.Get("zombie_craft_sub_ticks_count").IntValue;
		if (CraftComponent.ZombieSubTicks >= intValue)
		{
			CraftComponent.ZombieSubTicks -= intValue;
		}
		int num = CraftComponent.ZombieSubTicks + 1;
		int deltaTicks = 0;
		if (num >= intValue)
		{
			num -= intValue;
			deltaTicks = GetActionDamage(tool);
		}
		CraftComponent.ZombieSubTicks = num;
		CraftComponent.UpdateManual(deltaTicks);
		WgoData.NotifyApplyTool(isFirstHit: false);
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
