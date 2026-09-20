using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[Serializable]
public class CraftComponent : IComponent
{
	public delegate void DelCraftAddedToQueue(CraftElementBase craftQueueElement);

	public delegate void DelCraftRemovedFromQueue(CraftElementBase craftQueueElement);

	public const int MAX_QUEUE_CRAFTS = 999;

	public const int MIN_QUEUE_CRAFTS = 1;

	public const int MIN_QUEUE_CRAFTS_NON_STARTED = 0;

	private const float RESTART_CRAFT_FROM_QUEUE_DELAY_TIME = 1f;

	private const float CRAFT_PROGRESS_EPSILON = 0.001f;

	private const float FINISH_HELD_TIME = 0.6f;

	private const int GARDEN_MAX_PROGRESS_TICKS = 3;

	[SerializeField]
	private List<CraftElementBase> craftElementsQueue = new List<CraftElementBase>();

	[SerializeField]
	private int curCraftQueueIdx = -1;

	[SerializeField]
	private int prevQueueCount;

	[SerializeField]
	private float restartQueueTimer;

	[SerializeField]
	private CraftComponentStatus status;

	[SerializeField]
	private float autoCraftTickDuration;

	[SerializeField]
	private float currentAutoCraftTickTime;

	[SerializeField]
	private int zombieSubTicks;

	[SerializeField]
	private bool hasPreFinishUpdate;

	[SerializeField]
	private float finishHeldTimer;

	private int preFinishHoldCount;

	[SerializeField]
	private CraftElementBase lastStartedCraftWithRequirements;

	[NonSerialized]
	private ICraftable craftableObject;

	[NonSerialized]
	private List<CraftDefBase> craftsFromBalance;

	private bool isRemovingDestroyCraft;

	public int ZombieSubTicks
	{
		get
		{
			return zombieSubTicks;
		}
		set
		{
			zombieSubTicks = value;
			this.OnZombieSubTicksChanged?.Invoke(value);
		}
	}

	public float AutoCraftTickProgressNormalized
	{
		get
		{
			float num = ((autoCraftTickDuration > 0f) ? autoCraftTickDuration : 5f);
			return Mathf.Clamp01(currentAutoCraftTickTime / num);
		}
	}

	public List<CraftDefBase> AvailableCrafts
	{
		get
		{
			List<CraftDefBase> list = new List<CraftDefBase>();
			if (craftsFromBalance == null && GameBalance.Me.craftsInCache.TryGetValue(craftableObject.CraftableObjectId, out var value))
			{
				craftsFromBalance = value;
			}
			if (craftsFromBalance == null)
			{
				return new List<CraftDefBase>();
			}
			foreach (CraftDefBase item in craftsFromBalance)
			{
				if (!IsCraftHiddenByKnowledge(item) && (!(item is CraftDef { isNeedsUnlock: not false } craftDef) || MainGame.Instance.GameSave.knowledgeSystem.unlockedCrafts.Contains(craftDef.id)))
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public List<CraftDefBase> CraftsIn
	{
		get
		{
			List<CraftDefBase> list = new List<CraftDefBase>();
			if (craftsFromBalance == null && GameBalance.Me.craftsInCache.TryGetValue(craftableObject.CraftableObjectId, out var value))
			{
				craftsFromBalance = value;
			}
			if (craftsFromBalance == null)
			{
				return new List<CraftDefBase>();
			}
			foreach (CraftDefBase item in craftsFromBalance)
			{
				if (!IsCraftHiddenByKnowledge(item))
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	public bool IsStarted
	{
		get
		{
			if (status != CraftComponentStatus.Started)
			{
				return status == CraftComponentStatus.FinishDelayed;
			}
			return true;
		}
	}

	public bool IsQueueDelayed => status == CraftComponentStatus.QueueDelayed;

	public bool IsFinishDelayed => status == CraftComponentStatus.FinishDelayed;

	public bool HasPreFinishUpdate => hasPreFinishUpdate;

	public bool IsPreFinishHeld => preFinishHoldCount > 0;

	public CraftComponentStatus Status
	{
		get
		{
			return status;
		}
		set
		{
			CraftComponentStatus num = status;
			status = value;
			if (num != status)
			{
				this.OnStatusChanged?.Invoke(status);
			}
		}
	}

	public ICraftable CraftableObject => craftableObject;

	public bool HasCraftsByBalance
	{
		get
		{
			if (AvailableCrafts != null)
			{
				return AvailableCrafts.Count > 0;
			}
			return false;
		}
	}

	public CraftElementBase CurrentCraftElement
	{
		get
		{
			if (curCraftQueueIdx < 0 || curCraftQueueIdx >= craftElementsQueue.Count)
			{
				return null;
			}
			return craftElementsQueue[curCraftQueueIdx];
		}
	}

	public bool HasCraftsInQueue => craftElementsQueue.Count > 0;

	public bool IsAutoCraftable => CurrentCraftElement?.Def.isAuto ?? false;

	public bool IsManualActualCraftable => !IsAutoCraftable;

	public List<CraftElementBase> CraftElementsQueue => craftElementsQueue;

	public CraftElementBase LastStartedCraftWithRequirements
	{
		get
		{
			return lastStartedCraftWithRequirements;
		}
		set
		{
			lastStartedCraftWithRequirements = value;
		}
	}

	public bool IsDestroyingCraftActive
	{
		get
		{
			if (CurrentCraftElement != null && CurrentCraftElement is CraftElement)
			{
				return ((CraftElement)CurrentCraftElement).Definition.isObjDestroyCraft;
			}
			return false;
		}
	}

	public bool IsRemovingDestroyCraft => isRemovingDestroyCraft;

	public event Action OnCraftStart;

	public event Action OnCraftFinish;

	public event Action OnPreFinishHoldReleased;

	public event Action<CraftComponentStatus> OnStatusChanged;

	public event Action<float> OnCraftCurProgressNormalizedChanged;

	public event DelCraftAddedToQueue OnCraftAddedToQueue;

	public event DelCraftRemovedFromQueue OnCraftRemovedFromQueue;

	public event Action OnCurCraftIndexUpdate;

	public event Action<int> OnZombieSubTicksChanged;

	private static bool IsCraftHiddenByKnowledge(CraftDefBase craftDef)
	{
		KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
		if (knowledgeSystem.blackListCrafts.Contains(craftDef.id))
		{
			return true;
		}
		return knowledgeSystem.IsOneTimeCraftCompleted(craftDef);
	}

	public bool ShouldRegisterInCraftSystem()
	{
		if (!HasCraftsInQueue)
		{
			return false;
		}
		if (craftableObject != null && craftableObject.CraftableType == CraftableType.ConveyorWorkbench)
		{
			return IsDestroyingCraftActive;
		}
		return true;
	}

	public void Init(ICraftable craftableObject)
	{
		craftsFromBalance = null;
		this.craftableObject = craftableObject;
		autoCraftTickDuration = craftableObject.AutoCraftTickDuration;
		for (int i = 0; i < craftElementsQueue.Count; i++)
		{
			craftElementsQueue[i]?.BindCraftable(craftableObject);
		}
		lastStartedCraftWithRequirements?.BindCraftable(craftableObject);
		Init_Runtime(craftableObject);
	}

	private void Init_Runtime(ICraftable craftableObject)
	{
		if (GameBalance.Me.craftsInCache.TryGetValue(this.craftableObject.CraftableObjectId, out var value))
		{
			craftsFromBalance = value;
		}
	}

	public void ResetCraftsFromBalanceCache()
	{
		craftsFromBalance = null;
	}

	public bool TryStartCraft(CraftElementBase craftElement)
	{
		if (!IsStarted && GetStartCraftStatus(craftElement) == CraftStatus.OK)
		{
			AddToQueue(craftElement);
			TryContinueFromQueue();
			return true;
		}
		return false;
	}

	public void AddCraftNoStart(CraftElementBase craftElement)
	{
		if (CurrentCraftElement == null || !CurrentCraftElement.IsStarted)
		{
			AddToQueue(craftElement);
		}
	}

	public void RemoveCurNotStartedCraft()
	{
		RemoveFromQueue(CurrentCraftElement);
	}

	public void TryStartCurCraft()
	{
		if (CurrentCraftElement == null || !CurrentCraftElement.IsStarted)
		{
			TryContinueFromQueue();
		}
	}

	public void TryFinishCurCraft()
	{
		if (CurrentCraftElement != null)
		{
			Finish();
		}
	}

	public void ContinueAutoCraft()
	{
		Finish();
	}

	public void AddDestroyCraft(CraftElement craftElement)
	{
		if (CraftableObject.CraftableAttachedWorker is ZombieWgoData zombieWgoData)
		{
			MainGame.Instance.dropSystem.DropItem(zombieWgoData.ZombieItem, zombieWgoData.AttachedWgoData.WorldId, zombieWgoData.AttachedWgoData.GetDropPos(zombieWgoData.ZombieItem));
			zombieWgoData.UnAttachFromWgoData();
			MainGame.ZombieSystemData.PutZombieFromGameSceneToStore(zombieWgoData);
		}
		AddToQueue(craftElement, addToQueueTop: true);
		TryContinueFromQueue();
		if (CraftableObject.CraftableType == CraftableType.ConveyorWorkbench)
		{
			MainGame.Instance.craftSystem.AddCraftObject(this);
		}
		this.OnStatusChanged?.Invoke(status);
	}

	public void RemoveDestroyCraft()
	{
		if (((CraftElement)CurrentCraftElement).Definition.isObjDestroyCraft)
		{
			isRemovingDestroyCraft = true;
			if (CraftableObject.CraftableType == CraftableType.ConveyorWorkbench)
			{
				MainGame.Instance.craftSystem.RemoveCraftObject(this);
			}
			RemoveFromQueue(CurrentCraftElement, removeEvenIfStarted: true);
			TryContinueFromQueue();
			if (CurrentCraftElement != null && CurrentCraftElement.IsPreFinishUpdated)
			{
				finishHeldTimer = 0.6f;
				Status = CurrentCraftElement.PrevCraftComponentStatus;
			}
			this.OnStatusChanged?.Invoke(status);
			isRemovingDestroyCraft = false;
		}
	}

	public CraftElementBase AddToQueue(CraftElementBase craftElement, bool addToQueueTop = false, int queueIdx = -1)
	{
		if (craftElement.Def.IsOneTimeCraft())
		{
			if (MainGame.Instance.GameSave.knowledgeSystem.IsOneTimeCraftCompleted(craftElement.Def))
			{
				return craftElement;
			}
			CraftElementBase craftElementBase = craftElementsQueue.Find((CraftElementBase x) => x.CraftId == craftElement.CraftId);
			if (craftElementBase != null && CraftDefExtensions.ShouldSkipDuplicateOneTimeCraft(craftElement.IsStarted, craftElementBase.IsStarted))
			{
				return craftElementBase;
			}
		}
		craftElement.BindCraftable(craftableObject);
		CraftElementBase craftElementBase2 = null;
		if (craftElementsQueue.Count > 0)
		{
			craftElementBase2 = (CurrentCraftElement.IsStarted ? CurrentCraftElement : null);
			List<CraftElementBase> list = craftElementsQueue;
			CraftElementBase craftElementBase3 = list[list.Count - 1];
			if (!addToQueueTop && craftElementBase3.TryMergeWith(craftElement))
			{
				return craftElementBase3;
			}
		}
		craftElement.CraftStatus = GetStartCraftStatus(craftElement);
		if (!addToQueueTop)
		{
			if (queueIdx == -1)
			{
				craftElementsQueue.Add(craftElement);
			}
			else
			{
				craftElementsQueue.Insert(queueIdx, craftElement);
			}
		}
		else
		{
			int index = ((craftElementBase2 != null && craftElementBase2.CraftId == craftElement.CraftId) ? 1 : 0);
			craftElementsQueue.Insert(index, craftElement);
		}
		UpdateQueue();
		craftableObject.OnAddToQueue(craftElement);
		this.OnCraftAddedToQueue?.Invoke(craftElement);
		return craftElement;
	}

	public void RemoveFromQueue(CraftElementBase craftElement, bool removeEvenIfStarted = false)
	{
		if ((CurrentCraftElement == null || craftElement != CurrentCraftElement || !CurrentCraftElement.IsStarted || removeEvenIfStarted) && craftElementsQueue.Remove(craftElement))
		{
			UpdateQueue();
			this.OnCraftRemovedFromQueue?.Invoke(craftElement);
		}
	}

	public void Update(float deltaTime)
	{
		CraftComponentStatus craftComponentStatus = status;
		if (craftComponentStatus == CraftComponentStatus.FinishDelayed || craftComponentStatus == CraftComponentStatus.ReadyToFinishAutoCraft || craftComponentStatus == CraftComponentStatus.WaitingForWorkerPickUp || (IsAutoCraftable && CraftableObject.CraftableAttachedWorker is ZombieWgoData { CrafterCurrentOrder: not null }))
		{
			return;
		}
		if (IsQueueDelayed || status == CraftComponentStatus.ReadyToStartCraft)
		{
			restartQueueTimer += deltaTime;
			if (restartQueueTimer >= 1f)
			{
				restartQueueTimer = 0f;
				TryContinueFromQueue();
			}
			return;
		}
		CraftElementBase currentCraftElement = CurrentCraftElement;
		if (currentCraftElement == null || !currentCraftElement.IsStarted)
		{
			return;
		}
		if (CurrentCraftElement.ProgressTicks < CurrentCraftElement.TotalProgressTicks)
		{
			currentAutoCraftTickTime += deltaTime;
			float num = autoCraftTickDuration;
			if (num <= 0f)
			{
				num = 5f;
			}
			if (currentAutoCraftTickTime.EqualsOrMore(num))
			{
				int num2 = Mathf.FloorToInt(currentAutoCraftTickTime / num);
				if (num2 + currentCraftElement.ProgressTicks > CurrentCraftElement.TotalProgressTicks)
				{
					num2 = CurrentCraftElement.TotalProgressTicks - currentCraftElement.ProgressTicks;
				}
				currentAutoCraftTickTime %= num;
				if (currentCraftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing)
				{
					UpdateGardenGrowingCraft(num2, currentCraftElement);
				}
				else
				{
					currentCraftElement.Update(num2);
				}
				this.OnCraftCurProgressNormalizedChanged?.Invoke(currentCraftElement.ProgressTimeNormalized);
			}
		}
		TrySetPreFinishState();
	}

	public void UpdateManual(int deltaTicks)
	{
		if (IsFinishDelayed || status == CraftComponentStatus.WaitingForWorkerPickUp || status == CraftComponentStatus.WaitingForOutputDrop)
		{
			return;
		}
		if (IsQueueDelayed || status == CraftComponentStatus.ReadyToStartCraft)
		{
			TryContinueFromQueue();
			return;
		}
		CraftElementBase currentCraftElement = CurrentCraftElement;
		if (currentCraftElement != null && currentCraftElement.IsStarted)
		{
			if (deltaTicks + currentCraftElement.ProgressTicks > CurrentCraftElement.TotalProgressTicks)
			{
				deltaTicks = CurrentCraftElement.TotalProgressTicks - currentCraftElement.ProgressTicks;
			}
			if (CurrentCraftElement.ProgressTicks < CurrentCraftElement.TotalProgressTicks)
			{
				CurrentCraftElement.Update(deltaTicks);
			}
			TrySetPreFinishState();
			this.OnCraftCurProgressNormalizedChanged?.Invoke(CurrentCraftElement.ProgressTimeNormalized);
		}
	}

	public void Cancel()
	{
		if (!(CurrentCraftElement.Def is CraftDef { replaceWgoId: "0" }))
		{
			DropItems(CurrentCraftElement.CraftInput);
			CraftElementBase currentCraftElement = CurrentCraftElement;
			CurrentCraftElement.Cancel();
			craftableObject.OnCraftCancel(currentCraftElement);
			Status = CraftComponentStatus.Canceled;
		}
	}

	public void PreFinishUpdate(float deltaTime)
	{
		if (IsPreFinishHeld)
		{
			return;
		}
		finishHeldTimer += deltaTime;
		if (!finishHeldTimer.EqualsOrMore(0.6f))
		{
			return;
		}
		CraftElementBase currentCraftElement = CurrentCraftElement;
		currentCraftElement.BindCraftable(craftableObject);
		currentCraftElement.IsPreFinishUpdated = true;
		hasPreFinishUpdate = false;
		currentCraftElement.UpdateActualOutputBeforeFinish();
		if (currentCraftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenGrowing)
		{
			Finish();
		}
		else if (CraftableObject.CraftableAttachedWorker is ZombieWgoData { ZombieType: var zombieType } zombieWgoData)
		{
			switch (zombieType)
			{
			case ZombieType.Crafter:
			{
				HandleOutput(currentCraftElement, out var isZombieCrafterMadeDrop);
				craftableObject.OnCraftEnd(currentCraftElement);
				Status = CraftComponentStatus.WaitingForWorkerPickUp;
				currentCraftElement.PrevCraftComponentStatus = Status;
				if (isZombieCrafterMadeDrop && zombieWgoData.WorldZoneData.FindOrdersByTarget(zombieWgoData.UniqueId, typeof(PickupOrder)).Count == 0)
				{
					zombieWgoData.CrafterFinishAndContinueAfterBigItemDropped();
				}
				break;
			}
			case ZombieType.ConveyorCrafter:
				HandleOutputConveyor(currentCraftElement);
				if (currentCraftElement.CaBeFinished)
				{
					craftableObject.OnCraftEnd(currentCraftElement);
				}
				Status = CraftComponentStatus.WaitingForOutputDrop;
				currentCraftElement.PrevCraftComponentStatus = Status;
				break;
			case ZombieType.Gardener:
				Finish();
				break;
			}
		}
		else if (currentCraftElement.Def.isAuto && !currentCraftElement.Def.isHidden && !currentCraftElement.Def.autoFinishAutoCraft)
		{
			Status = CraftComponentStatus.ReadyToFinishAutoCraft;
			currentCraftElement.PrevCraftComponentStatus = Status;
		}
		else if (currentCraftElement.CanFinishCraft(craftableObject) == CraftStatus.OK)
		{
			Finish();
		}
		else
		{
			Status = CraftComponentStatus.FinishDelayed;
			currentCraftElement.PrevCraftComponentStatus = Status;
		}
	}

	public void AddPreFinishHold()
	{
		preFinishHoldCount++;
		finishHeldTimer = 0f;
	}

	public void ReleasePreFinishHold()
	{
		if (preFinishHoldCount <= 0)
		{
			return;
		}
		preFinishHoldCount--;
		if (preFinishHoldCount <= 0)
		{
			if (hasPreFinishUpdate)
			{
				finishHeldTimer = 0.6f;
			}
			this.OnPreFinishHoldReleased?.Invoke();
		}
	}

	public void Clear()
	{
		CurrentCraftElement?.Cancel();
		craftElementsQueue.Clear();
		UpdateQueue();
	}

	public void UpdateQueueElementsCraftStatus()
	{
		MultiInventory multiInventory = null;
		bool flag = false;
		foreach (CraftElementBase item in craftElementsQueue)
		{
			if (!item.IsStarted)
			{
				if (multiInventory == null)
				{
					multiInventory = craftableObject.GetCraftableMultiInventory();
				}
				item.CraftStatus = GetStartCraftStatus(item, multiInventory);
				flag = true;
			}
			else
			{
				item.CraftStatus = item.CanFinishCraft(craftableObject);
			}
		}
		if (flag)
		{
			TrySetIdxCurrentCraftFromQueue(useCachedCraftStatuses: true);
		}
	}

	public void UpdateCanContinueManualCraftState(float deltaTime = 1f)
	{
		if (IsStarted && !CurrentCraftElement.Def.isAuto)
		{
			CurrentCraftElement.CraftStatus = CurrentCraftElement.CheckWorkerDependentValues(craftableObject.CraftableAttachedWorker, deltaTime);
		}
	}

	public CraftStatus GetStartCraftStatus(CraftElementBase craftElement, MultiInventory multiInventory = null)
	{
		if (!IsCraftAllowedByAttachedExtensions(craftElement))
		{
			return CraftStatus.NoExtension;
		}
		CraftStatus craftStatus = craftElement.CheckWorkerDependentValues(craftableObject.CraftableAttachedWorker, 1f, skipEnergyCheck: true, skipInsanityCheck: true);
		if (craftStatus != 0)
		{
			return craftStatus;
		}
		if (!craftElement.IsStarted)
		{
			return craftElement.CanStartCraft(craftableObject, multiInventory);
		}
		return craftStatus;
	}

	public bool IsCraftAllowedByAttachedExtensions(CraftDefBase craftDef)
	{
		if (craftDef == null)
		{
			return false;
		}
		return IsCraftAllowedByAttachedExtensionsImpl(craftDef);
	}

	public void ProcessInstantCraft(ICraftable craftable, CraftElement craftElement)
	{
		while (craftElement.Count > 0)
		{
			craftElement.DoBeforeStartCalculations(craftable);
			RemoveRequirements(craftElement);
			craftElement.Start(craftable);
			craftableObject.OnCraftStart(craftElement);
			this.OnCraftStart?.Invoke();
			craftElement.Finish();
			HandleOutput(craftElement, out var _);
			craftableObject.OnCraftEnd(craftElement);
			this.OnCraftFinish?.Invoke();
			craftElement.Count--;
			if (GetStartCraftStatus(craftElement) != 0)
			{
				break;
			}
		}
	}

	private void TrySetPreFinishState()
	{
		if (CurrentCraftElement.ProgressTicks >= CurrentCraftElement.TotalProgressTicks && !hasPreFinishUpdate)
		{
			hasPreFinishUpdate = true;
			finishHeldTimer = 0f;
		}
	}

	private void DropItems(List<Item> items)
	{
		foreach (Item item in items)
		{
			craftableObject.MakeDrop(item);
		}
	}

	[CanBeNull]
	private CraftElementBase Start(CraftElementBase craftElement)
	{
		if (craftElement.Count == 0)
		{
			return null;
		}
		craftElement.DoBeforeStartCalculations(craftableObject);
		if (CurrentCraftElement != null && craftElement != CurrentCraftElement && !CurrentCraftElement.IsStarted)
		{
			CurrentCraftElement.Finish();
		}
		CraftElementBase craftElementBase = null;
		if (!craftElement.IsStarted)
		{
			craftElementBase = craftElement.Clone();
			if (!craftElement.IsInfinite)
			{
				craftElement.UpdateCountOnFinish();
			}
			RemoveRequirements(craftElementBase);
			autoCraftTickDuration = craftableObject.AutoCraftTickDuration;
			craftElementBase.Start(craftableObject);
			craftableObject.OnCraftStart(craftElementBase);
			if (craftElementBase.Requirements.Count != 0)
			{
				lastStartedCraftWithRequirements = craftElementBase;
			}
		}
		this.OnCraftStart?.Invoke();
		Status = CraftComponentStatus.Started;
		return craftElementBase;
	}

	private void Finish()
	{
		CraftElementBase currentCraftElement = CurrentCraftElement;
		currentCraftElement.UpdateCountOnFinish();
		currentAutoCraftTickTime = 0f;
		zombieSubTicks = 0;
		CurrentCraftElement.Finish();
		if (status != CraftComponentStatus.WaitingForWorkerPickUp && status != CraftComponentStatus.WaitingForOutputDrop)
		{
			HandleOutput(currentCraftElement, out var _);
			if (currentCraftElement.Def is CraftDef { isObjDestroyCraft: not false })
			{
				for (int i = 1; i < craftElementsQueue.Count; i++)
				{
					CraftElementBase craftElementBase = craftElementsQueue[i];
					if (craftElementBase.IsStarted || craftElementBase.CraftInput.Count <= 0)
					{
						continue;
					}
					foreach (NeedItemData requirement in craftElementBase.Requirements)
					{
						if (!requirement.ItemDef.isFuel)
						{
							craftableObject.MakeDrop(new Item(requirement.id, requirement.GetCount(craftableObject as WgoData)));
						}
					}
				}
			}
			craftableObject.OnCraftEnd(currentCraftElement);
		}
		this.OnCraftFinish?.Invoke();
		UpdateQueue();
		if (craftElementsQueue.Count > 0)
		{
			TryContinueFromQueue();
		}
		else
		{
			Status = CraftComponentStatus.Finished;
		}
	}

	private void HandleOutput(CraftElementBase craftElementBase, out bool isZombieCrafterMadeDrop)
	{
		isZombieCrafterMadeDrop = false;
		if (craftElementBase.ParamsData.customRes.GetInt("ignore_handle_output") == 1)
		{
			return;
		}
		List<Item> list = craftElementBase.MakeOutput();
		if (CraftableObject.CraftableAttachedWorker != null && CraftableObject.CraftableAttachedWorker is ZombieWgoData zombieWgoData)
		{
			if (zombieWgoData.ZombieType == ZombieType.Crafter)
			{
				foreach (Item item in list)
				{
					if (item.Definition.itemGroupIds.Contains("town_box"))
					{
						isZombieCrafterMadeDrop = true;
						craftableObject.MakeDrop(item);
					}
					else
					{
						zombieWgoData.CrafterAddCraftDrop(item);
					}
				}
				return;
			}
			if (zombieWgoData.ZombieType == ZombieType.ConveyorCrafter)
			{
				CraftableObject.CraftableObjectCraftInventory.AddItemsToInventory(list);
			}
		}
		else if (craftElementBase.Def.isAuto && CraftableObject.CraftableObjectCraftInventory.Data != null && !CraftableObject.CraftableObjectCraftInventory.Data.IsEmpty)
		{
			CraftableObject.CraftableObjectCraftInventory.AddItemsToInventory(list);
		}
		else
		{
			DropItems(list);
		}
	}

	private void HandleOutputConveyor(CraftElementBase craftElementBase)
	{
		List<Item> items = craftElementBase.MakeOutput();
		CraftableObject.CraftableObjectCraftInventory.AddItemsToInventory(items);
	}

	private void RemoveRequirements(CraftElementBase craftElement)
	{
		craftElement.RemoveCraftRequirements(craftableObject);
		if (craftElement.Def.needItemsFromWgo.Count != 0)
		{
			craftableObject.CraftableObjectInventory.RemoveItems(CurrentCraftElement.Def.needItemsFromWgo);
		}
	}

	public bool TryContinueFromQueue()
	{
		RemoveUnavailableExtensionCraftsFromQueue();
		UpdateQueueElementsCraftStatus();
		bool flag = CraftableObject.CraftableAttachedWorker is ZombieWgoData zombieWgoData && zombieWgoData.CrafterCurrentOrder != null;
		if (CurrentCraftElement != null && CurrentCraftElement.CraftStatus == CraftStatus.OK && !flag)
		{
			CraftElementBase currentCraftElement = CurrentCraftElement;
			CraftElementBase craftElementBase = Start(currentCraftElement);
			curCraftQueueIdx = 0;
			if (craftElementBase != null)
			{
				AddToQueue(craftElementBase, addToQueueTop: true);
			}
			else
			{
				ReplaceElementToTop(currentCraftElement);
			}
			this.OnCraftCurProgressNormalizedChanged?.Invoke(CurrentCraftElement.ProgressTimeNormalized);
			return true;
		}
		if (craftElementsQueue.Count > 0)
		{
			curCraftQueueIdx = 0;
			Status = CraftComponentStatus.QueueDelayed;
		}
		return false;
	}

	private bool RemoveUnavailableExtensionCraftsFromQueue()
	{
		bool flag = false;
		for (int num = craftElementsQueue.Count - 1; num >= 0; num--)
		{
			CraftElementBase craftElementBase = craftElementsQueue[num];
			if (!craftElementBase.IsStarted && !IsCraftAllowedByAttachedExtensions(craftElementBase))
			{
				craftElementsQueue.RemoveAt(num);
				this.OnCraftRemovedFromQueue?.Invoke(craftElementBase);
				flag = true;
			}
		}
		if (flag)
		{
			UpdateQueue();
		}
		return flag;
	}

	private bool IsCraftAllowedByAttachedExtensions(CraftElementBase craftElement)
	{
		if (craftElement?.Def == null)
		{
			return false;
		}
		return IsCraftAllowedByAttachedExtensionsImpl(craftElement.Def);
	}

	private bool IsCraftAllowedByAttachedExtensionsImpl(CraftDefBase craftDef)
	{
		if (!(craftableObject is WgoData wgoData))
		{
			return true;
		}
		if (string.IsNullOrEmpty(craftDef.extensionNeedId))
		{
			return true;
		}
		List<string> attachedWorkbenchExtensionIds = wgoData.Definition.attachedWorkbenchExtensionIds;
		if (wgoData.AttachedWorkbenchExtensions.Count > 0 && !attachedWorkbenchExtensionIds.Contains(craftDef.extensionNeedId))
		{
			return false;
		}
		foreach (SGuid attachedWorkbenchExtension in wgoData.AttachedWorkbenchExtensions)
		{
			WgoData wgoData2 = MainGame.Instance.GameSave.WorldData.GetWgoData(attachedWorkbenchExtension);
			if (wgoData2 != null && attachedWorkbenchExtensionIds.Contains(wgoData2.id) && wgoData2.id == craftDef.extensionNeedId)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateQueue()
	{
		CraftComponentStatus craftComponentStatus = status;
		if (prevQueueCount <= 0 && craftElementsQueue.Count > 0)
		{
			prevQueueCount = craftElementsQueue.Count;
			if (CraftableObject.CraftableType == CraftableType.Regular)
			{
				MainGame.Instance.craftSystem.AddCraftObject(this);
			}
			craftComponentStatus = CraftComponentStatus.ReadyToStartCraft;
		}
		curCraftQueueIdx = ((craftElementsQueue.Count == 0) ? (-1) : Mathf.Clamp(curCraftQueueIdx, 0, craftElementsQueue.Count - 1));
		if (curCraftQueueIdx == -1 && craftElementsQueue.Count > 0)
		{
			curCraftQueueIdx = 0;
		}
		for (int i = 0; i < craftElementsQueue.Count; i++)
		{
			CraftElementBase craftElementBase = craftElementsQueue[i];
			if (craftElementBase.Count <= 0)
			{
				craftElementsQueue.RemoveAt(i);
				this.OnCraftRemovedFromQueue?.Invoke(craftElementBase);
				i--;
			}
		}
		curCraftQueueIdx = ((craftElementsQueue.Count == 0) ? (-1) : Mathf.Clamp(curCraftQueueIdx, 0, craftElementsQueue.Count - 1));
		if (craftElementsQueue.Count == 0)
		{
			curCraftQueueIdx = -1;
			prevQueueCount = 0;
			MainGame.Instance.craftSystem.RemoveCraftObject(this);
			craftComponentStatus = CraftComponentStatus.Finished;
		}
		TrySetIdxCurrentCraftFromQueue();
		Status = craftComponentStatus;
	}

	private void TrySetIdxCurrentCraftFromQueue(bool useCachedCraftStatuses = false)
	{
		if (craftElementsQueue.Count == 0 || (CurrentCraftElement != null && CurrentCraftElement.IsStarted))
		{
			return;
		}
		curCraftQueueIdx = 0;
		for (int i = 0; i < craftElementsQueue.Count; i++)
		{
			CraftElementBase craftElementBase = craftElementsQueue[i];
			if ((useCachedCraftStatuses ? craftElementBase.CraftStatus : GetStartCraftStatus(craftElementBase)) == CraftStatus.OK)
			{
				curCraftQueueIdx = i;
				break;
			}
		}
		this.OnCurCraftIndexUpdate?.Invoke();
	}

	private void ReplaceElementToTop(CraftElementBase craftElement)
	{
		if (CraftElementsQueue.IndexOf(craftElement) != -1)
		{
			CraftElementsQueue.Remove(craftElement);
			CraftElementsQueue.Insert(0, craftElement);
			UpdateQueue();
		}
	}

	public bool TryElementUpToQueue(CraftElementBase craftElement)
	{
		int num = CraftElementsQueue.IndexOf(craftElement);
		if (num == 0)
		{
			return false;
		}
		CraftElementsQueue.Move(craftElement, num - 1);
		TrySetIdxCurrentCraftFromQueue();
		return true;
	}

	public bool TryElementDownToQueue(CraftElementBase craftElement)
	{
		int num = CraftElementsQueue.IndexOf(craftElement);
		if (num == CraftElementsQueue.Count - 1)
		{
			return false;
		}
		CraftElementsQueue.Move(craftElement, num + 1);
		TrySetIdxCurrentCraftFromQueue();
		return true;
	}

	public void UpdateGardenGrowingCraft(int ticks, CraftElementBase craftEl)
	{
		int num = 0;
		for (int i = 0; i < ticks; i++)
		{
			if (craftEl.ProgressTicks >= craftEl.TotalProgressTicks)
			{
				break;
			}
			float num2 = 100f / (float)craftEl.ParamsData.MasteryLock;
			if (craftEl.ParamsData.MasteryValue < craftEl.ParamsData.MasteryLock)
			{
				float num3 = num2 * (float)craftEl.ParamsData.MasteryValue;
				num = (((float)UnityEngine.Random.Range(1, 100) <= num3) ? 1 : 0);
			}
			else
			{
				num = Math.Clamp(craftEl.ParamsData.MasteryValue / craftEl.ParamsData.MasteryLock, 0, ConstDef.Get("max_cells_per_one_hit").IntValue);
			}
			num = Mathf.Clamp(num, 0, 3);
			if (num + craftEl.ProgressTicks > craftEl.TotalProgressTicks)
			{
				num = craftEl.TotalProgressTicks - craftEl.ProgressTicks;
			}
			craftEl.Update(num);
		}
	}
}
