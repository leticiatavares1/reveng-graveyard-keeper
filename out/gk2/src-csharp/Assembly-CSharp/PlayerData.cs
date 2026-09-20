using System;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PlayerData
{
	public VariableNotificator<Vector3> position = new VariableNotificator<Vector3>();

	[SerializeField]
	private VariableNotificator<Vector2> direction = new VariableNotificator<Vector2>();

	public VariableNotificator<AnimationState> charState = new VariableNotificator<AnimationState>();

	public Action<Item> OnOverheadItemAdded;

	public Action OnOverheadItemRemoved;

	public Inventory inventory;

	public Inventory toolBeltInventory;

	[SerializeField]
	[FormerlySerializedAs("overheadItem")]
	[PreviouslySerializedAs("overheadItem")]
	private Item overheadItemLegacy;

	[SerializeField]
	private List<Item> overheadItems = new List<Item>();

	public Item interactingItem;

	[SerializeField]
	private GameRes res = new GameRes();

	public EnergySystem energySystem = new EnergySystem();

	public StaminaSystem staminaSystem = new StaminaSystem();

	public string[] pinnedItems = new string[4];

	public PlayerCustomizationData customization = new PlayerCustomizationData();

	public string currentGameSceneId;

	public SermonResultData currentSermon;

	public bool isInTutorialMode;

	public List<string> tutorialModeExcludedList = new List<string>();

	public SGuid tutorialArrowWgoId;

	public bool isWispEnabled;

	public bool isDirectionLocked;

	public bool openedCraftWindowOnce;

	public bool interactedWithFishingReservoirOnce;

	public bool sawFightTutorialOnce;

	public bool sawInspirationTutorialOnce;

	public bool sawInspirationTalentsTutorialOnce;

	public bool interactedWithChalkBoardOnce;

	public bool openedGraveWindowOnce;

	public HPComponent hpComponent = new HPComponent(100);

	[SerializeField]
	private SGuid guid = new SGuid("49042eb8-eda7-4612-80c8-6fbfc39b52ac");

	[NonSerialized]
	private WorldZoneData currentWorldZoneData;

	[NonSerialized]
	public List<TownZone> insideTownZones = new List<TownZone>();

	[NonSerialized]
	public List<TownSubZone> insideTownSubZones = new List<TownSubZone>();

	[NonSerialized]
	public CharacterWindowData.CharPage lastOpenedPage = CharacterWindowData.CharPage.Main;

	public Vector2 Direction
	{
		get
		{
			return direction.Value;
		}
		set
		{
			if (!isDirectionLocked)
			{
				direction.Value = value;
			}
		}
	}

	public Item overheadItem
	{
		get
		{
			if (!HasOverheadItem)
			{
				return null;
			}
			return overheadItems[overheadItems.Count - 1];
		}
	}

	public IReadOnlyList<Item> OverheadItems
	{
		get
		{
			IReadOnlyList<Item> readOnlyList = overheadItems;
			return readOnlyList ?? Array.Empty<Item>();
		}
	}

	public int OverheadCount
	{
		get
		{
			if (overheadItems == null)
			{
				return 0;
			}
			return overheadItems.Count;
		}
	}

	public bool HasOverheadItem => OverheadCount > 0;

	public bool HasMultipleOverheadItems => OverheadCount > 1;

	public int ExtraOverhead => Mathf.Max(0, GetResInt("extra_overhead"));

	public int OverheadStackLimit => 1 + ExtraOverhead;

	public bool HasFreeOverheadSlot => OverheadCount < OverheadStackLimit;

	public bool HasInteractingItem
	{
		get
		{
			if (interactingItem != null)
			{
				return !interactingItem.IsEmpty;
			}
			return false;
		}
	}

	public Inventory Inventory => inventory;

	public WorldZoneData CurrentWorldZoneData => currentWorldZoneData;

	public SGuid Guid => guid;

	public event Action<List<Item>> OnDropCollected;

	public event Action OnPinnedItemsChanged;

	public event Action OnGameResChanged;

	public event Action<Item> OnItemUsed;

	public static event Action<string> OnReputationTechEnoughRep;

	public static PlayerData CreatePlayerData()
	{
		PlayerData playerData = new PlayerData();
		playerData.Init();
		return playerData;
	}

	public void PrepareForGame()
	{
		lastOpenedPage = CharacterWindowData.CharPage.Main;
		if (insideTownZones == null)
		{
			insideTownZones = new List<TownZone>();
		}
		if (insideTownSubZones == null)
		{
			insideTownSubZones = new List<TownSubZone>();
		}
		MigrateOverheadItems();
		InitGameResSystems();
	}

	public void UnPrepareFromGame()
	{
	}

	public void SetNPCRep(string repRes, int value)
	{
		res.Set(repRes, value);
		ValidateReputationTechs();
	}

	public void AddNPCRep(string repRes, int value)
	{
		res.Add(repRes, value);
		ValidateReputationTechs();
	}

	public int GetNPCRep(string repRes)
	{
		return res.GetInt(repRes);
	}

	private void ValidateReputationTechs()
	{
		foreach (TechDef techDef in GameBalance.Me.techDefs)
		{
			if (!MainGame.Instance.GameSave.knowledgeSystem.IsTechUnlocked(techDef.id) && (techDef.techDefType == TechDefType.CharRep || techDef.techDefType == TechDefType.DisRep) && techDef.TechState == TechState.Available && techDef.EnoughResources)
			{
				PlayerData.OnReputationTechEnoughRep?.Invoke(techDef.id);
			}
		}
	}

	public void EquipItem(Item item)
	{
		if (!toolBeltInventory.Data.TryGetItemInInventory(item.id, out var _))
		{
			toolBeltInventory.AddItemToInventory(item);
		}
		else
		{
			Debug.LogWarning($"Item [{item.id}] with type [{item.Definition.type}] was already equipped");
		}
	}

	public void UseItem(Item item)
	{
		if (item.Definition.CanBeUsed)
		{
			if (item.Definition.stayOnUse)
			{
				UseLogic();
			}
			else if (inventory.RemoveItemById(item.id, 1).Count > 0)
			{
				UseLogic();
			}
		}
		void UseLogic()
		{
			GameRes gameResOnUse = item.Definition.GetGameResOnUse();
			if (!gameResOnUse.IsEmpty())
			{
				foreach (GameResAtom item2 in gameResOnUse.List)
				{
					string type = item2.type;
					if (!(type == "energy"))
					{
						if (type == "insanity")
						{
							PlayerInsanityGameResSystem.GetSystem().Add(item2.value);
						}
						else
						{
							res.Add(gameResOnUse);
						}
					}
					else
					{
						PlayerEnergyGameResSystem.GetSystem().Add(item2.value);
					}
				}
			}
			if (!string.IsNullOrEmpty(item.Definition.onUseSound))
			{
				LazyAudio.PlayAndForget(item.Definition.onUseSound);
			}
			foreach (LazyExpression onUseExpression in item.Definition.onUseExpressions)
			{
				onUseExpression.Evaluate(item);
			}
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerUseItem, item.id);
			this.OnItemUsed?.Invoke(item);
		}
	}

	public void RemoveItem(Item item)
	{
		inventory.RemoveItemById(item.id, 1);
	}

	public void AddOverheadItem(Item item)
	{
		if (item != null && !item.IsEmpty)
		{
			EnsureOverheadItems();
			if (!HasFreeOverheadSlot)
			{
				DropOverheadItem();
			}
			if (HasInteractingItem)
			{
				RemoveInteractingItem();
			}
			Debug.Log($"AddOverheadItem item:[{item.id}] count:[{item.Count}]");
			overheadItems.Add(item);
			OnOverheadItemAdded?.Invoke(item);
			RefreshOverheadVisuals();
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.AddOverhead, item.id);
			PlayOverheadItemTakeSound(item);
		}
	}

	public bool TryAddOverheadItemNoReplace(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			return false;
		}
		if (!HasFreeOverheadSlot)
		{
			return false;
		}
		AddOverheadItem(item);
		return true;
	}

	public bool TryGetOverheadItem(Predicate<Item> match, out Item item)
	{
		item = null;
		if (match == null || overheadItems == null)
		{
			return false;
		}
		for (int num = overheadItems.Count - 1; num >= 0; num--)
		{
			Item item2 = overheadItems[num];
			if (item2 != null && !item2.IsEmpty && match(item2))
			{
				item = item2;
				return true;
			}
		}
		return false;
	}

	public void DropOverheadItem()
	{
		if (HasOverheadItem)
		{
			DropOverheadItem(overheadItem);
		}
	}

	public void DropOverheadItem(Item item)
	{
		if (item != null && !item.IsEmpty)
		{
			Debug.Log("DropOverheadItem:[" + item.id + "]");
			Vector3 foundDropPos;
			bool playerDropPosition = SpecialPhysicsCastUtils.GetPlayerDropPosition(position.Value, direction.Value, out foundDropPos);
			Debug.Log(string.Format("Player drop pos by [{0}] result: {1}, [{2}]", "SpecialPhysicsCastUtils", playerDropPosition ? "success" : "fail", foundDropPos));
			MainGame.Instance.dropSystem.DropItem(item, MainGame.PlayerData.currentGameSceneId, foundDropPos);
			RemoveOverheadItem(item);
		}
	}

	public void RemoveOverheadItem()
	{
		if (HasOverheadItem)
		{
			RemoveOverheadItem(overheadItem);
		}
	}

	public void RemoveOverheadItem(Item item)
	{
		EnsureOverheadItems();
		int num = IndexOfOverheadItem(item);
		if (num >= 0)
		{
			overheadItems.RemoveAt(num);
			OnOverheadItemRemoved?.Invoke();
			RefreshOverheadVisuals();
		}
	}

	public void InsertOverheadItemTo(WgoData wgoData)
	{
		InsertOverheadItemTo(wgoData, overheadItem);
	}

	public void InsertOverheadItemTo(WgoData wgoData, Item item)
	{
		if (wgoData != null && item != null && !item.IsEmpty)
		{
			string id = item.id;
			wgoData.Inventory.AddItemToInventory(item);
			RemoveOverheadItem(item);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerInsertOverheadToWgoAnItem, id);
		}
	}

	public void IncreaseOverheadStackLimit(int increaseValue)
	{
		if (increaseValue > 0)
		{
			AddRes("extra_overhead", increaseValue);
		}
	}

	public void SetOverheadStackLimit(int limit)
	{
		if (limit >= 1)
		{
			SetRes("extra_overhead", limit - 1);
		}
	}

	private void MigrateOverheadItems()
	{
		EnsureOverheadItems();
		if (overheadItemLegacy != null && !overheadItemLegacy.IsEmpty)
		{
			if (overheadItems.Count == 0)
			{
				overheadItems.Add(overheadItemLegacy);
			}
			overheadItemLegacy = null;
		}
	}

	private void EnsureOverheadItems()
	{
		if (overheadItems == null)
		{
			overheadItems = new List<Item>();
		}
	}

	private int IndexOfOverheadItem(Item item)
	{
		if (item == null || overheadItems == null)
		{
			return -1;
		}
		int num = overheadItems.LastIndexOf(item);
		if (num >= 0)
		{
			return num;
		}
		for (int num2 = overheadItems.Count - 1; num2 >= 0; num2--)
		{
			Item item2 = overheadItems[num2];
			if (item2 != null && item2.UniqueId.Equals(item.UniqueId) && item2.id == item.id)
			{
				return num2;
			}
		}
		return -1;
	}

	private void RefreshOverheadVisuals()
	{
		PlayerController playerController = MainGame.PlayerController;
		if (!(playerController == null))
		{
			if (!HasOverheadItem)
			{
				playerController.RemoveOverheadItem();
			}
			else
			{
				playerController.SetOverheadItems(overheadItems);
			}
		}
	}

	private void PlayOverheadItemTakeSound(Item item)
	{
		if (item.Definition.itemGroupIds.Contains("zombie"))
		{
			LazyAudio.PlayAndForget("oh_zombie_grab");
		}
		else if (item.Definition.itemGroupIds.Contains("corpse"))
		{
			LazyAudio.PlayAndForget("oh_corpse_grab");
		}
		else if (item.id == "wood")
		{
			LazyAudio.PlayAndForget("oh_wood_grab");
		}
	}

	public void SetInteractingItem(Item item)
	{
		if (LazySingleton<FightingGameController>.Instance.CurrentFightState == FightState.Disabled && !HasMultipleOverheadItems)
		{
			if (HasOverheadItem)
			{
				DropOverheadItem();
			}
			if (HasInteractingItem)
			{
				RemoveInteractingItem();
			}
			Debug.Log("#SetInteractingItem:[" + item.id + "]");
			interactingItem = item;
			MainGame.PlayerController.SetInteractingItem(item, inventory.Data.GetTotalCountInInventory(item.id));
			MainGame.PlayerController.PlayerInteractionComponent.ResetInteractionState();
		}
	}

	public void RemoveInteractingItem()
	{
		if (HasInteractingItem)
		{
			Debug.Log("RemoveInteractingItem:[" + interactingItem.id + "]");
			interactingItem = null;
			MainGame.PlayerController.RemoveInteractingItem();
		}
	}

	public void UpdateInteractingItem()
	{
		if (HasInteractingItem && inventory.Data.GetTotalCountInInventory(interactingItem.id) <= 0)
		{
			RemoveInteractingItem();
			MainGame.PlayerController.PlayerInteractionComponent.ResetInteractionState();
		}
		else
		{
			SetInteractingItem(interactingItem);
			MainGame.PlayerController.PlayerInteractionComponent.ResetInteractionState();
		}
	}

	public void SetCurrentWorldZoneData(WorldZoneData worldZoneData, Action listener)
	{
		if (currentWorldZoneData != null)
		{
			currentWorldZoneData.RemovePlayerData(this);
			currentWorldZoneData.OnWgoDataChanged -= listener;
		}
		currentWorldZoneData = worldZoneData;
		if (currentWorldZoneData != null)
		{
			currentWorldZoneData.AddPlayerData(this);
			currentWorldZoneData.OnWgoDataChanged += listener;
		}
	}

	public void AddTownZone(TownZone townZone)
	{
		if (!insideTownZones.Contains(townZone))
		{
			Debug.Log("#town_zone# AddTownZone:[" + townZone.name + "]");
			insideTownZones.Add(townZone);
		}
	}

	public void RemoveTownZone(TownZone townZone)
	{
		if (insideTownZones.Contains(townZone))
		{
			Debug.Log("#town_zone# RemoveTownZone:[" + townZone.name + "]");
			insideTownZones.Remove(townZone);
		}
	}

	public void AddTownSubZone(TownSubZone townSubZone)
	{
		if (!insideTownSubZones.Contains(townSubZone))
		{
			Debug.Log("#town_zone# AddTownSubZone:[" + townSubZone.id + "]");
			insideTownSubZones.Add(townSubZone);
		}
	}

	public void RemoveTownSubZone(TownSubZone townSubZone)
	{
		if (insideTownSubZones.Contains(townSubZone))
		{
			Debug.Log("#town_zone# RemoveTownSubZone:[" + townSubZone.id + "]");
			insideTownSubZones.Remove(townSubZone);
		}
	}

	public void TryApplyStartState()
	{
		StartReses startReses = StartReses.Load();
		if (!(startReses != null))
		{
			return;
		}
		foreach (StartReses.StartItemData startItem in startReses.startItems)
		{
			Item item = new Item(startItem.id, startItem.count);
			if (startItem.shouldBeEquipped)
			{
				EquipItem(item);
			}
			else
			{
				inventory.AddItemToInventory(item);
			}
		}
		res.Set(startReses.startGameRes);
	}

	public void CollectDrop(DropView dropView)
	{
		DropData data = dropView.Data;
		if (data.IsResDrop)
		{
			CollectResDrop(data);
		}
		else
		{
			if (data.Size == ItemSize.Big)
			{
				return;
			}
			if (inventory.Data.CanAddItemCountToInventory(data.Item) > 0)
			{
				inventory.AddItemToInventory(data.Item, out var addedItems);
				foreach (LazyExpression item in data.Item.Definition.onDropCollected)
				{
					item.Evaluate(addedItems[0]);
				}
				this.OnDropCollected?.Invoke(addedItems);
				Debug.Log($"Drop[{data.Id}] collected successfully, left count = [{data.Count}]");
				if (data.Count == 0)
				{
					MainGame.Instance.dropSystem.RemoveDrop(data, data.WorldId);
				}
				else
				{
					data.NotifyCountChanged();
				}
			}
			else
			{
				bool isPhysicDisabled = dropView.IsPhysicDisabled;
				dropView.StopMoving();
				dropView.DoKick(MainGame.PlayerController.transform, isPhysicDisabled);
				LazySingleton<UINotificator>.Instance.HandleInventoryFull();
			}
		}
	}

	public void CollectResDrop(DropData drop)
	{
		if (drop == null || !drop.IsResDrop || drop.IsRemoving)
		{
			return;
		}
		string text = drop.Item.id.Replace("game_res_", string.Empty);
		int count = drop.Item.Count;
		if (TechDef.FlyingReses.Contains(text))
		{
			Vector3 pos = ((MainGame.PlayerController != null) ? MainGame.PlayerController.transform.position : drop.Position);
			for (int i = 0; i < count; i++)
			{
				FlyingTechPoint.Drop(pos, text);
			}
			LazyAudio.Play("tech_point_collect");
		}
		else
		{
			res.Add(text, count);
		}
		foreach (LazyExpression item in drop.Item.Definition.onDropCollected)
		{
			item.Evaluate(drop.Item);
		}
		drop.Item.Count = 0;
		MainGame.Instance.dropSystem.RemoveDrop(drop, drop.WorldId);
		Debug.Log($"Drop RES[{drop.Id}] collected successfully, left count = [{drop.Count}]");
	}

	public void ApplyCustomization(PlayerCustomizationData customizationData)
	{
		customization = customizationData;
		PlayerSkinHelper.ApplySkin(customizationData, onlyForCustomizationCharacter: false);
		PlayerSkinHelper.ApplyPlayerColorsByData(customizationData, onlyForCustomizationCharacter: false);
	}

	private void Init()
	{
		inventory = new Inventory("inventory", 20);
		toolBeltInventory = new Inventory("toolBeltInventory", 14);
		toolBeltInventory.AddItemToInventory(new Item("hand_tool"));
		res.Set("money", 50f);
		res.Set("g_garden_fertilizer_slots", GameBalance.Me.GetData<ConstDef>("g_garden_fertilizer_slots").IntValue);
		res.Set("g_garden_farming_base", GameBalance.Me.GetData<ConstDef>("g_garden_farming_base").IntValue);
		res.Set("g_vineyard_farming_base", GameBalance.Me.GetData<ConstDef>("g_vineyard_farming_base").IntValue);
		res.Set("g_garden_autocraft_dec", GameBalance.Me.GetData<ConstDef>("g_garden_autocraft_dec").IntValue);
		GameResSystemDef data = GameBalance.Me.GetData<GameResSystemDef>("energy");
		GameResSystemDef data2 = GameBalance.Me.GetData<GameResSystemDef>("insanity");
		GameResSystemDef data3 = GameBalance.Me.GetData<GameResSystemDef>("tech_red");
		GameResSystemDef data4 = GameBalance.Me.GetData<GameResSystemDef>("tech_green");
		GameResSystemDef data5 = GameBalance.Me.GetData<GameResSystemDef>("tech_blue");
		GameResSystemDef data6 = GameBalance.Me.GetData<GameResSystemDef>("donkey_body_drop_chance");
		res.Set(data.ResId, data.start.EvaluateFloat());
		res.Set(data2.ResId, data2.start.EvaluateFloat());
		res.Set(data3.ResId, data3.start.EvaluateFloat());
		res.Set(data4.ResId, data4.start.EvaluateFloat());
		res.Set(data5.ResId, data5.start.EvaluateFloat());
		res.Set(data6.ResId, data6.start.EvaluateFloat());
	}

	private void InitGameResSystems()
	{
		Dictionary<string, GameResSystemBase> dictionary = new Dictionary<string, GameResSystemBase>();
		dictionary.Add("energy", new PlayerEnergyGameResSystem(GameBalance.Me.GetData<GameResSystemDef>("energy").ResId, res));
		dictionary.Add("insanity", new PlayerInsanityGameResSystem(GameBalance.Me.GetData<GameResSystemDef>("insanity").ResId, res));
		dictionary.Add("stamina", new PlayerStaminaGameResSystem(GameBalance.Me.GetData<GameResSystemDef>("stamina").ResId, res));
		dictionary.Add("money", new PlayerMoneyGameResSystem(GameBalance.Me.GetData<GameResSystemDef>("money").ResId, res));
		dictionary.Add("happiness", new PlayerHappinessGameResSystem(GameBalance.Me.GetData<GameResSystemDef>("happiness").ResId, res));
		foreach (WorldZoneDef worldZoneDef in GameBalance.Me.worldZoneDefs)
		{
			string text = "wz_" + worldZoneDef.id;
			dictionary.TryAdd(text, new WorldZoneQualitySystemGameResSystem(text, res));
		}
		for (int i = 0; i < GameBalance.Me.gameResSystemDefs.Count; i++)
		{
			dictionary.TryAdd(GameBalance.Me.gameResSystemDefs[i].ResId, new GK2GameResSystem(GameBalance.Me.gameResSystemDefs[i].ResId, res));
		}
		res.SetSystems(dictionary);
	}

	public void SetHotBarItemAtIndex(string equippedItem, int index)
	{
		for (int i = 0; i < pinnedItems.Length; i++)
		{
			if (pinnedItems[i] == equippedItem)
			{
				pinnedItems[i] = "";
			}
		}
		pinnedItems[index] = equippedItem;
		this.OnPinnedItemsChanged?.Invoke();
	}

	public void TryUseHotBarItem(string itemId)
	{
		if (string.IsNullOrEmpty(itemId) || !pinnedItems.Contains(itemId))
		{
			return;
		}
		Item itemById = inventory.GetItemById(itemId);
		if (itemById != null && !itemById.IsEmpty)
		{
			if (itemById.IsSeed || itemById.IsFertilizer)
			{
				SetInteractingItem(itemById);
			}
			else if (itemById.Definition.CanBeUsed)
			{
				UseItem(itemById);
			}
		}
	}

	public void SetTutorialModeState(bool isActive)
	{
		isInTutorialMode = isActive;
		if (!isInTutorialMode)
		{
			tutorialModeExcludedList.Clear();
		}
	}

	public void AddToTutorialModeExcludedList(List<string> wgoExcludedUniqueIdList)
	{
		if (isInTutorialMode)
		{
			tutorialModeExcludedList.AddRange(wgoExcludedUniqueIdList);
		}
	}

	public void RemoveFromTutorialModeExcludedList(List<string> wgoExcludedUniqueIdList)
	{
		if (isInTutorialMode)
		{
			tutorialModeExcludedList.RemoveAll(wgoExcludedUniqueIdList.Contains);
		}
	}

	public void SetDirectionLock(bool isEnabled)
	{
		isDirectionLocked = !isEnabled;
	}

	public void AddDirectionListener(Action<Vector2> action)
	{
		direction.ValueChanged += action;
	}

	public void RemoveDirectionListener(Action<Vector2> action)
	{
		direction.ValueChanged -= action;
	}

	public void IncreaseInventorySize(int increaseValue)
	{
		MainGame.PlayerData.Inventory.Data.InventorySize += increaseValue;
		Debug.Log("Player inventory size increased by  " + increaseValue);
	}

	public void ReduceInventorySize(int reduceValue)
	{
		if (MainGame.PlayerData.Inventory.Data.InventorySize <= reduceValue)
		{
			Debug.LogError($"Cannot reduce inventory size by {reduceValue}. Current size: {MainGame.PlayerData.Inventory.Data.InventorySize} (must be > {reduceValue})");
			return;
		}
		if (MainGame.PlayerData.Inventory.Data.InventoryFillSize > MainGame.PlayerData.Inventory.Data.InventorySize - reduceValue)
		{
			Debug.LogError($"Cannot reduce inventory size by {reduceValue}. Current size: {MainGame.PlayerData.Inventory.Data.InventorySize} (must be > current fill size {MainGame.PlayerData.Inventory.Data.InventoryFillSize})");
			return;
		}
		MainGame.PlayerData.Inventory.Data.InventorySize -= reduceValue;
		Debug.Log("Player inventory size reduced by  " + reduceValue);
	}

	public bool IsEnoughRes(GameRes gameRes)
	{
		return res.IsEnough(gameRes);
	}

	public bool IsEnoughRes(GameResAtom gameResAtom)
	{
		return res.IsEnough(gameResAtom);
	}

	public void AddRes(GameRes gameRes)
	{
		res.Add(gameRes);
		this.OnGameResChanged?.Invoke();
	}

	public void AddRes(string type, float value)
	{
		res.Add(type, value);
		this.OnGameResChanged?.Invoke();
	}

	public void SetRes(GameRes gameRes)
	{
		res.Set(gameRes);
		this.OnGameResChanged?.Invoke();
	}

	public void SetRes(string type, float value)
	{
		res.Set(type, value);
		this.OnGameResChanged?.Invoke();
	}

	public void SetResWithoutSystemsCheck(string type, float value)
	{
		bool num = !res.Has(type) || !Mathf.Approximately(res.GetWithoutSystemsCheck(type), value);
		res.SetWithoutSystemsCheck(type, value);
		if (num)
		{
			this.OnGameResChanged?.Invoke();
		}
	}

	public float GetRes(string type, float defaultValue = 0f)
	{
		return res.Get(type, defaultValue);
	}

	public int GetResInt(string type)
	{
		return res.GetInt(type);
	}

	public GameResSystemBase GetResSystem(string type)
	{
		return res.GetSystem(type);
	}

	public void SubRes(string type, float value)
	{
		res.Sub(type, value);
		this.OnGameResChanged?.Invoke();
	}

	public void AddResWithoutSystemsCheck(string type, float value)
	{
		res.AddWithoutSystemsCheck(type, value);
		this.OnGameResChanged?.Invoke();
	}

	public void SubRes(GameRes gameRes)
	{
		res.Sub(gameRes);
		this.OnGameResChanged?.Invoke();
	}

	public void MultiplyRes(string type, float value)
	{
		res.Multiply(type, value);
		this.OnGameResChanged?.Invoke();
	}
}
