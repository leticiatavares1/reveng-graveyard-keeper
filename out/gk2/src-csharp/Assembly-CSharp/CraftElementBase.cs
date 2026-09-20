using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftElementBase
{
	[SerializeField]
	protected string craftId;

	[SerializeField]
	protected bool isStarted;

	[SerializeField]
	protected bool isPaused;

	[SerializeField]
	protected bool isFinished;

	[SerializeField]
	protected bool isAllRequirementsTaken;

	[SerializeField]
	protected int currentProgressTicks;

	[SerializeField]
	protected int totalProgressTicks;

	[SerializeField]
	protected int failedProgressTicks;

	[SerializeField]
	protected int succeededProgressTicks;

	[SerializeField]
	protected List<NeedItemData> requirements;

	[SerializeField]
	protected int count;

	[SerializeField]
	protected bool isInfinite;

	[SerializeField]
	protected List<Item> craftInput = new List<Item>();

	[SerializeField]
	protected CraftStatus craftStatus;

	[SerializeField]
	protected CraftParamsData paramsData;

	[SerializeField]
	protected List<Item> customCraftOutput = new List<Item>();

	[SerializeField]
	protected List<Item> customItems = new List<Item>();

	[SerializeField]
	protected List<ItemCount> preToWgoOnStartItems = new List<ItemCount>();

	[SerializeField]
	protected List<ItemCount> preToWgoOnFinishItems = new List<ItemCount>();

	[SerializeField]
	protected List<ItemCount> preOutputItems = new List<ItemCount>();

	[SerializeField]
	protected bool isFinishOutputUpdated;

	[SerializeField]
	protected CraftComponentStatus prevCraftComponentStatus;

	[SerializeField]
	protected bool isPreFinishUpdated;

	[NonSerialized]
	protected ICraftable craftable;

	private CraftDefBase def;

	public string CraftId => craftId;

	public virtual bool IsStarted => isStarted;

	public bool IsFinished => isFinished;

	public virtual bool CaBeFinished => true;

	public bool IsAllRequirementsTaken
	{
		get
		{
			return isAllRequirementsTaken;
		}
		set
		{
			isAllRequirementsTaken = value;
		}
	}

	public bool IsFailed
	{
		get
		{
			if (Def.isStarCraft && paramsData.total <= 0f)
			{
				return paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common;
			}
			return false;
		}
	}

	public CraftComponentStatus PrevCraftComponentStatus
	{
		get
		{
			return prevCraftComponentStatus;
		}
		set
		{
			prevCraftComponentStatus = value;
		}
	}

	public bool IsPreFinishUpdated
	{
		get
		{
			return isPreFinishUpdated;
		}
		set
		{
			isPreFinishUpdated = value;
		}
	}

	public List<Item> CustomItems => customItems;

	public CraftDefBase Def
	{
		get
		{
			if (def != null)
			{
				return def;
			}
			def = GetCraftDef();
			return def;
		}
	}

	public List<Item> CraftInput => craftInput;

	public List<NeedItemData> Requirements => requirements;

	public CraftParamsData ParamsData => paramsData;

	public float ProgressTimeNormalized
	{
		get
		{
			if (totalProgressTicks == 0)
			{
				return 1f;
			}
			return (float)currentProgressTicks / (float)totalProgressTicks;
		}
	}

	public float ProgressTimeNormalizedFailed
	{
		get
		{
			if (totalProgressTicks == 0)
			{
				return 1f;
			}
			return (float)failedProgressTicks / (float)totalProgressTicks;
		}
	}

	public int ProgressTicks => currentProgressTicks;

	public virtual int TotalProgressTicks => totalProgressTicks;

	public int FailedProgressTicks => failedProgressTicks;

	public int SucceededProgressTicks => succeededProgressTicks;

	public List<ItemCount> PreToWgoOnStartItems => preToWgoOnStartItems;

	public List<ItemCount> PreToWgoOnFinishItems => preToWgoOnFinishItems;

	public List<ItemCount> PreOutputItems => preOutputItems;

	public int Count
	{
		get
		{
			return count;
		}
		set
		{
			count = value;
			this.OnCountChanged?.Invoke();
		}
	}

	public bool IsInfinite
	{
		get
		{
			return isInfinite;
		}
		set
		{
			if (!value)
			{
				count = 1;
			}
			isInfinite = value;
			this.OnCountChanged?.Invoke();
		}
	}

	public CraftStatus CraftStatus
	{
		get
		{
			return craftStatus;
		}
		set
		{
			if (craftStatus != value)
			{
				craftStatus = value;
				this.OnStatusChanged?.Invoke(craftStatus);
			}
		}
	}

	public event Action<CraftStatus> OnStatusChanged;

	public event Action OnCountChanged;

	public event Action OnProgressChanged;

	public bool TryMergeWith(CraftElementBase other)
	{
		if (Def.IsOneTimeCraft())
		{
			return false;
		}
		if (craftId == other.craftId)
		{
			if (isStarted != other.isStarted)
			{
				return false;
			}
			if (isPaused != other.isPaused)
			{
				return false;
			}
			if (!paramsData.Equals(other.paramsData))
			{
				return false;
			}
			if (requirements.Count != other.requirements.Count)
			{
				return false;
			}
			for (int i = 0; i < requirements.Count; i++)
			{
				if (!requirements[i].Equals(other.requirements[i]))
				{
					return false;
				}
			}
			Count += other.count;
			return true;
		}
		return false;
	}

	public CraftElementBase()
	{
	}

	public CraftElementBase(string craftId, int count, CraftParamsData craftParamsData)
		: this(craftId, count, new List<NeedItemData>(), craftParamsData)
	{
	}

	public CraftElementBase(string craftId, int count, List<NeedItemData> requirements, CraftParamsData paramsData)
	{
		this.craftId = craftId;
		this.count = count;
		this.requirements = requirements;
		this.paramsData = paramsData;
	}

	protected CraftElementBase(CraftElementBase other, int count = 1)
	{
		CopyFrom(other, count);
	}

	protected virtual void CopyFrom(CraftElementBase other, int count = 1)
	{
		craftId = other.craftId;
		isStarted = other.isStarted;
		isPaused = other.isPaused;
		isFinished = other.isFinished;
		isAllRequirementsTaken = other.isAllRequirementsTaken;
		currentProgressTicks = other.currentProgressTicks;
		totalProgressTicks = other.totalProgressTicks;
		failedProgressTicks = other.failedProgressTicks;
		succeededProgressTicks = other.succeededProgressTicks;
		requirements = new List<NeedItemData>(other.requirements);
		this.count = count;
		isInfinite = false;
		craftInput = new List<Item>(other.craftInput);
		craftStatus = other.craftStatus;
		paramsData = new CraftParamsData(other.paramsData);
		customCraftOutput = new List<Item>(other.customCraftOutput);
		customItems = new List<Item>(other.customItems);
		preToWgoOnStartItems = new List<ItemCount>(other.preToWgoOnStartItems);
		preToWgoOnFinishItems = new List<ItemCount>(other.preToWgoOnFinishItems);
		preOutputItems = new List<ItemCount>(other.preOutputItems);
		isFinishOutputUpdated = other.isFinishOutputUpdated;
		craftable = other.craftable;
	}

	public virtual CraftElementBase Clone(int count = 1)
	{
		return new CraftElementBase(this, count);
	}

	public NeedItemData GetRequirement(string itemId)
	{
		foreach (NeedItemData requirement in requirements)
		{
			if (itemId == requirement.id)
			{
				return requirement;
			}
		}
		return null;
	}

	public void AddRequirement(NeedItemData needItemData)
	{
		requirements.Add(needItemData);
	}

	public void RemoveRequirement(string itemId)
	{
		for (int i = 0; i < requirements.Count; i++)
		{
			if (requirements[i].id == itemId)
			{
				requirements.RemoveAt(i);
				break;
			}
		}
	}

	public virtual void DoBeforeStartCalculations(ICraftable craftable)
	{
		preToWgoOnStartItems = (Def.isStarCraft ? Def.addItemsToWgoOnStart.MakePreOutput(craftable, (paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common) ? paramsData.total : 0f) : Def.addItemsToWgoOnStart.MakePreOutput(craftable));
		preToWgoOnFinishItems = (Def.isStarCraft ? Def.addItemsToWgoOnFinish.MakePreOutput(craftable, (paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common) ? paramsData.total : 0f) : Def.addItemsToWgoOnFinish.MakePreOutput(craftable));
	}

	public virtual CraftStatus CanStartCraft(ICraftable craftableObject, MultiInventory multiInventory = null)
	{
		KnowledgeSystem knowledgeSystem = MainGame.Instance?.GameSave?.knowledgeSystem;
		if (knowledgeSystem != null && knowledgeSystem.IsOneTimeCraftCompleted(Def))
		{
			return CraftStatus.Other;
		}
		if (multiInventory == null)
		{
			multiInventory = craftableObject.GetCraftableMultiInventory();
		}
		if ((Def.isStarCraft || Def.isAutopsyCraft) && craftableObject is WgoData wgoData && craftableObject.CraftableAttachedWorker != null && craftableObject.CraftableAttachedWorker.GetMasteryLevelForTalentBranch(wgoData.Definition.talent, Def) <= 0)
		{
			return CraftStatus.NotEnoughMastery;
		}
		if (requirements.Count > 0 && !multiInventory.HasItemsById(requirements, craftableObject as WgoData))
		{
			return CraftStatus.NotEnoughResources;
		}
		if (Def.needItemsFromWgo.Count > 0 && !craftableObject.CraftableObjectInventory.Data.HasItemsWithIds(Def.needItemsFromWgo))
		{
			return CraftStatus.NotEnoughResources;
		}
		if (Def.addItemsToWgoOnStart.HasOutputItems && !craftableObject.CraftableObjectInventory.CanAddItemsToInventory(preToWgoOnStartItems))
		{
			return CraftStatus.NotEnoughSpaceInWgo;
		}
		return CraftStatus.OK;
	}

	public virtual CraftStatus CanFinishCraft(ICraftable craftableObject)
	{
		if (Def.isAuto)
		{
			if (customCraftOutput.Count == 0)
			{
				if (!craftableObject.CraftableObjectCraftInventory.CanAddItemsToInventory(preOutputItems))
				{
					return CraftStatus.NotEnoughSpaceInWgo;
				}
			}
			else
			{
				List<ItemCount> list = new List<ItemCount>();
				foreach (ItemCount item in list)
				{
					list.Add(item);
				}
				foreach (Item item2 in customCraftOutput)
				{
					list.Add(new ItemCount(item2));
					if (!craftableObject.CraftableObjectCraftInventory.CanAddItemsToInventory(list))
					{
						return CraftStatus.NotEnoughSpaceInWgo;
					}
				}
			}
		}
		if (Def.addItemsToWgoOnFinish.HasOutputItems && !craftableObject.CraftableObjectInventory.CanAddItemsToInventoryConsideringDestination(this, preToWgoOnFinishItems))
		{
			return CraftStatus.NotEnoughSpaceInWgo;
		}
		if (!Def.isAuto && craftableObject.CraftableAttachedWorker != null && paramsData.customRes.GetInt("do_not_check_multiinventory_space") == 0 && craftableObject.CraftableAttachedWorker is ZombieWgoData && !craftableObject.GetCraftableMultiInventory(excludeWorkerInventory: true).CanAddItems(preOutputItems))
		{
			return CraftStatus.NotEnoughSpaceInMultiInventory;
		}
		return CraftStatus.OK;
	}

	public void BindCraftable(ICraftable craftable)
	{
		this.craftable = craftable;
	}

	public virtual void Start(ICraftable craftable)
	{
		currentProgressTicks = ((Def.isStarCraft || paramsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing || Def.isAutopsyCraft) ? paramsData.CraftStartTicks : 0);
		failedProgressTicks = ((paramsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing) ? paramsData.FailedStartTicks : 0);
		succeededProgressTicks = currentProgressTicks;
		currentProgressTicks += failedProgressTicks;
		this.craftable = craftable;
		totalProgressTicks = Def.duration.EvaluateInt(craftable) + paramsData.PerksCraftAddTotalProgressTicksValue;
		isStarted = true;
		preOutputItems = (Def.isStarCraft ? Def.outputItems.MakePreOutput(craftable, (paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common) ? 1 : 0) : Def.outputItems.MakePreOutput(craftable));
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.CraftStart, craftId);
	}

	public virtual void Finish()
	{
		bool num = isStarted;
		isStarted = false;
		isFinished = true;
		if (num)
		{
			MainGame.Instance?.GameSave?.knowledgeSystem?.CompleteOneTimeCraft(Def);
		}
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.CraftFinish, craftId);
	}

	public virtual void Cancel()
	{
		isStarted = false;
	}

	public virtual void Update(int deltaTicks)
	{
		if (deltaTicks == 0 && (Def.isStarCraft || paramsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing || Def.isAutopsyCraft) && (craftable.CraftableAttachedWorker == null || (craftable.CraftableAttachedWorker != null && !(craftable.CraftableAttachedWorker is ZombieWgoData))))
		{
			currentProgressTicks++;
			failedProgressTicks++;
			NotifyProgressChanged();
		}
		else
		{
			currentProgressTicks += deltaTicks;
			succeededProgressTicks += deltaTicks;
			craftable.OnSuccessfulTicksChange(succeededProgressTicks - deltaTicks, succeededProgressTicks, this);
			NotifyProgressChanged();
		}
	}

	public virtual List<Item> MakeOutput()
	{
		List<Item> list = OutputItems.MakeOutput(preOutputItems);
		list.AddRange(customCraftOutput);
		return list;
	}

	public virtual void UpdateActualOutputBeforeFinish()
	{
		if (!isFinishOutputUpdated)
		{
			UpdateTotalParamValue();
			preOutputItems = (Def.isStarCraft ? Def.outputItems.MakePreOutput(craftable, (paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common) ? paramsData.total : 0f) : Def.outputItems.MakePreOutput(craftable));
			preToWgoOnFinishItems = (Def.isStarCraft ? Def.addItemsToWgoOnFinish.MakePreOutput(craftable, (paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common) ? paramsData.total : 0f) : Def.addItemsToWgoOnFinish.MakePreOutput(craftable));
			isFinishOutputUpdated = true;
		}
	}

	public virtual void Clear()
	{
		craftId = null;
		def = null;
		isStarted = false;
		isPaused = false;
		currentProgressTicks = 0;
		failedProgressTicks = 0;
		succeededProgressTicks = 0;
		paramsData = null;
		craftInput.Clear();
	}

	public virtual void RemoveCraftRequirements(ICraftable craftable)
	{
		craftInput = craftable.GetCraftableMultiInventory().RemoveItems(requirements, craftable as WgoData);
	}

	public virtual CraftStatus CheckWorkerDependentValues(IWorker worker, float deltaTime = 1f, bool skipEnergyCheck = false, bool skipInsanityCheck = false)
	{
		return CraftStatus.OK;
	}

	public virtual int GetCurrentQuality()
	{
		return -1;
	}

	public virtual void UpdateCountOnFinish()
	{
		Count--;
	}

	public void SetCustomOutputItems(List<Item> outputItems)
	{
		customCraftOutput = outputItems;
	}

	public void SetCustomItems(List<Item> items)
	{
		customItems = items;
	}

	public void UpdateTotalParamValue()
	{
		if (Def.isStarCraft && paramsData.craftParamsType == CraftParamsData.CraftParamsType.Common)
		{
			int num = 0;
			if (succeededProgressTicks >= ((CraftDef)Def).goldLevel)
			{
				num = 3;
			}
			else if (succeededProgressTicks >= ((CraftDef)Def).silverLevel)
			{
				num = 2;
			}
			else if (succeededProgressTicks >= ((CraftDef)Def).bronzeLevel)
			{
				num = 1;
			}
			paramsData.total = num;
		}
	}

	protected virtual CraftDefBase GetCraftDef()
	{
		def = GameBalance.GetCraftDefBase(craftId);
		return def;
	}

	protected void NotifyProgressChanged()
	{
		this.OnProgressChanged?.Invoke();
	}
}
