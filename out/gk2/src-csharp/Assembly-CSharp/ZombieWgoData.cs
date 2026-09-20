using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

[Serializable]
public class ZombieWgoData : WgoData, IWorker
{
	public enum ZombieCaretakerState
	{
		OnStation,
		GoToStation,
		GoToInventoryToPickUpOrderItem,
		PickingUpOrderItemFromInventory,
		GoToZombieToDeliverOrderItem,
		GoToZombieToPickUpOrderItem,
		GoToInventoryToDeliverOrderItem,
		GoToInventoryToPutPortableItemWithExistingOrder,
		GoToInventoryToPutPortableItemWithoutExistingOrder,
		WaitingOtherCaretakersOnInventory,
		FailedToFindPath,
		CanNotPutItemToInventory
	}

	public enum ZombieGardenerState
	{
		OnStation,
		GoToStation,
		TeleportSeedsFromMultiInventory,
		GoToGardenBedToPlantSeeds,
		PlantingSeeds,
		GoToGardenBedToTakePlants,
		GatheringPlants,
		WaitingForWgoDeath,
		FailedToFindPath,
		CanNotPutItemToInventory
	}

	public enum ZombieConveyorTransporterState
	{
		OnStation,
		GoToStation,
		GoToStationCellToPickUp,
		GoToStorageToPutItem,
		FailedToFindPath,
		CanNotPutItemToInventory
	}

	private const float CARETAKER_WAITING_NEAR_INVENTORY_DISTANCE = 2f;

	[SerializeField]
	private string name;

	[SerializeField]
	private ZombieType zombieType;

	[SerializeField]
	private Item zombieItem;

	[SerializeField]
	private SGuid attachedWgoDataUniqueId = SGuid.Empty;

	[SerializeField]
	private AnimationState curAnimState;

	private WgoData attachedWgoData;

	private IWorkActivity currentActivity;

	public SGuid equippedHand = SGuid.Empty;

	public SGuid equippedArmor = SGuid.Empty;

	public SGuid equippedCollar = SGuid.Empty;

	public List<ZombieTalentData> talentData = new List<ZombieTalentData>();

	public List<string> disabledTalentLevelUps = new List<string>();

	public int techRed;

	public int techBlue;

	public int techGreen;

	[SerializeField]
	private List<SGuid> crafterOrders = new List<SGuid>();

	[SerializeField]
	private string crafterOrderedCraftId;

	[SerializeField]
	private SGuid caretakerExecutingOrder = SGuid.Empty;

	[SerializeField]
	private Item caretakerPortableItem = Item.Empty;

	[SerializeField]
	private ZombieCaretakerState caretakerState;

	[SerializeField]
	private ZombieCaretakerState caretakerPreviousState;

	[SerializeField]
	private SGuid caretakerCurrentTargetUniqueId = SGuid.Empty;

	[SerializeField]
	private SGuid caretakerCurrentMovementTargetUniqueId = SGuid.Empty;

	[SerializeField]
	private float caretakerPickingUpFromInventoryTime;

	[SerializeField]
	private float caretakerTimeForCheckFailedPathAgain;

	[SerializeField]
	private Inventory porterInventory;

	[SerializeField]
	private SGuid gardenerExecutingOrder = SGuid.Empty;

	[SerializeField]
	private Item gardenerPortableItem = Item.Empty;

	[SerializeField]
	private ZombieGardenerState gardenerState;

	[SerializeField]
	private ZombieGardenerState gardenerPreviousState;

	[SerializeField]
	private SGuid gardenerCurrentTargetUniqueId = SGuid.Empty;

	[SerializeField]
	private SGuid gardenerCurrentMovementTargetUniqueId = SGuid.Empty;

	[SerializeField]
	private float gardenerPickingUpFromInventoryTime;

	[SerializeField]
	private float gardenerTimeForCheckFailedPathAgain;

	[SerializeField]
	private SGuid gardenerStation = SGuid.Empty;

	[SerializeField]
	private SGuid gardenerCurrentWorkingWgo = SGuid.Empty;

	[SerializeField]
	private SGuid conveyorTransporterExecutingOrder = SGuid.Empty;

	[SerializeField]
	private Item conveyorTransporterPortableItem = Item.Empty;

	[SerializeField]
	private ZombieConveyorTransporterState conveyorTransporterState;

	[SerializeField]
	private ZombieConveyorTransporterState conveyorTransporterPreviousState;

	[SerializeField]
	private SGuid conveyorTransporterCurrentTargetUniqueId = SGuid.Empty;

	[SerializeField]
	private SGuid conveyorTransporterCurrentMovementTargetUniqueId = SGuid.Empty;

	[SerializeField]
	private float conveyorTransporterTimeForCheckFailedPathAgain;

	public ZombieType ZombieType => zombieType;

	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	public Item ZombieItem => zombieItem;

	public WgoData AttachedWgoData
	{
		get
		{
			if (attachedWgoData == null)
			{
				attachedWgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(attachedWgoDataUniqueId);
			}
			return attachedWgoData;
		}
	}

	public int WhiteSkulls
	{
		get
		{
			int num = 0;
			foreach (Item item in ZombieItem.Inventory)
			{
				num += item.Definition.whiteSkulls * item.Count;
			}
			return Mathf.Clamp(num, 0, 999);
		}
	}

	public int RedSkulls
	{
		get
		{
			int num = 0;
			foreach (Item item in ZombieItem.Inventory)
			{
				num += item.Definition.redSkulls * item.Count;
			}
			return Mathf.Clamp(num, 0, 999);
		}
	}

	public SGuid Id => base.UniqueId;

	public IWorkActivity WorkerActivity => currentActivity;

	public MultiInventory WorkerMultiInventory => new MultiInventory(base.Inventory);

	public Inventory WorkerInventory
	{
		get
		{
			if (zombieType == ZombieType.Gardener)
			{
				return new Inventory(zombieItem);
			}
			return AttachedWgoData.CraftableObjectCraftInventory;
		}
	}

	public override Inventory CraftableObjectCraftInventory => AttachedWgoData.CraftableObjectCraftInventory;

	public Inventory WorkerToolInventory
	{
		get
		{
			Inventory inventory = new Inventory("toolBeltInventory", 2);
			if (!equippedHand.IsEmpty)
			{
				inventory.AddItemToInventory(new Item(Hand.id));
			}
			if (!equippedArmor.IsEmpty)
			{
				inventory.AddItemToInventory(new Item(Armor.id));
			}
			return inventory;
		}
	}

	public GameRes WorkerGameRes => gameRes;

	public Item Collar
	{
		get
		{
			ZombieItem.TryGetItemInInventoryByGUID(equippedCollar.Id, out var itemResult);
			return itemResult;
		}
	}

	public Item Hand
	{
		get
		{
			ZombieItem.TryGetItemInInventoryByGUID(equippedHand.Id, out var itemResult);
			return itemResult;
		}
	}

	public int AttackValue
	{
		get
		{
			Item hand = Hand;
			if (hand.IsEmpty)
			{
				return 0;
			}
			if (hand.Definition.type == ItemType.Pike)
			{
				return hand.Definition.damage.EvaluateInt();
			}
			if (hand.Definition.type == ItemType.Bow)
			{
				return hand.Definition.damage.EvaluateInt();
			}
			return 0;
		}
	}

	public Item Armor
	{
		get
		{
			ZombieItem.TryGetItemInInventoryByGUID(equippedArmor.Id, out var itemResult);
			return itemResult;
		}
	}

	public int ArmorValue
	{
		get
		{
			Item armor = Armor;
			if (!armor.IsEmpty)
			{
				return armor.Definition.quality;
			}
			return 0;
		}
	}

	public List<SGuid> CrafterOrders => crafterOrders;

	public OrderBase CrafterCurrentOrder
	{
		get
		{
			if (CrafterOrders.Count <= 0)
			{
				return null;
			}
			return base.WorldZoneData.FindOrder(CrafterOrders[0]);
		}
	}

	private ZombieCraftActivity ZombieCraftActivity => currentActivity as ZombieCraftActivity;

	private ZombieHPActivity ZombieHPActivity => currentActivity as ZombieHPActivity;

	private OrderBase CaretakerExecutingOrder
	{
		get
		{
			if (!caretakerExecutingOrder.IsEmpty)
			{
				return base.WorldZoneData.FindOrder(caretakerExecutingOrder);
			}
			return null;
		}
	}

	private bool HasCaretakerExecutingOrder => !caretakerExecutingOrder.IsEmpty;

	private WgoData CaretakerCurrentTarget
	{
		get
		{
			if (!caretakerCurrentTargetUniqueId.IsEmpty)
			{
				return MainGame.WorldData.GetWgoData(caretakerCurrentTargetUniqueId);
			}
			return null;
		}
	}

	public SGuid CaretakerCurrentTargetUniqueId => caretakerCurrentTargetUniqueId;

	public ZombieCaretakerState CaretakerState
	{
		get
		{
			return caretakerState;
		}
		set
		{
			caretakerPreviousState = caretakerState;
			caretakerState = value;
			this.OnCaretakerStateChanged?.Invoke();
		}
	}

	public Item CaretakerPortableItem
	{
		get
		{
			return caretakerPortableItem;
		}
		set
		{
			if (value.IsEmpty)
			{
				if (caretakerPortableItem.id != "empty")
				{
					if (caretakerPortableItem.Definition.itemSize == ItemSize.Big)
					{
						this.OnRemoveOverheadItem?.Invoke();
					}
					else
					{
						this.OnRemoveInteractingItem?.Invoke();
					}
				}
			}
			else if (value.Definition.itemSize == ItemSize.Big)
			{
				this.OnSetOverheadItem?.Invoke(value, arg2: true);
			}
			else
			{
				this.OnSetInteractingItem?.Invoke(value);
			}
			caretakerPortableItem = value;
		}
	}

	private OrderBase GardenerExecutingOrder
	{
		get
		{
			if (!gardenerExecutingOrder.IsEmpty)
			{
				return base.WorldZoneData.FindOrder(gardenerExecutingOrder);
			}
			return null;
		}
	}

	private bool HasGardenerExecutingOrder => !gardenerExecutingOrder.IsEmpty;

	private WgoData GardenerCurrentTarget => MainGame.WorldData.GetWgoData(gardenerCurrentTargetUniqueId);

	public SGuid GardenerCurrentTargetUniqueId => gardenerCurrentTargetUniqueId;

	public ZombieGardenerState GardenerState
	{
		get
		{
			return gardenerState;
		}
		set
		{
			gardenerPreviousState = gardenerState;
			gardenerState = value;
			this.OnCaretakerStateChanged?.Invoke();
		}
	}

	public Item GardenerPortableItem
	{
		get
		{
			return caretakerPortableItem;
		}
		set
		{
			if (value.IsEmpty)
			{
				if (caretakerPortableItem.id != "empty")
				{
					if (caretakerPortableItem.Definition.itemSize == ItemSize.Big)
					{
						this.OnRemoveOverheadItem?.Invoke();
					}
					else
					{
						this.OnRemoveInteractingItem?.Invoke();
					}
				}
			}
			else if (value.Definition.itemSize == ItemSize.Big)
			{
				this.OnSetOverheadItem?.Invoke(value, arg2: true);
			}
			else
			{
				this.OnSetInteractingItem?.Invoke(value);
			}
			caretakerPortableItem = value;
		}
	}

	private OrderBase ConveyorTransporterExecutingOrder
	{
		get
		{
			if (!conveyorTransporterExecutingOrder.IsEmpty)
			{
				return base.WorldZoneData.FindOrder(conveyorTransporterExecutingOrder);
			}
			return null;
		}
	}

	private bool HasConveyorTransporterExecutingOrder => !conveyorTransporterExecutingOrder.IsEmpty;

	private WgoData ConveyorTransporterCurrentTarget
	{
		get
		{
			if (!conveyorTransporterCurrentTargetUniqueId.IsEmpty)
			{
				return MainGame.WorldData.GetWgoData(conveyorTransporterCurrentTargetUniqueId);
			}
			return null;
		}
	}

	public SGuid ConveyorTransporterCurrentTargetUniqueId => conveyorTransporterCurrentTargetUniqueId;

	public ZombieConveyorTransporterState ConveyorTransporterState
	{
		get
		{
			return conveyorTransporterState;
		}
		set
		{
			conveyorTransporterPreviousState = conveyorTransporterState;
			conveyorTransporterState = value;
			this.OnCaretakerStateChanged?.Invoke();
		}
	}

	public Item ConveyorTransporterPortableItem
	{
		get
		{
			return conveyorTransporterPortableItem;
		}
		set
		{
			if (value.IsEmpty)
			{
				if (conveyorTransporterPortableItem.id != "empty")
				{
					if (conveyorTransporterPortableItem.Definition.itemSize == ItemSize.Big)
					{
						this.OnRemoveOverheadItem?.Invoke();
					}
					else
					{
						this.OnRemoveInteractingItem?.Invoke();
					}
				}
			}
			else if (value.Definition.itemSize == ItemSize.Big)
			{
				this.OnSetOverheadItem?.Invoke(value, arg2: true);
			}
			else
			{
				this.OnSetInteractingItem?.Invoke(value);
			}
			conveyorTransporterPortableItem = value;
		}
	}

	public event Action<AnimationState, bool> OnAnimationStateChanged;

	public event Action<Item, bool> OnSetOverheadItem;

	public event Action OnRemoveOverheadItem;

	public event Action<Item> OnSetInteractingItem;

	public event Action OnRemoveInteractingItem;

	public event Action<Inventory> OnEquipmentChanged;

	public event Action OnCaretakerStateChanged;

	public static event Action<WgoData, ZombieWgoData, string, int> OnTechPointsAddedToZombie;

	public static event Action OnTalentLevelUpPurchased;

	public event Action CrafterOnOrderAddedEvent;

	public event Action CrafterOnOrderRemovedEvent;

	public ZombieWgoData()
	{
	}

	public ZombieWgoData(string id, Vector3 position, string worldId)
		: base(id, position, worldId)
	{
		foreach (TalentDef talentDef in GameBalance.Me.talentDefs)
		{
			ZombieTalentData zombieTalentData = new ZombieTalentData(talentDef.id);
			foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
			{
				if (talentLevelUpDef.isZombiePerk && talentLevelUpDef.availableAtStart && talentLevelUpDef.talentId == talentDef.id)
				{
					zombieTalentData.studiedLevelUps.Add(talentLevelUpDef.id);
					zombieTalentData.curTalentValue += talentLevelUpDef.talentValueAdd;
					if (!string.IsNullOrEmpty(talentLevelUpDef.linkedPerk))
					{
						AddPerk(talentLevelUpDef.linkedPerk);
					}
				}
			}
			talentData.Add(zombieTalentData);
		}
		RollName();
	}

	public ZombieWgoData CreateFighterFromThis(Vector3 position, string worldId)
	{
		ZombieWgoData zombieWgoData = new ZombieWgoData("zmb_wild_mob_allie", position, worldId);
		zombieWgoData.name = name;
		zombieWgoData.talentData = talentData;
		zombieWgoData.activePerks = activePerks;
		zombieWgoData.zombieItem = zombieItem;
		zombieWgoData.equippedHand = equippedHand;
		zombieWgoData.equippedArmor = equippedArmor;
		zombieWgoData.equippedCollar = equippedCollar;
		zombieWgoData.SetGameRes(gameRes);
		zombieWgoData.GameResStr.Set(base.GameResStr);
		return zombieWgoData;
	}

	public ZombieWgoData CreateAssistantFromThis(Vector3 position, string worldId, Direction direction = Direction.Down)
	{
		ZombieWgoData zombieWgoData = new ZombieWgoData("zombie_assistant", position, worldId);
		zombieItem.UniqueId.SetGuid(zombieWgoData.UniqueId);
		zombieWgoData.name = name;
		zombieWgoData.talentData = talentData;
		zombieWgoData.activePerks = activePerks;
		zombieWgoData.zombieItem = zombieItem;
		zombieWgoData.equippedHand = equippedHand;
		zombieWgoData.equippedArmor = equippedArmor;
		zombieWgoData.equippedCollar = equippedCollar;
		zombieWgoData.WorkerGameRes.Set(WorkerGameRes);
		zombieWgoData.WorkerGameRes.Set("zombie_body_id", 1002f);
		zombieWgoData.GameResStr.Set(base.GameResStr);
		zombieWgoData.direction.Value = direction.ConvertToVector2XZ();
		return zombieWgoData;
	}

	public ZombieWgoData CreateCommonZombieFromThis(Vector3 position, string worldId, Direction direction = Direction.Down)
	{
		ZombieWgoData zombieWgoData = new ZombieWgoData("zombie", position, worldId);
		zombieItem.UniqueId.SetGuid(zombieWgoData.UniqueId);
		zombieWgoData.name = name;
		zombieWgoData.talentData = talentData;
		zombieWgoData.activePerks = activePerks;
		zombieWgoData.zombieItem = zombieItem;
		zombieWgoData.equippedHand = equippedHand;
		zombieWgoData.equippedArmor = equippedArmor;
		zombieWgoData.equippedCollar = equippedCollar;
		WorkerGameRes.Set("zombie_body_id", 0f);
		WorkerGameRes.RemoveZeroValues();
		zombieWgoData.WorkerGameRes.Set(WorkerGameRes);
		zombieWgoData.GameResStr.Set(base.GameResStr);
		zombieWgoData.direction.Value = direction.ConvertToVector2XZ();
		return zombieWgoData;
	}

	public void RollName()
	{
		string text = name;
		name = MainGame.Instance.GameSave.knowledgeSystem.GetZombieName();
		if (!string.IsNullOrEmpty(text))
		{
			MainGame.Instance.GameSave.knowledgeSystem.freeZombieNames.Add(text);
		}
	}

	public override string ToString()
	{
		return $"[Zombie: id={id}, uniqueId={base.UniqueId}, type={zombieType}, name={name}, item={zombieItem}]";
	}

	public void PrepareForGameBase()
	{
		base.PrepareForGame();
	}

	public override void PrepareForGame()
	{
		base.PrepareForGame();
		SyncTalentLevelUpsFromBalance();
		if (AttachedWgoData == null)
		{
			return;
		}
		switch (zombieType)
		{
		case ZombieType.Crafter:
			CrafterHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
			AttachedWgoData.TrySetWorker(this);
			AttachedWgoData.CraftComponent.OnStatusChanged += CrafterHandleCraftStatusChange;
			AttachedWgoData.CraftComponent.OnCraftCurProgressNormalizedChanged += CrafterHandleCraftProgressChange;
			base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
			base.WorldZoneData.OnOrderAdded += OnOrderAdded;
			AttachedWgoData.CraftComponent.OnCraftAddedToQueue += CrafterOnCraftAddedToQueue;
			AttachedWgoData.CraftComponent.OnCraftRemovedFromQueue += CrafterOnCraftRemovedFromQueue;
			if (AttachedWgoData.CraftComponent.CurrentCraftElement != null && AttachedWgoData.CraftComponent.CurrentCraftElement.IsStarted && ZombieCraftActivity == null)
			{
				currentActivity = MainGame.Instance.craftSystem.TryGetCraftActivity(this);
				if (currentActivity == null)
				{
					CrafterStartCraftActivity(resetTicks: false);
					break;
				}
				ZombieCraftActivity.OnActiveStateChanged += CrafterOnCraftActivityStateChanged;
				CrafterOnCraftActivityStateChanged();
				UpdateAttachedWgoViewWidgets();
			}
			break;
		case ZombieType.ConveyorCrafter:
			ConveyorCrafterHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
			AttachedWgoData.TrySetWorker(this);
			AttachedWgoData.CraftComponent.OnStatusChanged += ConveyorCrafterHandleCraftStatusChange;
			AttachedWgoData.CraftComponent.OnStatusChanged += ConveyorHadleCraftStatusChange;
			AttachedWgoData.CraftComponent.OnCraftCurProgressNormalizedChanged += CrafterHandleCraftProgressChange;
			AttachedWgoData.CraftComponent.OnCraftAddedToQueue += ConveyorCrafterOnCraftAddedToQueue;
			AttachedWgoData.CraftComponent.OnCraftRemovedFromQueue += ConveyorCrafterOnCraftRemovedFromQueue;
			AttachedWgoData.CraftComponent.CraftableObject.CraftableObjectCraftInventory.OnItemsAdd += ConveyorCrafterTryStartCurrentCraft;
			if (AttachedWgoData.CraftComponent.CurrentCraftElement != null && AttachedWgoData.CraftComponent.CurrentCraftElement.IsStarted && ZombieCraftActivity == null)
			{
				currentActivity = MainGame.Instance.conveyorSystem.TryGetCraftActivity(this);
				if (currentActivity == null)
				{
					ConveyorCrafterStartCraftActivity(resetTicks: false);
					break;
				}
				ZombieCraftActivity.OnActiveStateChanged += ConveyorCrafterOnCraftActivityStateChanged;
				ConveyorCrafterOnCraftActivityStateChanged();
				UpdateAttachedWgoViewWidgets();
			}
			else if (AttachedWgoData.CraftComponent.HasCraftsInQueue)
			{
				ConveyorCrafterTryStartCurrentCraft();
			}
			break;
		case ZombieType.Caretaker:
			AttachedWgoData.TrySetWorker(this);
			base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
			break;
		case ZombieType.Gardener:
			AttachedWgoData.TrySetWorker(this);
			base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
			if (gardenerState == ZombieGardenerState.PlantingSeeds && ZombieCraftActivity == null)
			{
				currentActivity = MainGame.Instance.craftSystem.TryGetCraftActivity(this);
				if (ZombieCraftActivity != null)
				{
					ZombieCraftActivity.OnActiveStateChanged += GardenerOnCraftActivityStateChanged;
					GardenerOnCraftActivityStateChanged();
				}
				else
				{
					GardenerTryStartOrderExecutionOrGoToStation();
				}
			}
			else if ((gardenerState == ZombieGardenerState.GatheringPlants || gardenerState == ZombieGardenerState.WaitingForWgoDeath) && ZombieHPActivity == null)
			{
				currentActivity = MainGame.Instance.craftSystem.TryGetHPActivity(this);
				if (ZombieHPActivity != null)
				{
					ZombieHPActivity.OnActiveStateChanged += GardenerOnWorkActivityStateChanged;
					GardenerOnWorkActivityStateChanged();
				}
				else
				{
					GardenerTryStartOrderExecutionOrGoToStation();
				}
			}
			break;
		case ZombieType.ConveyorTransporter:
			AttachedWgoData.TrySetWorker(this);
			base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
			break;
		case ZombieType.Porter:
			if (GetGameResInt("is_staying_at_porter_station") == 1)
			{
				base.IsInteractable = false;
				AttachedWgoData.SetTriggerToAnimator("with_zombie");
			}
			else
			{
				base.IsInteractable = true;
				AttachedWgoData.SetTriggerToAnimator("path_state");
				SetLayerWeightToAnimator(4, 1f);
			}
			break;
		}
		this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
	}

	public override void DeInit()
	{
		base.DeInit();
		if (AttachedWgoData != null)
		{
			AttachedWgoData.CraftComponent.OnStatusChanged -= CrafterHandleCraftStatusChange;
		}
	}

	public void AttachToPowerSourceWgoData(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.Worker;
		CaretakerState = ZombieCaretakerState.OnStation;
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		AttachedWgoData.TrySetWorker(this, dockPointData);
	}

	public void AttachToStationWgoData(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.Caretaker;
		CaretakerState = ZombieCaretakerState.OnStation;
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		AttachedWgoData.TrySetWorker(this, dockPointData);
		base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
		ZombieDeliveryIndication.RedrawCrafterWorkbenchesInZone(AttachedWgoData.WorldZoneData);
	}

	public void AttachToGardenStationWgoData(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.Gardener;
		GardenerState = ZombieGardenerState.OnStation;
		gardenerStation.SetGuid(uniqueId);
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		AttachedWgoData.TrySetWorker(this, dockPointData);
		base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
	}

	public void AttachToConveyorTransporterStationWgoData(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.ConveyorTransporter;
		ConveyorTransporterState = ZombieConveyorTransporterState.OnStation;
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		AttachedWgoData.TrySetWorker(this, dockPointData);
		base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
	}

	public void AttachToCraftWgoData(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.Crafter;
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		CrafterHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
		AttachedWgoData.TrySetWorker(this, dockPointData);
		AttachedWgoData.CraftComponent.OnStatusChanged += CrafterHandleCraftStatusChange;
		AttachedWgoData.CraftComponent.OnCraftCurProgressNormalizedChanged += CrafterHandleCraftProgressChange;
		base.WorldZoneData.OnOrderRemoved += OnOrderRemoved;
		base.WorldZoneData.OnOrderAdded += OnOrderAdded;
		AttachedWgoData.CraftComponent.OnCraftAddedToQueue += CrafterOnCraftAddedToQueue;
		AttachedWgoData.CraftComponent.OnCraftRemovedFromQueue += CrafterOnCraftRemovedFromQueue;
		if (AttachedWgoData.CraftComponent.CurrentCraftElement != null && AttachedWgoData.CraftComponent.CurrentCraftElement.IsStarted && ZombieCraftActivity == null)
		{
			CrafterStartCraftActivity(resetTicks: false);
		}
		else if (AttachedWgoData.CraftComponent.HasCraftsInQueue)
		{
			CrafterTryPlaceOrderForCurrentCraftOrStartIt();
		}
	}

	public void AttachToConveyorCraftWgoData(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.ConveyorCrafter;
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		ConveyorCrafterHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
		AttachedWgoData.TrySetWorker(this, dockPointData);
		AttachedWgoData.CraftComponent.OnStatusChanged += ConveyorCrafterHandleCraftStatusChange;
		AttachedWgoData.CraftComponent.OnStatusChanged += ConveyorHadleCraftStatusChange;
		AttachedWgoData.CraftComponent.OnCraftCurProgressNormalizedChanged += CrafterHandleCraftProgressChange;
		AttachedWgoData.CraftComponent.OnCraftAddedToQueue += ConveyorCrafterOnCraftAddedToQueue;
		AttachedWgoData.CraftComponent.OnCraftRemovedFromQueue += ConveyorCrafterOnCraftRemovedFromQueue;
		AttachedWgoData.CraftComponent.CraftableObject.CraftableObjectCraftInventory.OnItemsAdd += ConveyorCrafterTryStartCurrentCraft;
		if (AttachedWgoData.CraftComponent.CurrentCraftElement != null && AttachedWgoData.CraftComponent.CurrentCraftElement.IsStarted && ZombieCraftActivity == null)
		{
			ConveyorCrafterStartCraftActivity(resetTicks: false);
		}
		else if (AttachedWgoData.CraftComponent.HasCraftsInQueue)
		{
			ConveyorCrafterTryStartCurrentCraft();
		}
	}

	public void AttachToPorterStation(SGuid uniqueId, Item zombie, DockPointData dockPointData = null)
	{
		zombieType = ZombieType.Porter;
		attachedWgoDataUniqueId.SetGuid(uniqueId);
		attachedWgoData = null;
		zombieItem = zombie;
		AttachedWgoData.TrySetWorker(this, dockPointData);
		SetGameRes("is_staying_at_porter_station", 1);
		porterInventory = Inventory.Create(4);
		AttachedWgoData.SetTriggerToAnimator("with_zombie");
		base.IsInteractable = false;
	}

	public void AttachToFightersContainer()
	{
		zombieType = ZombieType.Fighter;
	}

	public void UnAttachFromWgoData(bool dropPorterInventoryNearPlayer = false)
	{
		SetDefaultAnimState();
		if (!AttachedWgoData.CraftComponent.IsDestroyingCraftActive && AttachedWgoData.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp)
		{
			AttachedWgoData.CraftComponent.TryFinishCurCraft();
		}
		WorldZoneData worldZoneData = null;
		switch (zombieType)
		{
		case ZombieType.Crafter:
			CrafterDropCraftInventory();
			AttachedWgoData.CraftComponent.OnStatusChanged -= CrafterHandleCraftStatusChange;
			AttachedWgoData.CraftComponent.OnCraftCurProgressNormalizedChanged -= CrafterHandleCraftProgressChange;
			AttachedWgoData.CraftComponent.OnCraftAddedToQueue -= CrafterOnCraftAddedToQueue;
			AttachedWgoData.CraftComponent.OnCraftRemovedFromQueue -= CrafterOnCraftRemovedFromQueue;
			base.WorldZoneData.ClearOrders(new List<SGuid>(crafterOrders));
			CrafterStopCraftActivity();
			crafterOrderedCraftId = string.Empty;
			base.WorldZoneData.OnOrderRemoved -= OnOrderRemoved;
			base.WorldZoneData.OnOrderAdded -= OnOrderAdded;
			break;
		case ZombieType.ConveyorCrafter:
			AttachedWgoData.CraftComponent.OnStatusChanged -= ConveyorCrafterHandleCraftStatusChange;
			AttachedWgoData.CraftComponent.OnStatusChanged -= ConveyorHadleCraftStatusChange;
			AttachedWgoData.CraftComponent.OnCraftCurProgressNormalizedChanged -= CrafterHandleCraftProgressChange;
			AttachedWgoData.CraftComponent.OnCraftAddedToQueue -= ConveyorCrafterOnCraftAddedToQueue;
			AttachedWgoData.CraftComponent.OnCraftRemovedFromQueue -= ConveyorCrafterOnCraftRemovedFromQueue;
			AttachedWgoData.CraftComponent.CraftableObject.CraftableObjectCraftInventory.OnItemsAdd -= ConveyorCrafterTryStartCurrentCraft;
			ConveyorCrafterStopCraftActivity();
			break;
		case ZombieType.Caretaker:
			CaretakerTryStopOrderExecution();
			caretakerCurrentMovementTargetUniqueId = SGuid.Empty;
			if (!CaretakerPortableItem.IsEmpty)
			{
				MainGame.Instance.dropSystem.DropItem(CaretakerPortableItem, base.WorldId, base.Position);
				CaretakerPortableItem = Item.Empty;
			}
			worldZoneData = AttachedWgoData.WorldZoneData;
			break;
		case ZombieType.ConveyorTransporter:
			ConveyorTransporterTryStopOrderExecution();
			conveyorTransporterCurrentMovementTargetUniqueId = SGuid.Empty;
			if (!ConveyorTransporterPortableItem.IsEmpty)
			{
				MainGame.Instance.dropSystem.DropItem(ConveyorTransporterPortableItem, base.WorldId, base.Position);
				ConveyorTransporterPortableItem = Item.Empty;
			}
			base.WorldZoneData.OnOrderRemoved -= OnOrderRemoved;
			break;
		case ZombieType.Gardener:
			GardenerTryStopOrderExecution();
			gardenerCurrentMovementTargetUniqueId = SGuid.Empty;
			if (!GardenerPortableItem.IsEmpty)
			{
				MainGame.Instance.dropSystem.DropItem(GardenerPortableItem, base.WorldId, base.Position);
				GardenerPortableItem = Item.Empty;
			}
			if (GardenerState == ZombieGardenerState.PlantingSeeds)
			{
				GardenerStopCraftActivity(stopOnly: true);
				AttachedWgoData.ClearWorker();
			}
			else if (GardenerState == ZombieGardenerState.GatheringPlants)
			{
				GardenerStopWorkActivity(null, stopOnly: true);
				AttachedWgoData.ClearWorker();
			}
			attachedWgoData = null;
			attachedWgoDataUniqueId.SetGuid(gardenerStation);
			gardenerStation = SGuid.Empty;
			base.WorldZoneData.OnOrderRemoved -= OnOrderRemoved;
			break;
		case ZombieType.Porter:
		{
			SetGameRes("is_staying_at_porter_station", 0);
			string currentGameSceneId = base.WorldId;
			Vector3 pos = base.Position;
			if (dropPorterInventoryNearPlayer)
			{
				PlayerData playerData = MainGame.PlayerData;
				Vector2 vector = playerData.Direction * 1f;
				currentGameSceneId = playerData.currentGameSceneId;
				pos = playerData.position.Value + new Vector3(vector.x, 0f, vector.y);
			}
			foreach (Item item in porterInventory.Data.Inventory)
			{
				if (!(item.id == "fake_porter_slot_filler"))
				{
					MainGame.Instance.dropSystem.DropItem(item, currentGameSceneId, pos);
				}
			}
			porterInventory = null;
			AttachedWgoData.SetTriggerToAnimator("backpack_anim");
			base.IsInteractable = true;
			break;
		}
		default:
			throw new ArgumentOutOfRangeException();
		case ZombieType.Free:
		case ZombieType.Worker:
		case ZombieType.Fighter:
			break;
		}
		AttachedWgoData.ClearWorker();
		AttachedWgoData.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
		UpdateAttachedWgoViewWidgets();
		attachedWgoDataUniqueId = SGuid.Empty;
		attachedWgoData = null;
		takenDockPointsParentSGuid = null;
		zombieType = ZombieType.Free;
		base.MovementComponent.ForceStop();
		if (worldZoneData != null)
		{
			ZombieDeliveryIndication.RedrawCrafterWorkbenchesInZone(worldZoneData);
		}
	}

	public void SetDefaultAnimState()
	{
		curAnimState = AnimationState.Idle;
		this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
	}

	public override void OnReleaseWgoPartToPool()
	{
		base.OnReleaseWgoPartToPool();
		this.OnAnimationStateChanged?.Invoke(AnimationState.Idle, arg2: false);
	}

	public void SetZombieItem(Item zombie)
	{
		zombieItem = zombie;
	}

	public override MultiInventory GetCraftableMultiInventory(bool excludeWorkerInventory = false)
	{
		return new MultiInventory(WorkerInventory);
	}

	public void CustomUpdate(float deltaTime)
	{
		if (zombieType == ZombieType.Caretaker)
		{
			CaretakerUpdateBehaviour(deltaTime);
		}
		if (zombieType == ZombieType.Gardener)
		{
			GardenerUpdateBehaviour(deltaTime);
		}
		if (zombieType == ZombieType.ConveyorTransporter)
		{
			ConveyorTransporterUpdateBehaviour(deltaTime);
		}
	}

	private void OnOrderAdded(OrderBase order)
	{
		if (zombieType == ZombieType.Crafter)
		{
			CrafterOnOrderAdded(order);
		}
	}

	private void OnOrderRemoved(OrderBase order)
	{
		switch (zombieType)
		{
		case ZombieType.Crafter:
			CrafterOnOrderRemoved(order);
			break;
		case ZombieType.Caretaker:
			CaretakerOnOrderRemoved(order);
			break;
		case ZombieType.Gardener:
			GardenerOnOrderRemoved(order);
			break;
		case ZombieType.ConveyorTransporter:
			ConveyorTransporterOnOrderRemoved(order);
			break;
		case ZombieType.ConveyorCrafter:
		case ZombieType.Worker:
		case ZombieType.Porter:
			break;
		}
	}

	private void UpdateAttachedWgoViewWidgets()
	{
		if (!attachedWgoDataUniqueId.IsEmpty && AttachedWgoData != null)
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(AttachedWgoData.UniqueId);
			if (wgoViewGlobal != null)
			{
				wgoViewGlobal.DrawWidgets();
			}
		}
	}

	public int GetMasteryLevelForTalentBranch(string talentId, CraftDefBase craftDef = null)
	{
		int num = 0;
		foreach (Item item in WorkerToolInventory.Data.Inventory)
		{
			if (item.Definition.talentIds.Contains(talentId))
			{
				num += item.Definition.talentBonus;
			}
		}
		int perksCraftMasteryBonusValue = GetPerksCraftMasteryBonusValue(craftDef);
		return GetTalentBranch(talentId).curTalentValue + num + perksCraftMasteryBonusValue;
	}

	public bool HasToolForWork(WgoData wgoData, CraftDefBase craftDef)
	{
		ItemType itemType = ItemType.None;
		if (craftDef != null && craftDef is CraftDef { customItemTypeAction: not ItemType.None } craftDef2 && !craftDef.isAuto)
		{
			itemType = craftDef2.customItemTypeAction;
		}
		if (itemType == ItemType.None)
		{
			itemType = wgoData.Definition.toolAction.actionableTool;
		}
		switch (itemType)
		{
		case ItemType.None:
			return true;
		case ItemType.Hand:
			return true;
		default:
			if (!WorkerToolInventory.Data.GetItemByType(itemType).IsEmpty)
			{
				return true;
			}
			return false;
		}
	}

	public bool HasToolForWork(WgoData wgoData, out ItemType possibleTool)
	{
		possibleTool = ItemType.None;
		foreach (CraftDefBase availableCraft in wgoData.CraftComponent.AvailableCrafts)
		{
			if (availableCraft is CraftDef { customItemTypeAction: not ItemType.None } craftDef && !availableCraft.isAuto)
			{
				possibleTool = craftDef.customItemTypeAction;
				break;
			}
		}
		if (possibleTool == ItemType.None)
		{
			possibleTool = wgoData.Definition.toolAction.actionableTool;
		}
		if (possibleTool == ItemType.None)
		{
			return true;
		}
		if (!WorkerToolInventory.Data.GetItemByType(possibleTool).IsEmpty)
		{
			return true;
		}
		return false;
	}

	public int GetPerksCraftMasteryBonusValue(CraftDefBase craftDef)
	{
		int num = 0;
		if (craftDef == null)
		{
			return 0;
		}
		foreach (string linkedPerk in craftDef.linkedPerks)
		{
			if (HasPerk(linkedPerk))
			{
				num += GameBalance.Me.GetData<PerkDef>(linkedPerk).craftMasteryBonus;
			}
		}
		return num;
	}

	public int GetPerksCraftStartTicksBonusValue(CraftDefBase craftDef)
	{
		int num = 0;
		foreach (string linkedPerk in craftDef.linkedPerks)
		{
			PerkData perkData = activePerks.Find((PerkData x) => x.id == linkedPerk);
			if (perkData != null)
			{
				num += perkData.Definition.craftStartTicks;
			}
		}
		return num;
	}

	public int GetPerksCraftAddTotalProgressTicksValue(CraftDefBase craftDef)
	{
		int num = 0;
		foreach (string linkedPerk in craftDef.linkedPerks)
		{
			PerkData perkData = activePerks.Find((PerkData x) => x.id == linkedPerk);
			if (perkData != null)
			{
				num += perkData.Definition.craftTotalProgressTicksBonus;
			}
		}
		return num;
	}

	public bool CanRemoveItemFromBody(Item item)
	{
		int red = RedSkulls - item.Definition.redSkulls;
		int white = WhiteSkulls - item.Definition.whiteSkulls;
		return SkullsInCollarBorders(red, white);
	}

	public bool CanAddItemToBody(Item item)
	{
		int red = RedSkulls + item.Definition.redSkulls;
		int white = WhiteSkulls + item.Definition.whiteSkulls;
		return SkullsInCollarBorders(red, white);
	}

	public bool CanChangeItemInBody(Item itemFrom, Item itemTo)
	{
		int red = RedSkulls + itemTo.Definition.redSkulls - itemFrom.Definition.redSkulls;
		int white = WhiteSkulls + itemTo.Definition.whiteSkulls - itemFrom.Definition.whiteSkulls;
		return SkullsInCollarBorders(red, white);
	}

	public bool CanUpgradeCollarTo(Item item)
	{
		if (item == null || item.IsEmpty || item.Definition.type != ItemType.Collar)
		{
			return false;
		}
		Item collar = Collar;
		if (collar == null || collar.IsEmpty)
		{
			return false;
		}
		if (item.Definition.redSkullsMaxCollar <= collar.Definition.redSkullsMaxCollar)
		{
			return false;
		}
		return item.Definition.SkullsInBorders(WhiteSkulls, RedSkulls);
	}

	private bool SkullsInCollarBorders(int red, int white)
	{
		Item collar = Collar;
		if (collar == null || collar.IsEmpty)
		{
			return false;
		}
		return collar.Definition.SkullsInBorders(white, red);
	}

	public float GetPerksEnergyBonusValue(CraftDefBase craftDef)
	{
		float num = 0f;
		foreach (string linkedPerk in craftDef.linkedPerks)
		{
			PerkData perkData = activePerks.Find((PerkData x) => x.id == linkedPerk);
			if (perkData != null)
			{
				num += perkData.Definition.energyAdd;
			}
		}
		return num;
	}

	public float GetPerksInsanityBonusValue(CraftDefBase craftDef)
	{
		float num = 0f;
		foreach (string linkedPerk in craftDef.linkedPerks)
		{
			PerkData perkData = activePerks.Find((PerkData x) => x.id == linkedPerk);
			if (perkData != null)
			{
				num += perkData.Definition.insanityAdd;
			}
		}
		return num;
	}

	public Item GetToolForWorkOnCraft(WgoData wgoData, CraftDefBase craftDef)
	{
		ItemType itemType = ItemType.None;
		if (craftDef is CraftDef { customItemTypeAction: not ItemType.None } craftDef2 && !craftDef.isAuto)
		{
			itemType = craftDef2.customItemTypeAction;
		}
		if (itemType == ItemType.None)
		{
			itemType = wgoData.Definition.toolAction.actionableTool;
		}
		if (itemType == ItemType.Hand)
		{
			return new Item("hand_tool");
		}
		foreach (Item item in WorkerToolInventory.Data.Inventory)
		{
			if (item.Definition.type == itemType)
			{
				return item;
			}
		}
		return Item.Empty;
	}

	public CraftStatus CheckWorkerDependentValues(CraftElement craftElement, float deltaTime = 1f, bool skipEnergyCheck = false, bool skipInsanityCheck = false)
	{
		CraftDef craftDef = craftElement.Definition;
		if (!craftElement.ParamsData.HasRequiredTool)
		{
			return CraftStatus.DoesntHaveRequiredTool;
		}
		if (!craftDef.isStarCraft && !craftDef.isAutopsyCraft && craftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.Common && craftElement.ParamsData.MasteryValue < craftElement.ParamsData.MasteryLock)
		{
			return CraftStatus.NotEnoughMastery;
		}
		if ((craftDef.isStarCraft || craftDef.isAutopsyCraft) && craftElement.ParamsData.MasteryValue <= 0)
		{
			return CraftStatus.NotEnoughMastery;
		}
		if (craftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenPlanting && craftElement.ParamsData.MasteryValue <= 0)
		{
			return CraftStatus.NotEnoughMastery;
		}
		return CraftStatus.OK;
	}

	public void AddRes(string type, float value)
	{
		AddGameRes(type, value);
	}

	public void MultiplyRes(string type, float value)
	{
		MultiplyGameRes(type, value);
	}

	public void SetRes(string stype, float value)
	{
		SetGameRes(stype, value);
	}

	public float GetRes(string stype, float defaultValue = 0f)
	{
		return GetGameRes(stype);
	}

	public void DoTechPointsReward(WgoData from, int r, int g, int b)
	{
		if (r > 0)
		{
			techRed += r;
			ZombieWgoData.OnTechPointsAddedToZombie?.Invoke(from, this, "tech_red", r);
		}
		if (g > 0)
		{
			techGreen += g;
			ZombieWgoData.OnTechPointsAddedToZombie?.Invoke(from, this, "tech_green", g);
		}
		if (b > 0)
		{
			techBlue += b;
			ZombieWgoData.OnTechPointsAddedToZombie?.Invoke(from, this, "tech_blue", b);
		}
	}

	public ZombieTalentData GetTalentBranch(string talentId)
	{
		return talentData.Find((ZombieTalentData x) => x.id == talentId);
	}

	public bool IsTalentLevelUpStudied(string id)
	{
		foreach (ZombieTalentData talentDatum in talentData)
		{
			if (talentDatum.studiedLevelUps.Contains(id))
			{
				return true;
			}
		}
		return false;
	}

	public void OnAddOrgan(Item item)
	{
		if (!string.IsNullOrEmpty(item.Definition.bodyLinkedPerk))
		{
			AddPerk(item.Definition.bodyLinkedPerk);
		}
		if (item.Definition.redSkulls < 0)
		{
			CheckRedSkulls(addRedSkulls: false);
		}
		if (item.Definition.redSkulls > 0)
		{
			CheckRedSkulls(addRedSkulls: true);
		}
	}

	public void OnRemoveOrgan(Item item)
	{
		if (!string.IsNullOrEmpty(item.Definition.bodyLinkedPerk))
		{
			RemovePerk(item.Definition.bodyLinkedPerk);
		}
		if (item.Definition.redSkulls < 0)
		{
			CheckRedSkulls(addRedSkulls: true);
		}
		if (item.Definition.redSkulls > 0)
		{
			CheckRedSkulls(addRedSkulls: false);
		}
	}

	public void CheckRedSkulls(bool addRedSkulls)
	{
		int usedPerksCount = GetUsedPerksCount();
		int redSkulls = RedSkulls;
		if (addRedSkulls)
		{
			if (disabledTalentLevelUps.Count <= 0)
			{
				return;
			}
			int num = redSkulls + disabledTalentLevelUps.Count - usedPerksCount;
			{
				foreach (ZombieTalentData talentDatum in talentData)
				{
					if (num <= 0)
					{
						break;
					}
					for (int i = 0; i < talentDatum.studiedLevelUps.Count; i++)
					{
						if (num <= 0)
						{
							break;
						}
						if (disabledTalentLevelUps.Contains(talentDatum.studiedLevelUps[i]))
						{
							disabledTalentLevelUps.Remove(talentDatum.studiedLevelUps[i]);
							TalentLevelUpDef data = GameBalance.Me.GetData<TalentLevelUpDef>(talentDatum.studiedLevelUps[i]);
							if (!string.IsNullOrEmpty(data.linkedPerk))
							{
								AddPerk(data.linkedPerk);
							}
							GetTalentBranch(data.talentId).curTalentValue += data.talentValueAdd;
							num--;
						}
					}
				}
				return;
			}
		}
		int num2 = usedPerksCount - redSkulls - disabledTalentLevelUps.Count;
		if (num2 <= 0)
		{
			return;
		}
		foreach (ZombieTalentData talentDatum2 in talentData)
		{
			if (num2 <= 0)
			{
				break;
			}
			int num3 = talentDatum2.studiedLevelUps.Count - 1;
			while (num3 >= 0 && num2 > 0)
			{
				if (!disabledTalentLevelUps.Contains(talentDatum2.studiedLevelUps[num3]))
				{
					disabledTalentLevelUps.Add(talentDatum2.studiedLevelUps[num3]);
					TalentLevelUpDef data2 = GameBalance.Me.GetData<TalentLevelUpDef>(talentDatum2.studiedLevelUps[num3]);
					if (!string.IsNullOrEmpty(data2.linkedPerk))
					{
						RemovePerk(data2.linkedPerk);
					}
					GetTalentBranch(data2.talentId).curTalentValue -= data2.talentValueAdd;
					num2--;
				}
				num3--;
			}
		}
	}

	public TalentLevelUpDef.State GetLevelUpState(TalentLevelUpDef def)
	{
		if (GetTalentBranch(def.talentId).studiedLevelUps.Contains(def.id))
		{
			return TalentLevelUpDef.State.Unlocked;
		}
		if (MainGame.Instance.GameSave.knowledgeSystem.hiddenTalentLevelUps.Contains(def.id))
		{
			return TalentLevelUpDef.State.Hidden;
		}
		if (MainGame.Instance.GameSave.knowledgeSystem.unknownTalentLevelUps.Contains(def.id))
		{
			return TalentLevelUpDef.State.Unknown;
		}
		if (IsParentsUnlockedForTalentLevelUp(def) && IsEnoughResourcesToBuyTalentLevelUp(def) && IsEnoughFreeSkullsToBuyTalentLevelUp(def))
		{
			return TalentLevelUpDef.State.Available;
		}
		return TalentLevelUpDef.State.Visible;
	}

	public bool IsParentsUnlockedForTalentLevelUp(TalentLevelUpDef def)
	{
		bool result = false;
		ZombieTalentData talentBranch = GetTalentBranch(def.talentId);
		switch (def.lockType)
		{
		case TalentLevelUpDef.LockType.All:
			result = true;
			foreach (string parent in def.parents)
			{
				if (!talentBranch.studiedLevelUps.Contains(parent))
				{
					result = false;
					break;
				}
			}
			break;
		case TalentLevelUpDef.LockType.Any:
			foreach (string parent2 in def.parents)
			{
				if (talentBranch.studiedLevelUps.Contains(parent2))
				{
					result = true;
				}
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return result;
	}

	public bool IsEnoughResourcesToBuyTalentLevelUp(TalentLevelUpDef def)
	{
		if (def.techRed > 0)
		{
			return techRed >= def.techRed;
		}
		if (def.techBlue > 0)
		{
			return techBlue >= def.techBlue;
		}
		if (def.techGreen > 0)
		{
			return techGreen >= def.techGreen;
		}
		return true;
	}

	public bool IsEnoughFreeSkullsToBuyTalentLevelUp(TalentLevelUpDef def)
	{
		int usedPerksCount = GetUsedPerksCount();
		int redSkulls = RedSkulls;
		return usedPerksCount < redSkulls;
	}

	public void PurchaseTalentLevelUp(TalentLevelUpDef def, bool free = false)
	{
		ZombieTalentData talentBranch = GetTalentBranch(def.talentId);
		talentBranch.studiedLevelUps.Add(def.id);
		talentBranch.curTalentValue += def.talentValueAdd;
		if (!string.IsNullOrEmpty(def.linkedPerk))
		{
			AddPerk(def.linkedPerk);
		}
		if (!free)
		{
			if (def.techRed > 0)
			{
				techRed -= def.techRed;
			}
			if (def.techBlue > 0)
			{
				techBlue -= def.techBlue;
			}
			if (def.techGreen > 0)
			{
				techGreen -= def.techGreen;
			}
		}
		ZombieWgoData.OnTalentLevelUpPurchased?.Invoke();
		if (zombieType == ZombieType.Crafter)
		{
			CrafterTryPlaceOrderForCurrentCraftOrStartIt();
		}
		foreach (LazyExpression item in def.expressionsOnBuy)
		{
			item.Evaluate();
		}
	}

	public int GetUsedPerksCount()
	{
		int num = 0;
		foreach (ZombieTalentData talentDatum in talentData)
		{
			num += talentDatum.studiedLevelUps.Count;
		}
		return num;
	}

	private void SyncTalentLevelUpsFromBalance()
	{
		RemoveMissingTalentLevelUps(disabledTalentLevelUps);
		foreach (ZombieTalentData talentDatum in talentData)
		{
			RemoveMissingTalentLevelUps(talentDatum.studiedLevelUps);
		}
		foreach (TalentLevelUpDef talentLevelUpDef in GameBalance.Me.talentLevelUpDefs)
		{
			if (!talentLevelUpDef.isZombiePerk)
			{
				continue;
			}
			ZombieTalentData talentBranch = GetTalentBranch(talentLevelUpDef.talentId);
			if (talentBranch == null)
			{
				continue;
			}
			if (talentLevelUpDef.availableAtStart && !talentBranch.studiedLevelUps.Contains(talentLevelUpDef.id))
			{
				talentBranch.studiedLevelUps.Add(talentLevelUpDef.id);
				talentBranch.curTalentValue += talentLevelUpDef.talentValueAdd;
				if (GetUsedPerksCount() - disabledTalentLevelUps.Count > RedSkulls)
				{
					disabledTalentLevelUps.Add(talentLevelUpDef.id);
					talentBranch.curTalentValue -= talentLevelUpDef.talentValueAdd;
				}
			}
			if (talentBranch.studiedLevelUps.Contains(talentLevelUpDef.id) && !disabledTalentLevelUps.Contains(talentLevelUpDef.id) && !string.IsNullOrEmpty(talentLevelUpDef.linkedPerk) && !HasPerk(talentLevelUpDef.linkedPerk))
			{
				AddPerk(talentLevelUpDef.linkedPerk);
			}
		}
	}

	private static void RemoveMissingTalentLevelUps(List<string> ids)
	{
		for (int num = ids.Count - 1; num >= 0; num--)
		{
			if (GameBalance.Me.GetData<TalentLevelUpDef>(ids[num]) == null)
			{
				ids.RemoveAt(num);
			}
		}
	}

	public void CrafterAddCraftDrop(Item drop)
	{
		PickupOrder orderBase = new PickupOrder(base.UniqueId, drop);
		base.WorldZoneData.PlaceNewOrder(orderBase);
		WorkerInventory.AddItemToInventory(drop);
		crafterOrderedCraftId = string.Empty;
	}

	public void CrafterOnAttachedWgoCraftEnd(CraftElementBase ce)
	{
		CrafterStopCraftActivity();
		CrafterTryPlaceOrderForCurrentCraftOrStartIt();
	}

	public void CrafterOnOrderExecuted(OrderBase order)
	{
		if (order is DeliveryOrder deliveryOrder)
		{
			CrafterOnDeliveryOrderExecuted(deliveryOrder);
			return;
		}
		if (order is PickupOrder pickupOrder)
		{
			CrafterOnPickupOrderExecuted(pickupOrder);
			return;
		}
		throw new ArgumentOutOfRangeException("order", order, null);
	}

	private void CrafterOnPickupOrderExecuted(PickupOrder pickupOrder)
	{
		foreach (SGuid crafterOrder in crafterOrders)
		{
			if (pickupOrder.UniqueId != crafterOrder)
			{
				return;
			}
		}
		CrafterTryPlaceOrderForCurrentCraftOrStartIt();
	}

	public void CrafterFinishAndContinueAfterBigItemDropped()
	{
		WgoData wgoData = AttachedWgoData;
		PickupOrder pickupOrder = new PickupOrder(base.UniqueId, Item.Empty);
		base.WorldZoneData.PlaceNewOrder(pickupOrder);
		CraftDefBase craftDefBase = null;
		GameRes gameRes = null;
		if (wgoData.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp)
		{
			if (wgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.GetInt("auto_start_same_craft_after_pickup") == 1)
			{
				craftDefBase = wgoData.CraftComponent.CurrentCraftElement.ParamsData.CraftDef;
				gameRes = wgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Clone();
			}
			wgoData.CraftComponent.TryFinishCurCraft();
			wgoData.DropStoredTechPoints();
		}
		base.WorldZoneData.RemoveOrder(pickupOrder.UniqueId);
		CrafterOnOrderExecuted(pickupOrder);
		if (craftDefBase != null)
		{
			CraftParamsData craftParamsData = new CraftParamsData(wgoData.CraftComponent.AvailableCrafts[0].id, wgoData);
			if (gameRes != null)
			{
				craftParamsData.customRes = gameRes.Clone();
			}
			craftParamsData.customRes.Set("auto_start_same_craft_after_pickup", 1f);
			craftParamsData.customRes.Set("do_not_check_multiinventory_space", 1f);
			wgoData.CraftComponent.TryStartCraft(new CraftElementBase(wgoData.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
			CrafterStopCraftActivity();
			CrafterStartCraftActivity();
		}
	}

	private void CrafterOnOrderAdded(OrderBase order)
	{
		if (order.TargetWgoUniqueId == base.UniqueId)
		{
			crafterOrders.Add(order.UniqueId);
			this.CrafterOnOrderAddedEvent?.Invoke();
			UpdateAttachedWgoViewWidgets();
		}
	}

	private void CrafterOnOrderRemoved(OrderBase order)
	{
		if (order.TargetWgoUniqueId == base.UniqueId)
		{
			crafterOrders.Remove(order.UniqueId);
			this.CrafterOnOrderRemovedEvent?.Invoke();
			UpdateAttachedWgoViewWidgets();
		}
	}

	private void CrafterOnDeliveryOrderExecuted(DeliveryOrder deliveryOrder)
	{
		CrafterTryPlaceOrderForCurrentCraftOrStartIt();
	}

	private void CrafterDropCraftInventory()
	{
		foreach (Item item in WorkerInventory.Data.Inventory)
		{
			MainGame.Instance.dropSystem.DropItem(item, base.WorldId, base.Position);
		}
		WorkerInventory.Clear();
	}

	private void CrafterOnCraftAddedToQueue(CraftElementBase craftQueueElement)
	{
		CrafterTryPlaceOrderForCurrentCraftOrStartIt();
	}

	private void CrafterOnCraftRemovedFromQueue(CraftElementBase craftQueueElement)
	{
		if (crafterOrderedCraftId == craftQueueElement.CraftId && craftQueueElement.Count == 0 && !craftQueueElement.IsFinished)
		{
			CrafterDropCraftInventory();
			crafterOrderedCraftId = string.Empty;
			base.WorldZoneData.ClearOrders(new List<SGuid>(crafterOrders));
			CrafterTryPlaceOrderForCurrentCraftOrStartIt();
		}
		if (AttachedWgoData.CraftComponent.CurrentCraftElement == null && !AttachedWgoData.CraftComponent.HasCraftsInQueue)
		{
			CrafterStopCraftActivity();
		}
	}

	public void TryResumeCrafterWorkAfterLoad()
	{
		if (zombieType == ZombieType.Crafter && AttachedWgoData != null && ZombieCraftActivity == null && AttachedWgoData.CraftComponent.HasCraftsInQueue)
		{
			CrafterTryPlaceOrderForCurrentCraftOrStartIt();
		}
	}

	private void CrafterTryPlaceOrderForCurrentCraftOrStartIt()
	{
		CraftElementBase currentCraftElement = AttachedWgoData.CraftComponent.CurrentCraftElement;
		if (currentCraftElement == null || currentCraftElement.IsStarted || crafterOrders.Count > 0 || !CrafterCanUseTool(AttachedWgoData, currentCraftElement.Def) || !CrafterIsEnoughMastery(AttachedWgoData))
		{
			return;
		}
		for (int i = 0; i < currentCraftElement.Requirements.Count; i++)
		{
			NeedItemData needItemData = currentCraftElement.Requirements[i];
			if (!GameBalance.Me.GetData<ItemDef>(needItemData.id).isFuel && !WorkerInventory.Data.HasItemQuantityInInventory(needItemData.id, needItemData.GetCount(AttachedWgoData)))
			{
				DeliveryOrder orderBase = new DeliveryOrder(base.UniqueId, new Item(needItemData.id, needItemData.GetCount(AttachedWgoData)));
				base.WorldZoneData.PlaceNewOrder(orderBase);
				crafterOrderedCraftId = currentCraftElement.CraftId;
				CrafterStopCraftActivity();
			}
		}
		if (CrafterCurrentOrder == null && ZombieCraftActivity == null)
		{
			CrafterStartCraftActivity();
			crafterOrderedCraftId = string.Empty;
		}
	}

	public void CrafterStartCraftActivity(bool resetTicks = true)
	{
		ZombieCraftActivity craftActivity = (ZombieCraftActivity)(currentActivity = new ZombieCraftActivity(AttachedWgoData, this));
		ZombieCraftActivity.OnActiveStateChanged += CrafterOnCraftActivityStateChanged;
		CrafterOnCraftActivityStateChanged();
		MainGame.Instance.craftSystem.AddWorker(craftActivity);
		UpdateAttachedWgoViewWidgets();
		if (resetTicks)
		{
			AttachedWgoData.CraftComponent.ZombieSubTicks = 0;
		}
		AttachedWgoData.CraftComponent.TryContinueFromQueue();
	}

	public void CrafterStopCraftActivity()
	{
		if (ZombieCraftActivity != null)
		{
			MainGame.Instance.craftSystem.RemoveWorker(ZombieCraftActivity);
			if (ZombieCraftActivity != null)
			{
				ZombieCraftActivity.OnActiveStateChanged -= CrafterOnCraftActivityStateChanged;
				CrafterOnCraftActivityStateChanged();
				currentActivity = null;
			}
		}
	}

	public bool CrafterIsEnoughMastery(WgoData wgoData)
	{
		if (AttachedWgoData.CraftComponent.CurrentCraftElement.Def.isStarCraft)
		{
			return true;
		}
		if (AttachedWgoData.CraftComponent.CurrentCraftElement.Def.isAutopsyCraft)
		{
			return true;
		}
		if (AttachedWgoData.CraftComponent.CurrentCraftElement.Def.isPocketExtractCraft)
		{
			return true;
		}
		if (AttachedWgoData.CraftComponent.CurrentCraftElement.Def is SurveyDef)
		{
			return true;
		}
		return GetMasteryLevelForTalentBranch(wgoData.Definition.talent, AttachedWgoData.CraftComponent.CurrentCraftElement?.Def) >= ((AttachedWgoData.CraftComponent.CurrentCraftElement != null) ? AttachedWgoData.CraftComponent.CurrentCraftElement.Def.talentLock : wgoData.Definition.MasteryLock);
	}

	public bool CrafterCanUseTool(WgoData wgoData, CraftDefBase craftDefBase)
	{
		return HasToolForWork(wgoData, craftDefBase);
	}

	private void CrafterOnCraftActivityStateChanged()
	{
		CrafterHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
	}

	public void SetCustomAnimationState(AnimationState newState)
	{
		curAnimState = newState;
		this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
	}

	public void InvokeOnAnimationStateChanged(bool playSound = false)
	{
		this.OnAnimationStateChanged?.Invoke(curAnimState, playSound);
	}

	public void SyncPorterBackpackLayer()
	{
		if (zombieType == ZombieType.Porter)
		{
			float weight = ((GetGameResInt("is_staying_at_porter_station") == 1) ? 0f : 1f);
			SetLayerWeightToAnimator(4, weight);
		}
	}

	private void CrafterHandleCraftStatusChange(CraftComponentStatus craftStatus)
	{
		switch (attachedWgoData.id)
		{
		case "mine_ore_coal_crafter":
			return;
		case "clay_zombie_crafter":
			return;
		case "sand_zombie_crafter":
			return;
		}
		if (craftStatus == CraftComponentStatus.Finished)
		{
			CrafterTryPlaceOrderForCurrentCraftOrStartIt();
		}
		AnimationState animationState = ((ZombieCraftActivity != null && ZombieCraftActivity.IsActive) ? ((craftStatus == CraftComponentStatus.Started) ? ((!AttachedWgoData.Definition.isAutoCrafter) ? CrafterGetAnimationStateForCraft(AttachedWgoData.CraftComponent.CurrentCraftElement.Def) : AnimationState.Idle) : AnimationState.Idle) : AnimationState.Idle);
		if (curAnimState != animationState)
		{
			curAnimState = animationState;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
		}
	}

	private AnimationState CrafterGetAnimationStateForCraft(CraftDefBase craftDefBase)
	{
		if (craftDefBase.isAuto)
		{
			return AnimationState.Idle;
		}
		ItemType itemType = ItemType.None;
		if (craftDefBase is CraftDef { customItemTypeAction: not ItemType.None } craftDef && !craftDefBase.isAuto)
		{
			itemType = craftDef.customItemTypeAction;
		}
		if (itemType == ItemType.None)
		{
			itemType = AttachedWgoData.Definition.toolAction.actionableTool;
		}
		return (AnimationState)(itemType + 19);
	}

	private void CrafterHandleCraftProgressChange(float progress)
	{
		if (!AttachedWgoData.Definition.isAutoCrafter)
		{
			return;
		}
		switch (attachedWgoData.id)
		{
		case "mine_ore_coal_crafter":
			return;
		case "clay_zombie_crafter":
			return;
		case "sand_zombie_crafter":
			return;
		}
		if (progress > 0f && curAnimState != 0)
		{
			curAnimState = AnimationState.Idle;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
		}
	}

	public void CrafterOnToolChanged()
	{
		if (!attachedWgoDataUniqueId.IsEmpty)
		{
			if (AttachedWgoData.CraftComponent.CurrentCraftElement != null && AttachedWgoData.CraftComponent.CurrentCraftElement.IsStarted && ZombieCraftActivity == null)
			{
				CrafterStartCraftActivity(resetTicks: false);
			}
			else if (AttachedWgoData.CraftComponent.HasCraftsInQueue)
			{
				CrafterTryPlaceOrderForCurrentCraftOrStartIt();
			}
			AttachedWgoData.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
			UpdateAttachedWgoViewWidgets();
		}
	}

	private void CaretakerUpdateBehaviour(float deltaTime)
	{
		switch (CaretakerState)
		{
		case ZombieCaretakerState.OnStation:
			CaretakerTryGetNewOrder();
			break;
		case ZombieCaretakerState.GoToStation:
			CaretakerTryGetNewOrder();
			break;
		case ZombieCaretakerState.PickingUpOrderItemFromInventory:
			CaretakerTryPickUpFromInventory(deltaTime);
			break;
		case ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder:
			CaretakerTryGetNewOrder();
			if (!HasCaretakerExecutingOrder)
			{
				CaretakerTryCheckIsInventoryBusyIfInZone();
			}
			break;
		case ZombieCaretakerState.GoToInventoryToPickUpOrderItem:
		case ZombieCaretakerState.GoToInventoryToDeliverOrderItem:
		case ZombieCaretakerState.GoToInventoryToPutPortableItemWithExistingOrder:
			CaretakerTryCheckIsInventoryBusyIfInZone();
			break;
		case ZombieCaretakerState.WaitingOtherCaretakersOnInventory:
			CaretakerTryCheckIsInventoryFree();
			break;
		case ZombieCaretakerState.FailedToFindPath:
			caretakerTimeForCheckFailedPathAgain -= deltaTime;
			CaretakerTryMoveToCurrentTarget();
			break;
		case ZombieCaretakerState.CanNotPutItemToInventory:
			switch (caretakerPreviousState)
			{
			case ZombieCaretakerState.GoToInventoryToDeliverOrderItem:
				CaretakerState = caretakerPreviousState;
				CaretakerTryPutPickedUpOrderItemToInventory();
				break;
			case ZombieCaretakerState.GoToInventoryToPutPortableItemWithExistingOrder:
				CaretakerState = caretakerPreviousState;
				CaretakerTryPutPortableItemToInventoryWithExistingOrder();
				break;
			case ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder:
				CaretakerState = caretakerPreviousState;
				CaretakerTryPutPortableItemToInventoryWithoutExistingOrder();
				break;
			}
			break;
		case ZombieCaretakerState.GoToZombieToDeliverOrderItem:
		case ZombieCaretakerState.GoToZombieToPickUpOrderItem:
			break;
		}
	}

	public override void OnPathComplete(MovementComponent component)
	{
		switch (ZombieType)
		{
		case ZombieType.Porter:
			PorterOnPathComplete(component);
			break;
		case ZombieType.Caretaker:
			caretakerCurrentMovementTargetUniqueId = SGuid.Empty;
			base.OnPathComplete(component);
			if (CaretakerState != ZombieCaretakerState.WaitingOtherCaretakersOnInventory)
			{
				if (component.Completion == MovementComponent.CompletionState.Success)
				{
					CaretakerOnPathSuccess();
				}
				else if (component.Completion != MovementComponent.CompletionState.Canceled)
				{
					caretakerTimeForCheckFailedPathAgain = 1f;
					CaretakerState = ZombieCaretakerState.FailedToFindPath;
				}
			}
			break;
		case ZombieType.Gardener:
			gardenerCurrentMovementTargetUniqueId = SGuid.Empty;
			base.OnPathComplete(component);
			if (component.Completion == MovementComponent.CompletionState.Success)
			{
				GardenerOnPathSuccess();
			}
			else if (component.Completion != MovementComponent.CompletionState.Canceled)
			{
				gardenerTimeForCheckFailedPathAgain = 1f;
				GardenerState = ZombieGardenerState.FailedToFindPath;
			}
			break;
		case ZombieType.ConveyorTransporter:
			conveyorTransporterCurrentMovementTargetUniqueId = SGuid.Empty;
			base.OnPathComplete(component);
			if (component.Completion == MovementComponent.CompletionState.Success)
			{
				ConveyorTransporterOnPathSuccess();
			}
			else if (component.Completion != MovementComponent.CompletionState.Canceled)
			{
				conveyorTransporterTimeForCheckFailedPathAgain = 1f;
				ConveyorTransporterState = ZombieConveyorTransporterState.FailedToFindPath;
			}
			break;
		case ZombieType.Free:
		case ZombieType.Crafter:
		case ZombieType.ConveyorCrafter:
		case ZombieType.Worker:
		case ZombieType.Fighter:
			base.OnPathComplete(component);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void CaretakerOnPathSuccess()
	{
		curAnimState = AnimationState.Idle;
		switch (CaretakerState)
		{
		case ZombieCaretakerState.GoToStation:
		{
			CaretakerState = ZombieCaretakerState.OnStation;
			GDPointData gDPointData = AttachedWgoData.GetGDPointData("zombie_porter_station_gd_point");
			base.Position = gDPointData.Position;
			direction.Value = gDPointData.Direction.ConvertToVector2XZ();
			break;
		}
		case ZombieCaretakerState.GoToInventoryToPickUpOrderItem:
			CaretakerState = ZombieCaretakerState.PickingUpOrderItemFromInventory;
			curAnimState = AnimationState.WorkHands;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: true);
			break;
		case ZombieCaretakerState.GoToZombieToDeliverOrderItem:
			CaretakerTryExecuteDeliveryOrder();
			break;
		case ZombieCaretakerState.GoToZombieToPickUpOrderItem:
			CaretakerTryExecutePickUpOrder();
			break;
		case ZombieCaretakerState.GoToInventoryToDeliverOrderItem:
			CaretakerTryPutPickedUpOrderItemToInventory();
			break;
		case ZombieCaretakerState.GoToInventoryToPutPortableItemWithExistingOrder:
			CaretakerTryPutPortableItemToInventoryWithExistingOrder();
			break;
		case ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder:
			CaretakerTryPutPortableItemToInventoryWithoutExistingOrder();
			break;
		case ZombieCaretakerState.PickingUpOrderItemFromInventory:
			break;
		}
	}

	private void CaretakerTryGetNewOrder()
	{
		if (!HasCaretakerExecutingOrder)
		{
			OrderBase orderForCaretaker = base.WorldZoneData.GetOrderForCaretaker(CaretakerPortableItem);
			if (orderForCaretaker != null)
			{
				caretakerExecutingOrder = orderForCaretaker.UniqueId;
				orderForCaretaker.ExecutorUniqueId = base.UniqueId;
				CaretakerTryStartOrderExecutionOrGoToStation();
			}
		}
	}

	private void CaretakerTryGetNewOrderOrMoveToStation()
	{
		CaretakerTryGetNewOrder();
		if (!HasCaretakerExecutingOrder)
		{
			CaretakerTryMoveToStation();
		}
	}

	private void CaretakerOnOrderRemoved(OrderBase order)
	{
		if (order.UniqueId == caretakerExecutingOrder)
		{
			if (CaretakerState == ZombieCaretakerState.FailedToFindPath)
			{
				CaretakerState = caretakerPreviousState;
				caretakerTimeForCheckFailedPathAgain = 0f;
			}
			if (CaretakerState == ZombieCaretakerState.CanNotPutItemToInventory)
			{
				CaretakerState = caretakerPreviousState;
			}
			switch (CaretakerState)
			{
			case ZombieCaretakerState.GoToInventoryToPickUpOrderItem:
				CaretakerTryStopOrderExecution();
				CaretakerTryGetNewOrderOrMoveToStation();
				break;
			case ZombieCaretakerState.PickingUpOrderItemFromInventory:
				caretakerPickingUpFromInventoryTime = 0f;
				CaretakerTryStopOrderExecution();
				CaretakerTryGetNewOrderOrMoveToStation();
				break;
			case ZombieCaretakerState.GoToZombieToDeliverOrderItem:
				CaretakerTryStopOrderExecution();
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				CaretakerTryMoveToNearestInventoryToPutPortableItem();
				break;
			case ZombieCaretakerState.GoToZombieToPickUpOrderItem:
				CaretakerTryStopOrderExecution();
				CaretakerTryGetNewOrderOrMoveToStation();
				break;
			case ZombieCaretakerState.GoToInventoryToDeliverOrderItem:
				CaretakerTryStopOrderExecution();
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				break;
			case ZombieCaretakerState.GoToInventoryToPutPortableItemWithExistingOrder:
				CaretakerTryStopOrderExecution();
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				break;
			case ZombieCaretakerState.WaitingOtherCaretakersOnInventory:
				CaretakerTryStopOrderExecution();
				CaretakerTryGetNewOrderOrMoveToStation();
				break;
			case ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder:
				break;
			}
		}
	}

	private void CaretakerTryPickUpFromInventory(float deltaTime)
	{
		caretakerPickingUpFromInventoryTime += deltaTime;
		LazyExpression lazyExpression = new LazyExpression(ConstDef.Get("zombie_caretaker_picking_up_time").StringValue);
		if (!(caretakerPickingUpFromInventoryTime >= lazyExpression.EvaluateFloat(this)))
		{
			return;
		}
		caretakerPickingUpFromInventoryTime = 0f;
		Item item = CaretakerExecutingOrder.Item;
		bool isEmpty = CaretakerPortableItem.IsEmpty;
		int num = ((!isEmpty) ? CaretakerPortableItem.Count : 0);
		int num2 = item.Count - num;
		WgoData caretakerCurrentTarget = CaretakerCurrentTarget;
		int totalCountInInventory = caretakerCurrentTarget.Inventory.Data.GetTotalCountInInventory(item.id);
		int num3 = 0;
		if (totalCountInInventory >= num2)
		{
			caretakerCurrentTarget.Inventory.RemoveItemById(item.id, num2);
			if (isEmpty)
			{
				CaretakerPortableItem = new Item(item.id, item.Count);
			}
			else
			{
				CaretakerPortableItem.Count += num2;
				CaretakerPortableItem = CaretakerPortableItem;
			}
			CaretakerState = ZombieCaretakerState.GoToZombieToDeliverOrderItem;
			CaretakerTryMoveToZombie();
			return;
		}
		foreach (WgoData multiInventoryWgoData in base.WorldZoneData.MultiInventoryWgoDatas)
		{
			if (multiInventoryWgoData != caretakerCurrentTarget)
			{
				num3 += multiInventoryWgoData.Inventory.Data.GetTotalCountInInventory(item.id);
			}
		}
		if (totalCountInInventory + num3 >= num2)
		{
			caretakerCurrentTarget.Inventory.RemoveItemById(item.id, totalCountInInventory);
			if (isEmpty)
			{
				CaretakerPortableItem = new Item(item.id, totalCountInInventory);
			}
			else
			{
				CaretakerPortableItem.Count += totalCountInInventory;
				CaretakerPortableItem = CaretakerPortableItem;
			}
			CaretakerState = ZombieCaretakerState.GoToInventoryToPickUpOrderItem;
			CaretakerTryMoveToNearestInventoryWithRequiredItemCountToPickUp();
		}
		else
		{
			CaretakerTryStopOrderExecution();
			CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
			CaretakerTryMoveToNearestInventoryToPutPortableItem();
		}
	}

	private void CaretakerTryStartOrderExecutionOrGoToStation()
	{
		OrderBase orderBase = CaretakerExecutingOrder;
		bool isEmpty = CaretakerPortableItem.IsEmpty;
		if (orderBase != null)
		{
			if (orderBase is PickupOrder)
			{
				if (isEmpty)
				{
					CaretakerState = ZombieCaretakerState.GoToZombieToPickUpOrderItem;
					CaretakerTryMoveToZombie();
				}
				else
				{
					CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithExistingOrder;
					CaretakerTryMoveToNearestInventoryToPutPortableItem();
				}
			}
			else
			{
				if (!(orderBase is DeliveryOrder deliveryOrder))
				{
					return;
				}
				if (isEmpty)
				{
					CaretakerState = ZombieCaretakerState.GoToInventoryToPickUpOrderItem;
					CaretakerTryMoveToNearestInventoryWithRequiredItemCountToPickUp();
				}
				else if (CaretakerPortableItem.id == deliveryOrder.Item.id)
				{
					if (CaretakerPortableItem.Count >= deliveryOrder.Item.Count)
					{
						CaretakerState = ZombieCaretakerState.GoToZombieToDeliverOrderItem;
						CaretakerTryMoveToZombie();
					}
					else
					{
						CaretakerState = ZombieCaretakerState.GoToInventoryToPickUpOrderItem;
						CaretakerTryMoveToNearestInventoryWithRequiredItemCountToPickUp();
					}
				}
				else
				{
					CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithExistingOrder;
					CaretakerTryMoveToNearestInventoryToPutPortableItem();
				}
			}
		}
		else
		{
			CaretakerTryMoveToStation();
		}
	}

	private void CaretakerTryStopOrderExecution()
	{
		if (!caretakerExecutingOrder.IsEmpty)
		{
			if (CaretakerExecutingOrder != null)
			{
				CaretakerExecutingOrder.ExecutorUniqueId = SGuid.Empty;
			}
			caretakerExecutingOrder = SGuid.Empty;
		}
	}

	private void CaretakerTryExecuteDeliveryOrder()
	{
		if (HasCaretakerExecutingOrder)
		{
			ZombieWgoData zombieWgoData = CaretakerExecutingOrder.ZombieWgoData;
			if (CaretakerExecutingOrder.TryExecuteOrder(new ZombieCaretakerOrderExecutor(this), out var _))
			{
				OrderBase orderBase = CaretakerExecutingOrder;
				CaretakerTryStopOrderExecution();
				zombieWgoData.WorldZoneData.RemoveOrder(orderBase.UniqueId);
				zombieWgoData.CrafterOnOrderExecuted(orderBase);
				if (CaretakerPortableItem.IsEmpty)
				{
					CaretakerTryGetNewOrderOrMoveToStation();
					return;
				}
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				CaretakerTryMoveToNearestInventoryToPutPortableItem();
			}
			else
			{
				CaretakerTryStopOrderExecution();
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				CaretakerTryMoveToNearestInventoryToPutPortableItem();
			}
		}
		else
		{
			CaretakerTryStopOrderExecution();
			CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
		}
	}

	private void CaretakerTryExecutePickUpOrder()
	{
		if (HasCaretakerExecutingOrder)
		{
			ZombieWgoData zombieWgoData = CaretakerExecutingOrder.ZombieWgoData;
			WgoData wgoData = zombieWgoData.AttachedWgoData;
			CraftDefBase craftDefBase = null;
			GameRes gameRes = null;
			if (wgoData.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp && wgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.GetInt("auto_start_same_craft_after_pickup") == 1)
			{
				craftDefBase = wgoData.CraftComponent.CurrentCraftElement.ParamsData.CraftDef;
				gameRes = wgoData.CraftComponent.CurrentCraftElement.ParamsData.customRes.Clone();
			}
			if (CaretakerExecutingOrder.TryExecuteOrder(new ZombieCaretakerOrderExecutor(this), out var _))
			{
				OrderBase orderBase = CaretakerExecutingOrder;
				CaretakerState = ZombieCaretakerState.GoToInventoryToDeliverOrderItem;
				CaretakerTryMoveToNearestInventoryToPutPickedUpOrderItem();
				CaretakerTryStopOrderExecution();
				zombieWgoData.WorldZoneData.RemoveOrder(orderBase.UniqueId);
				zombieWgoData.CrafterOnOrderExecuted(orderBase);
				if (craftDefBase != null)
				{
					CraftParamsData craftParamsData = new CraftParamsData(wgoData.CraftComponent.AvailableCrafts[0].id, wgoData);
					if (gameRes != null)
					{
						craftParamsData.customRes = gameRes.Clone();
					}
					craftParamsData.customRes.Set("auto_start_same_craft_after_pickup", 1f);
					craftParamsData.customRes.Set("do_not_check_multiinventory_space", 1f);
					wgoData.CraftComponent.TryStartCraft(new CraftElementBase(wgoData.CraftComponent.AvailableCrafts[0].id, 1, craftParamsData));
					zombieWgoData.CrafterStopCraftActivity();
					zombieWgoData.CrafterStartCraftActivity();
				}
			}
			else
			{
				CaretakerTryStopOrderExecution();
				CaretakerTryGetNewOrderOrMoveToStation();
			}
		}
		else
		{
			CaretakerTryGetNewOrderOrMoveToStation();
		}
	}

	private void CaretakerTryMoveToStation()
	{
		CaretakerState = ZombieCaretakerState.GoToStation;
		caretakerCurrentTargetUniqueId = AttachedWgoData.UniqueId;
		CaretakerTryMoveToCurrentTarget();
	}

	private void CaretakerTryMoveToNearestInventoryWithRequiredItemCountToPickUp()
	{
		Item item = CaretakerExecutingOrder.Item;
		int num = 0;
		if (!CaretakerPortableItem.IsEmpty)
		{
			num = CaretakerPortableItem.Count;
		}
		if (base.WorldZoneData.CanDeliveryOrderBeTakenOnExecution(CaretakerExecutingOrder as DeliveryOrder, num))
		{
			List<WgoData> list = new List<WgoData>();
			foreach (WgoData multiInventoryWgoData in base.WorldZoneData.MultiInventoryWgoDatas)
			{
				if (multiInventoryWgoData.Inventory.Data.GetTotalCountInInventory(item.id) >= item.Count - num)
				{
					list.Add(multiInventoryWgoData);
				}
			}
			float num2 = float.MaxValue;
			WgoData wgoData = null;
			if (list.Count > 0)
			{
				foreach (WgoData item2 in list)
				{
					float num3 = Mathf.Abs(Vector3.Distance(item2.Position, base.Position));
					if (num3 < num2)
					{
						num2 = num3;
						wgoData = item2;
					}
				}
			}
			else
			{
				List<WgoData> list2 = new List<WgoData>();
				foreach (WgoData multiInventoryWgoData2 in base.WorldZoneData.MultiInventoryWgoDatas)
				{
					if (multiInventoryWgoData2.Inventory.Data.GetTotalCountInInventory(item.id) >= 0)
					{
						list2.Add(multiInventoryWgoData2);
					}
				}
				foreach (WgoData item3 in list2)
				{
					float num4 = Mathf.Abs(Vector3.Distance(item3.Position, base.Position));
					if (num4 < num2)
					{
						num2 = num4;
						wgoData = item3;
					}
				}
			}
			caretakerCurrentTargetUniqueId = wgoData.UniqueId;
			CaretakerTryMoveToCurrentTarget();
		}
		else
		{
			CaretakerTryStopOrderExecution();
			CaretakerTryGetNewOrderOrMoveToStation();
		}
	}

	private void CaretakerTryMoveToNearestInventoryToPutPortableItem()
	{
		WgoData wgoData = CaretakerGetNearestInventoryWithSpaceForPortableItem();
		if (wgoData == null)
		{
			caretakerCurrentTargetUniqueId = SGuid.Empty;
			CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
		}
		else
		{
			caretakerCurrentTargetUniqueId = wgoData.UniqueId;
			CaretakerTryMoveToCurrentTarget();
		}
	}

	private void CaretakerTryMoveToNearestInventoryToPutPickedUpOrderItem()
	{
		WgoData wgoData = CaretakerGetNearestInventoryWithSpaceForPortableItem();
		if (wgoData == null)
		{
			caretakerCurrentTargetUniqueId = SGuid.Empty;
			CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
		}
		else
		{
			caretakerCurrentTargetUniqueId = wgoData.UniqueId;
			CaretakerTryMoveToCurrentTarget();
		}
	}

	private WgoData CaretakerGetNearestInventoryWithSpaceForPortableItem()
	{
		float num = float.MaxValue;
		WgoData wgoData = null;
		float num2 = float.MaxValue;
		WgoData result = null;
		foreach (WgoData multiInventoryWgoData in base.WorldZoneData.MultiInventoryWgoDatas)
		{
			if (!multiInventoryWgoData.Inventory.CanAddItemToInventory(CaretakerPortableItem))
			{
				continue;
			}
			float num3 = Vector3.Distance(multiInventoryWgoData.Position, base.Position);
			if (multiInventoryWgoData.Inventory.Data.HasItemByItemId(CaretakerPortableItem.id, 1))
			{
				if (num3 < num)
				{
					num = num3;
					wgoData = multiInventoryWgoData;
				}
			}
			else if (num3 < num2)
			{
				num2 = num3;
				result = multiInventoryWgoData;
			}
		}
		if (wgoData == null)
		{
			return result;
		}
		return wgoData;
	}

	private void CaretakerTryMoveToCurrentTarget()
	{
		if (caretakerCurrentMovementTargetUniqueId?.Guid == caretakerCurrentTargetUniqueId?.Guid)
		{
			return;
		}
		if (CaretakerState == ZombieCaretakerState.FailedToFindPath)
		{
			if (caretakerTimeForCheckFailedPathAgain <= 0f)
			{
				CaretakerState = caretakerPreviousState;
				caretakerTimeForCheckFailedPathAgain = 0f;
				MoveAction();
			}
		}
		else
		{
			MoveAction();
		}
		void MoveAction()
		{
			DockPointData nearestDockPointData = CaretakerCurrentTarget.GetNearestDockPointData(base.Position, DockPointData.Availability.All);
			if (base.MovementComponent.IsMoving)
			{
				base.MovementComponent.ForceStop();
			}
			if (CaretakerState == ZombieCaretakerState.OnStation)
			{
				base.Position = AttachedWgoData.GetNearestDockPointDataWorldPositionOrMyPosition(base.Position, DockPointData.Availability.OnlyOccupied, (DockPointData dockPointData, Vector3 parentPos) => !dockPointData.BakedData.DisableTargetingForCaretaker);
				direction.Value = AttachedWgoData.GetNearestDockPointData(base.Position, DockPointData.Availability.All)?.Direction.ConvertToVector2XZ() ?? Direction.Down.ConvertToVector2XZ();
			}
			Vector3 nearestDockPointDataWorldPositionOrMyPosition = CaretakerCurrentTarget.GetNearestDockPointDataWorldPositionOrMyPosition(base.Position, DockPointData.Availability.OnlyOccupied, (DockPointData dockPointData, Vector3 parentPos) => !dockPointData.BakedData.DisableTargetingForCaretaker);
			GraphMask graphMask = NavigationGraphMaskUtils.ToGraphMask(base.WorldZoneData?.MovementGraphs, base.WorldZoneData?.navigationGraph ?? LazyConsts.Navigation.Graph.None);
			switch ((nearestDockPointData != null) ? base.MovementComponent.StartPath(nearestDockPointDataWorldPositionOrMyPosition, nearestDockPointData.BakedData, graphMask, base.WorldId) : base.MovementComponent.StartPath(nearestDockPointDataWorldPositionOrMyPosition, graphMask, base.WorldId))
			{
			case MovementComponent.StartPathResult.Started:
				caretakerCurrentMovementTargetUniqueId = caretakerCurrentTargetUniqueId;
				break;
			case MovementComponent.StartPathResult.AlreadyAtDestinationPoint:
				CaretakerOnPathSuccess();
				break;
			case MovementComponent.StartPathResult.IncorrectMovementType:
				caretakerTimeForCheckFailedPathAgain = 1f;
				CaretakerState = ZombieCaretakerState.FailedToFindPath;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void CaretakerTryMoveToZombie()
	{
		caretakerCurrentTargetUniqueId = MainGame.ZombieSystemData.GetZombie(CaretakerExecutingOrder.TargetWgoUniqueId).AttachedWgoData.UniqueId;
		CaretakerTryMoveToCurrentTarget();
	}

	private void CaretakerTryPutPickedUpOrderItemToInventory()
	{
		if (CaretakerCurrentTarget == null)
		{
			if (CaretakerIsAnyInventoryWithSpaceForPortableItemExists())
			{
				CaretakerState = ZombieCaretakerState.GoToInventoryToDeliverOrderItem;
				CaretakerTryMoveToNearestInventoryToPutPickedUpOrderItem();
			}
			else
			{
				CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
			}
			return;
		}
		CaretakerCurrentTarget.Inventory.AddItemToInventory(CaretakerPortableItem);
		if (CaretakerPortableItem.Count > 0)
		{
			if (CaretakerIsAnyInventoryWithSpaceForPortableItemExists())
			{
				CaretakerState = ZombieCaretakerState.GoToInventoryToDeliverOrderItem;
				CaretakerTryMoveToNearestInventoryToPutPickedUpOrderItem();
			}
			else
			{
				CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
			}
		}
		else
		{
			CaretakerPortableItem = Item.Empty;
			CaretakerTryGetNewOrderOrMoveToStation();
		}
	}

	private void CaretakerTryPutPortableItemToInventoryWithExistingOrder()
	{
		if (CaretakerCurrentTarget == null)
		{
			if (CaretakerIsAnyInventoryWithSpaceForPortableItemExists())
			{
				CaretakerState = ZombieCaretakerState.GoToInventoryToDeliverOrderItem;
				CaretakerTryMoveToNearestInventoryToPutPickedUpOrderItem();
			}
			else
			{
				CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
			}
			return;
		}
		CaretakerCurrentTarget.Inventory.AddItemToInventory(CaretakerPortableItem);
		if (CaretakerPortableItem.Count > 0)
		{
			if (CaretakerIsAnyInventoryWithSpaceForPortableItemExists())
			{
				CaretakerState = ZombieCaretakerState.GoToInventoryToDeliverOrderItem;
				CaretakerTryMoveToNearestInventoryToPutPickedUpOrderItem();
			}
			else
			{
				CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
			}
		}
		else
		{
			CaretakerPortableItem = Item.Empty;
			CaretakerTryStartOrderExecutionOrGoToStation();
		}
	}

	private void CaretakerTryPutPortableItemToInventoryWithoutExistingOrder()
	{
		if (CaretakerCurrentTarget == null)
		{
			if (CaretakerIsAnyInventoryWithSpaceForPortableItemExists())
			{
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				CaretakerTryMoveToNearestInventoryToPutPortableItem();
			}
			else
			{
				CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
			}
			return;
		}
		CaretakerCurrentTarget.Inventory.AddItemToInventory(CaretakerPortableItem);
		if (CaretakerPortableItem.Count > 0)
		{
			if (CaretakerIsAnyInventoryWithSpaceForPortableItemExists())
			{
				CaretakerState = ZombieCaretakerState.GoToInventoryToPutPortableItemWithoutExistingOrder;
				CaretakerTryMoveToNearestInventoryToPutPortableItem();
			}
			else
			{
				CaretakerState = ZombieCaretakerState.CanNotPutItemToInventory;
			}
		}
		else
		{
			CaretakerPortableItem = Item.Empty;
			CaretakerTryMoveToStation();
		}
	}

	private void CaretakerTryCheckIsInventoryBusyIfInZone()
	{
		if (CaretakerCurrentTarget == null || !(Mathf.Abs(Vector3.Distance(CaretakerCurrentTarget.Position, base.Position)) <= 2f))
		{
			return;
		}
		foreach (SGuid zombieOnSceneWgoId in MainGame.ZombieSystemData.zombieOnSceneWgoIds)
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(zombieOnSceneWgoId);
			if (zombie != this && zombie.CaretakerState == ZombieCaretakerState.PickingUpOrderItemFromInventory && zombie.CaretakerCurrentTargetUniqueId == CaretakerCurrentTargetUniqueId)
			{
				CaretakerState = ZombieCaretakerState.WaitingOtherCaretakersOnInventory;
				base.MovementComponent.ForceStop();
			}
		}
	}

	private void CaretakerTryCheckIsInventoryFree()
	{
		foreach (SGuid zombieOnSceneWgoId in MainGame.ZombieSystemData.zombieOnSceneWgoIds)
		{
			ZombieWgoData zombie = MainGame.ZombieSystemData.GetZombie(zombieOnSceneWgoId);
			if (zombie != this && zombie.CaretakerState == ZombieCaretakerState.PickingUpOrderItemFromInventory && zombie.CaretakerCurrentTargetUniqueId == CaretakerCurrentTargetUniqueId)
			{
				return;
			}
		}
		CaretakerState = caretakerPreviousState;
		CaretakerTryMoveToCurrentTarget();
	}

	private bool CaretakerIsAnyInventoryWithSpaceForPortableItemExists()
	{
		return CaretakerGetNearestInventoryWithSpaceForPortableItem() != null;
	}

	public void ConveyorCrafterOnToolChanged()
	{
		if (!attachedWgoDataUniqueId.IsEmpty)
		{
			if (AttachedWgoData.CraftComponent.CurrentCraftElement != null && AttachedWgoData.CraftComponent.CurrentCraftElement.IsStarted && ZombieCraftActivity == null)
			{
				ConveyorCrafterStartCraftActivity(resetTicks: false);
			}
			else if (AttachedWgoData.CraftComponent.HasCraftsInQueue)
			{
				ConveyorCrafterTryStartCurrentCraft();
			}
			AttachedWgoData.CraftComponent.UpdateCanContinueManualCraftState(Time.deltaTime);
			UpdateAttachedWgoViewWidgets();
		}
	}

	public void ConveyorCrafterOnAttachedWgoCraftEnd(CraftElementBase ce)
	{
		if (attachedWgoData.CraftComponent.Status != CraftComponentStatus.Started)
		{
			ConveyorCrafterStopCraftActivity();
		}
	}

	private void ConveyorCrafterOnCraftAddedToQueue(CraftElementBase craftQueueElement)
	{
		ConveyorCrafterTryStartCurrentCraft();
	}

	private void ConveyorCrafterOnCraftRemovedFromQueue(CraftElementBase craftQueueElement)
	{
		if (AttachedWgoData.CraftComponent.CurrentCraftElement == null)
		{
			ConveyorCrafterStopCraftActivity();
		}
	}

	private void ConveyorCrafterTryStartCurrentCraft(List<Item> items = null)
	{
		CraftElementBase currentCraftElement = AttachedWgoData.CraftComponent.CurrentCraftElement;
		if (currentCraftElement != null && AttachedWgoData.CraftComponent.Status != CraftComponentStatus.WaitingForOutputDrop && CrafterCanUseTool(AttachedWgoData, currentCraftElement.Def) && CrafterIsEnoughMastery(AttachedWgoData) && ZombieCraftActivity == null)
		{
			ConveyorCrafterStartCraftActivity();
		}
	}

	private void ConveyorHadleCraftStatusChange(CraftComponentStatus craftStatus)
	{
		craftStatus = AttachedWgoData.CraftComponent.Status;
		switch (craftStatus)
		{
		case CraftComponentStatus.Finished:
			ConveyorCrafterStopCraftActivity();
			break;
		case CraftComponentStatus.Started:
			if (ZombieCraftActivity == null)
			{
				ConveyorCrafterStartCraftActivity();
			}
			break;
		}
	}

	private void ConveyorCrafterHandleCraftStatusChange(CraftComponentStatus craftStatus)
	{
		craftStatus = AttachedWgoData.CraftComponent.Status;
		AnimationState animationState = ((ZombieCraftActivity != null && ZombieCraftActivity.IsActive) ? ((craftStatus == CraftComponentStatus.Started) ? ((!AttachedWgoData.Definition.isAutoCrafter) ? CrafterGetAnimationStateForCraft(AttachedWgoData.CraftComponent.CurrentCraftElement.Def) : AnimationState.Idle) : AnimationState.Idle) : AnimationState.Idle);
		if (curAnimState != animationState)
		{
			curAnimState = animationState;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
		}
	}

	private void ConveyorCrafterStartCraftActivity(bool resetTicks = true)
	{
		ZombieCraftActivity craftActivity = (ZombieCraftActivity)(currentActivity = new ZombieCraftActivity(AttachedWgoData, this));
		ZombieCraftActivity.OnActiveStateChanged += ConveyorCrafterOnCraftActivityStateChanged;
		ConveyorCrafterOnCraftActivityStateChanged();
		MainGame.Instance.conveyorSystem.AddWorker(craftActivity);
		UpdateAttachedWgoViewWidgets();
		if (resetTicks)
		{
			AttachedWgoData.CraftComponent.ZombieSubTicks = 0;
		}
		if (AttachedWgoData.CraftComponent.Status != CraftComponentStatus.WaitingForOutputDrop)
		{
			AttachedWgoData.CraftComponent.TryContinueFromQueue();
		}
	}

	private void ConveyorCrafterStopCraftActivity()
	{
		if (ZombieCraftActivity != null)
		{
			MainGame.Instance.conveyorSystem.RemoveWorker(ZombieCraftActivity);
			if (ZombieCraftActivity != null)
			{
				ZombieCraftActivity.OnActiveStateChanged -= ConveyorCrafterOnCraftActivityStateChanged;
				ConveyorCrafterOnCraftActivityStateChanged();
				currentActivity = null;
			}
		}
	}

	private void ConveyorCrafterOnCraftActivityStateChanged()
	{
		ConveyorCrafterHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
	}

	public bool PorterCheckDeliveryStart()
	{
		if (GetGameResInt("is_staying_at_porter_station") == 1)
		{
			PorterStationDef data = GameBalance.Me.GetData<PorterStationDef>(AttachedWgoData.id);
			if (data != null)
			{
				for (int i = 0; i < base.WorldZoneData.MultiInventoryWgoDatas.Count; i++)
				{
					Inventory inventory = base.WorldZoneData.MultiInventoryWgoDatas[i].Inventory;
					for (int j = 0; j < data.items.Count; j++)
					{
						NeedItemData needItemData = data.items[j];
						if (attachedWgoData.GetGameResInt(needItemData.id) != 1 || (needItemData.ItemDef.itemSize == ItemSize.Big && porterInventory.Data.InventorySize - porterInventory.Data.InventoryFillSize < 2))
						{
							continue;
						}
						int num = inventory.Data.GetTotalCountInInventory(needItemData.id);
						if (num <= 0)
						{
							continue;
						}
						if (needItemData.ItemDef.itemSize == ItemSize.Big)
						{
							do
							{
								porterInventory.AddItemToInventory(new Item("fake_porter_slot_filler"));
								porterInventory.AddItemToInventory(new Item(needItemData.id));
								inventory.RemoveItemById(needItemData.id, 1);
								num--;
							}
							while (num > 0 && porterInventory.Data.InventorySize - porterInventory.Data.InventoryFillSize >= 2);
							continue;
						}
						porterInventory.TryAddItemToInventory(new Item(needItemData.id, num), out var addedItems);
						foreach (Item item in addedItems)
						{
							inventory.RemoveItemById(item.id, item.Count);
						}
					}
				}
				if (porterInventory.Data.InventoryFillSize > 0)
				{
					GDPointData gDPointDataById = MainGame.WorldData.gdPointsData.GetGDPointDataById(base.WorldZoneData.Definition.porterStartPoint);
					GDPointData gDPointDataById2 = MainGame.WorldData.gdPointsData.GetGDPointDataById(base.WorldZoneData.Definition.porterEndPoint);
					base.Position = gDPointDataById.Position;
					switch (base.MovementComponent.StartPath(gDPointDataById2.Position, base.WorldId, base.WorldId, MovementType.GDGraph))
					{
					case MovementComponent.StartPathResult.Started:
						SetGameRes("is_staying_at_porter_station", 0);
						SetGameRes("is_moving_to_target_world_zone", 1);
						AttachedWgoData.SetTriggerToAnimator("path_anim");
						SetLayerWeightToAnimator(4, 1f);
						base.IsInteractable = true;
						return true;
					default:
						throw new ArgumentOutOfRangeException();
					case MovementComponent.StartPathResult.AlreadyAtDestinationPoint:
					case MovementComponent.StartPathResult.IncorrectMovementType:
						break;
					}
				}
			}
		}
		return false;
	}

	private void PorterOnPathComplete(MovementComponent component)
	{
		switch (component.Completion)
		{
		case MovementComponent.CompletionState.Success:
			if (GetGameResInt("is_moving_to_target_world_zone") == 1)
			{
				PorterOnCameToTargetWorldZone();
			}
			else
			{
				PorterOnCameToStationWorldZone();
			}
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MovementComponent.CompletionState.None:
		case MovementComponent.CompletionState.Fail:
		case MovementComponent.CompletionState.Canceled:
			break;
		}
	}

	private void PorterOnCameToTargetWorldZone()
	{
		PorterStationDef data = GameBalance.Me.GetData<PorterStationDef>(AttachedWgoData.id);
		if (data == null)
		{
			return;
		}
		WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById(data.targetWorldZone);
		MultiInventory multiInventory;
		if (data.customTargets.Count > 0)
		{
			List<Inventory> list = new List<Inventory>();
			for (int i = 0; i < data.customTargets.Count; i++)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(data.customTargets[i]);
				if (wgoData != null)
				{
					list.Add(wgoData.Inventory);
					if (wgoData.id == "crates_small" && MainGame.Instance.GameSave.worldData.TryGetWgoData("warehouse_crane", out var foundWgoData, out var _))
					{
						foundWgoData.SetTriggerToAnimator("Work");
					}
				}
			}
			multiInventory = new MultiInventory(list);
		}
		else
		{
			multiInventory = new MultiInventory(worldZoneDataById);
		}
		if (multiInventory.inventoryList.Count > 0)
		{
			List<Item> list2 = new List<Item>();
			foreach (Item item2 in porterInventory.Data.Inventory)
			{
				if (!(item2.id == "fake_porter_slot_filler") && item2.Count > 0)
				{
					Item item = new Item(item2.id, item2.Count);
					int num = TryAddItemPreferringSameItem(multiInventory.inventoryList, item);
					if (num > 0)
					{
						list2.Add(new Item(item2.id, num));
					}
				}
			}
			foreach (Item item3 in list2)
			{
				porterInventory.RemoveItemById(item3.id, item3.Count);
				if (item3.Definition.itemSize == ItemSize.Big)
				{
					porterInventory.RemoveItemById("fake_porter_slot_filler", item3.Count);
				}
			}
		}
		GDPointData gDPointDataById = MainGame.WorldData.gdPointsData.GetGDPointDataById(base.WorldZoneData.Definition.porterStartPoint);
		switch (base.MovementComponent.StartPath(gDPointDataById.Position, base.WorldId, base.WorldId, MovementType.GDGraph))
		{
		case MovementComponent.StartPathResult.Started:
			SetGameRes("is_moving_to_target_world_zone", 0);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case MovementComponent.StartPathResult.AlreadyAtDestinationPoint:
		case MovementComponent.StartPathResult.IncorrectMovementType:
			break;
		}
	}

	private void PorterOnCameToStationWorldZone()
	{
		SetGameRes("is_staying_at_porter_station", 1);
		if (!PorterCheckDeliveryStart())
		{
			GDPointData gDPointData = attachedWgoData.GetGDPointData("zombie_porter_station_gd_point");
			base.Position = gDPointData.Position;
			direction.Value = Direction.Down.ConvertToVector2XZ();
			AttachedWgoData.SetTriggerToAnimator("with_zombie");
			base.IsInteractable = false;
			SetLayerWeightToAnimator(4, 0f);
		}
	}

	private Inventory GetInventoryWithSpacePreferringSameItem(List<Inventory> inventories, Item item)
	{
		Inventory inventory = null;
		foreach (Inventory inventory2 in inventories)
		{
			if (inventory2.Data.CanAddItemCountToInventory(item) > 0)
			{
				if (inventory2.Data.HasItemByItemId(item.id, 1))
				{
					return inventory2;
				}
				if (inventory == null)
				{
					inventory = inventory2;
				}
			}
		}
		return inventory;
	}

	private int TryAddItemPreferringSameItem(List<Inventory> inventories, Item item)
	{
		int num = 0;
		while (item.Count > 0)
		{
			Inventory inventoryWithSpacePreferringSameItem = GetInventoryWithSpacePreferringSameItem(inventories, item);
			if (inventoryWithSpacePreferringSameItem == null)
			{
				break;
			}
			if (item.Definition.stackCount == 1)
			{
				int count = item.Count;
				inventoryWithSpacePreferringSameItem.AddItemToInventory(item);
				int num2 = count - item.Count;
				if (num2 <= 0)
				{
					break;
				}
				num += num2;
				continue;
			}
			int num3 = inventoryWithSpacePreferringSameItem.Data.CanAddItemCountToInventory(item);
			if (num3 > item.Count)
			{
				num3 = item.Count;
			}
			if (num3 <= 0)
			{
				break;
			}
			Item item2 = item.Split(num3);
			inventoryWithSpacePreferringSameItem.AddItemToInventory(item2);
			int num4 = num3 - item2.Count;
			if (item2.Count > 0)
			{
				item.Count += item2.Count;
			}
			if (num4 <= 0)
			{
				break;
			}
			num += num4;
		}
		return num;
	}

	private void GardenerUpdateBehaviour(float deltaTime)
	{
		switch (GardenerState)
		{
		case ZombieGardenerState.OnStation:
			GardenerTryGetNewOrder();
			break;
		case ZombieGardenerState.TeleportSeedsFromMultiInventory:
			GardenerTryTakeSeedsFromMultiInventory();
			break;
		case ZombieGardenerState.PlantingSeeds:
			if ((AttachedWgoData != null && attachedWgoData.CraftComponent.CurrentCraftElement != null && !attachedWgoData.CraftComponent.CurrentCraftElement.Def.id.Contains("_planting")) || AttachedWgoData == null || AttachedWgoData.CraftComponent.CurrentCraftElement == null)
			{
				GardenerStopCraftActivity();
			}
			break;
		case ZombieGardenerState.GatheringPlants:
			if (AttachedWgoData != null && AttachedWgoData.HpComponent.isDeathDelayed)
			{
				GardenerState = ZombieGardenerState.WaitingForWgoDeath;
			}
			else if (AttachedWgoData == null || (AttachedWgoData.HpComponent.Hp == 0 && !AttachedWgoData.HpComponent.isDeathDelayed))
			{
				GardenerStopWorkActivity();
			}
			break;
		case ZombieGardenerState.WaitingForWgoDeath:
			if (AttachedWgoData == null || !AttachedWgoData.HpComponent.isDeathDelayed)
			{
				if (ZombieHPActivity != null)
				{
					GardenerStopWorkActivity();
				}
				else
				{
					GardenerTryGetNewOrderOrMoveToStation();
				}
			}
			break;
		case ZombieGardenerState.FailedToFindPath:
			gardenerTimeForCheckFailedPathAgain -= deltaTime;
			GardenerTryMoveToCurrentTarget();
			break;
		case ZombieGardenerState.CanNotPutItemToInventory:
			GardenerTryPutGardenItemsToMultiInventory();
			break;
		case ZombieGardenerState.GoToStation:
		case ZombieGardenerState.GoToGardenBedToPlantSeeds:
		case ZombieGardenerState.GoToGardenBedToTakePlants:
			break;
		}
	}

	private void GardenerOnPathSuccess()
	{
		curAnimState = AnimationState.Idle;
		switch (GardenerState)
		{
		case ZombieGardenerState.GoToStation:
		{
			GDPointData gDPointData = AttachedWgoData.GetGDPointData("zombie_garden_crafter_gd_point");
			base.Position = gDPointData.Position;
			direction.Value = gDPointData.Direction.ConvertToVector2XZ();
			GardenerTryPutGardenItemsToMultiInventory();
			break;
		}
		case ZombieGardenerState.GoToGardenBedToPlantSeeds:
			curAnimState = AnimationState.WorkHands;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
			GardenerTryStartPlanting();
			break;
		case ZombieGardenerState.GoToGardenBedToTakePlants:
			curAnimState = AnimationState.Idle;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
			GardenerTryStartGathering();
			break;
		case ZombieGardenerState.TeleportSeedsFromMultiInventory:
		case ZombieGardenerState.PlantingSeeds:
			break;
		}
	}

	private void GardenerTryGetNewOrder(Type orderType = null, SGuid target = null)
	{
		if (!Hand.IsEmpty && Hand.Definition.type == ItemType.Shovel && !HasGardenerExecutingOrder)
		{
			OrderBase orderForGardener = base.WorldZoneData.GetOrderForGardener(this, orderType, target);
			if (orderForGardener != null)
			{
				gardenerExecutingOrder = orderForGardener.UniqueId;
				orderForGardener.ExecutorUniqueId = base.UniqueId;
				GardenerTryStartOrderExecutionOrGoToStation();
			}
		}
	}

	private void GardenerTryGetNewOrderOrMoveToStation()
	{
		GardenerTryGetNewOrder();
		if (!HasGardenerExecutingOrder)
		{
			GardenerTryMoveToStation();
		}
	}

	private void GardenerTryGetNewOrderFromPrevTargetOrMoveToStation(Type orderType, SGuid target)
	{
		GardenerTryGetNewOrder(orderType, target);
		if (!HasGardenerExecutingOrder)
		{
			GardenerTryMoveToStation();
		}
	}

	private void GardenerOnOrderRemoved(OrderBase order)
	{
		if (order.UniqueId == gardenerExecutingOrder)
		{
			if (GardenerState == ZombieGardenerState.FailedToFindPath)
			{
				GardenerState = gardenerPreviousState;
				gardenerTimeForCheckFailedPathAgain = 0f;
			}
			if (GardenerState == ZombieGardenerState.CanNotPutItemToInventory)
			{
				GardenerState = gardenerPreviousState;
			}
			ZombieGardenerState zombieGardenerState = GardenerState;
			if (zombieGardenerState == ZombieGardenerState.GoToGardenBedToPlantSeeds || zombieGardenerState == ZombieGardenerState.GoToGardenBedToTakePlants)
			{
				GardenerTryStopOrderExecution();
				GardenerTryGetNewOrderOrMoveToStation();
			}
		}
	}

	private void GardenerTryStartOrderExecutionOrGoToStation()
	{
		OrderBase orderBase = GardenerExecutingOrder;
		if (orderBase != null)
		{
			if (orderBase is PlantOrder)
			{
				GardenerState = ZombieGardenerState.TeleportSeedsFromMultiInventory;
				GardenerTryTakeSeedsFromMultiInventory();
			}
			else if (orderBase is GatherOrder)
			{
				GardenerState = ZombieGardenerState.GoToGardenBedToTakePlants;
				gardenerCurrentTargetUniqueId = orderBase.TargetWgoUniqueId;
				GardenerTryMoveToCurrentTarget();
			}
		}
		else
		{
			GardenerTryMoveToStation();
		}
	}

	private void GardenerTryTakeSeedsFromMultiInventory()
	{
		OrderBase orderBase = GardenerExecutingOrder;
		if (orderBase == null)
		{
			GardenerTryStopOrderExecution();
			GardenerState = ZombieGardenerState.GoToStation;
			GardenerTryMoveToStation();
			return;
		}
		WgoData wgoData = MainGame.WorldData.GetWgoData(orderBase.TargetWgoUniqueId);
		if (wgoData == null)
		{
			GardenerTryStopOrderExecution();
			base.WorldZoneData.RemoveOrder(orderBase.UniqueId);
			GardenerState = ZombieGardenerState.GoToStation;
			GardenerTryMoveToStation();
			return;
		}
		if (wgoData.CraftComponent.IsStarted)
		{
			GardenerState = ZombieGardenerState.GoToGardenBedToPlantSeeds;
			gardenerCurrentTargetUniqueId = orderBase.TargetWgoUniqueId;
			GardenerTryMoveToCurrentTarget();
			return;
		}
		if (base.WorldZoneData.CanPlantOrderBeTakenOnExecution(orderBase as PlantOrder, out var enoughItemId))
		{
			ItemDef data = GameBalance.Me.GetData<ItemDef>(enoughItemId);
			int num = orderBase.Item.Count;
			foreach (WgoData multiInventoryWgoData in base.WorldZoneData.MultiInventoryWgoDatas)
			{
				if (!multiInventoryWgoData.Definition.inventoryWhiteList.Contains(data) || multiInventoryWgoData.Definition.inventoryBlackList.Contains(data))
				{
					continue;
				}
				int totalCountInInventory = multiInventoryWgoData.Inventory.Data.GetTotalCountInInventory(enoughItemId);
				if (totalCountInInventory > 0)
				{
					totalCountInInventory = Mathf.Min(totalCountInInventory, num);
					num -= totalCountInInventory;
					WorkerInventory.AddItemsToInventory(multiInventoryWgoData.Inventory.RemoveItemById(enoughItemId, totalCountInInventory));
					if (num == 0)
					{
						GardenerState = ZombieGardenerState.GoToGardenBedToPlantSeeds;
						gardenerCurrentTargetUniqueId = orderBase.TargetWgoUniqueId;
						GardenerTryMoveToCurrentTarget();
						return;
					}
				}
			}
		}
		GardenerState = ZombieGardenerState.GoToStation;
		GardenerTryMoveToStation();
	}

	private void GardenerTryStopOrderExecution()
	{
		if (!gardenerExecutingOrder.IsEmpty)
		{
			if (GardenerExecutingOrder != null)
			{
				GardenerExecutingOrder.ExecutorUniqueId = SGuid.Empty;
			}
			gardenerExecutingOrder = SGuid.Empty;
		}
	}

	private void GardenerTryStartPlanting()
	{
		if (HasGardenerExecutingOrder)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(GardenerExecutingOrder.TargetWgoUniqueId);
			if (wgoData != null)
			{
				if (!wgoData.CraftComponent.IsStarted)
				{
					List<Item> itemsByGroupId = WorkerInventory.GetItemsByGroupId("seed");
					Item item = null;
					if (GardenerExecutingOrder is PlantOrder plantOrder)
					{
						foreach (Item item2 in itemsByGroupId)
						{
							if (!plantOrder.isStarGroupItem && item2.id == plantOrder.Item.id)
							{
								item = item2;
								break;
							}
							if (plantOrder.isStarGroupItem && GameBalance.Me.starGroupItemsCache[plantOrder.Item.id].Contains(item2.Definition))
							{
								item = item2;
								break;
							}
						}
					}
					if (item == null || item.IsEmpty)
					{
						GardenerTryStopOrderExecution();
						GardenerState = ZombieGardenerState.GoToStation;
						GardenerTryMoveToStation();
						return;
					}
					CraftDefBase cropCraft = GardenInteractionHandler.TryFindGardenCraft(item, wgoData);
					wgoData.TrySetWorker(this);
					GardenInteractionHandler.TryApplySeed(item, cropCraft, wgoData);
					wgoData.ClearWorker();
				}
				GardenerStartCraftActivity();
			}
			else
			{
				OrderBase orderBase = GardenerExecutingOrder;
				GardenerTryStopOrderExecution();
				if (orderBase != null)
				{
					base.WorldZoneData.RemoveOrder(orderBase.UniqueId);
				}
				GardenerState = ZombieGardenerState.GoToStation;
				GardenerTryMoveToStation();
			}
		}
		else
		{
			GardenerTryStopOrderExecution();
			GardenerState = ZombieGardenerState.GoToStation;
			GardenerTryMoveToStation();
		}
	}

	private void GardenerTryStartGathering()
	{
		if (HasGardenerExecutingOrder)
		{
			if (MainGame.WorldData.GetWgoData(GardenerExecutingOrder.TargetWgoUniqueId) != null)
			{
				GardenerState = ZombieGardenerState.GatheringPlants;
				GardenerStartWorkActivity();
				return;
			}
			OrderBase orderBase = GardenerExecutingOrder;
			GardenerTryStopOrderExecution();
			if (orderBase != null)
			{
				base.WorldZoneData.RemoveOrder(orderBase.UniqueId);
			}
			GardenerState = ZombieGardenerState.GoToStation;
			GardenerTryMoveToStation();
		}
		else
		{
			GardenerTryStopOrderExecution();
			GardenerState = ZombieGardenerState.GoToStation;
			GardenerTryMoveToStation();
		}
	}

	public void GardenerStartCraftActivity(bool resetTicks = true)
	{
		WgoData wgoData = (attachedWgoData = MainGame.WorldData.GetWgoData(GardenerExecutingOrder.TargetWgoUniqueId));
		attachedWgoDataUniqueId.SetGuid(wgoData.UniqueId);
		ZombieCraftActivity craftActivity = (ZombieCraftActivity)(currentActivity = new ZombieCraftActivity(AttachedWgoData, this));
		ZombieCraftActivity.OnActiveStateChanged += GardenerOnCraftActivityStateChanged;
		GardenerOnCraftActivityStateChanged();
		attachedWgoData.TrySetWorker(this);
		MainGame.Instance.craftSystem.AddWorker(craftActivity);
		UpdateAttachedWgoViewWidgets();
		if (resetTicks)
		{
			AttachedWgoData.CraftComponent.ZombieSubTicks = 0;
		}
		AttachedWgoData.CraftComponent.TryContinueFromQueue();
		if (GardenBedNavigation.TryGetOpenApproach(AttachedWgoData, base.Position, out var _, out var direction))
		{
			base.direction.Value = direction.ConvertToVector2XZ();
		}
		else
		{
			DockPointData nearestDockPointData = AttachedWgoData.GetNearestDockPointData(base.Position, DockPointData.Availability.All);
			if (nearestDockPointData != null)
			{
				base.direction.Value = nearestDockPointData.Direction.ConvertToVector2XZ();
			}
		}
		GardenerState = ZombieGardenerState.PlantingSeeds;
	}

	public void GardenerStopCraftActivity(bool stopOnly = false)
	{
		if (ZombieCraftActivity == null)
		{
			return;
		}
		MainGame.Instance.craftSystem.RemoveWorker(ZombieCraftActivity);
		if (ZombieCraftActivity != null)
		{
			ZombieCraftActivity.OnActiveStateChanged -= GardenerOnCraftActivityStateChanged;
			curAnimState = AnimationState.Idle;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
			currentActivity = null;
		}
		if (!stopOnly)
		{
			if (GardenerState == ZombieGardenerState.PlantingSeeds)
			{
				GardenerTryExecutePlantOrder();
				GardenerTryMoveToStation();
			}
			else
			{
				GardenerTryGetNewOrderOrMoveToStation();
			}
		}
	}

	private void GardenerOnCraftActivityStateChanged()
	{
		GardenerHandleCraftStatusChange(AttachedWgoData.CraftComponent.Status);
	}

	private void GardenerHandleCraftStatusChange(CraftComponentStatus craftStatus)
	{
		if (craftStatus == CraftComponentStatus.Finished)
		{
			GardenerStopCraftActivity();
		}
		AnimationState animationState = ((ZombieCraftActivity != null && ZombieCraftActivity.IsActive) ? ((craftStatus == CraftComponentStatus.Started) ? ((!AttachedWgoData.Definition.isAutoCrafter) ? CrafterGetAnimationStateForCraft(AttachedWgoData.CraftComponent.CurrentCraftElement.Def) : AnimationState.Idle) : AnimationState.Idle) : AnimationState.Idle);
		if (curAnimState != animationState)
		{
			curAnimState = animationState;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
		}
	}

	private void GardenerOnWorkActivityStateChanged()
	{
		GardenerHandleWorkStatusChange();
	}

	private void GardenerHandleWorkStatusChange()
	{
		if (ZombieHPActivity == null || !ZombieHPActivity.IsActive)
		{
			if (AttachedWgoData != null && AttachedWgoData.HpComponent.isDeathDelayed)
			{
				GardenerState = ZombieGardenerState.WaitingForWgoDeath;
			}
			else if (AttachedWgoData != null && AttachedWgoData.HpComponent.Hp > 0)
			{
				GardenerStartWorkActivity();
			}
			else
			{
				GardenerStopWorkActivity();
			}
		}
		AnimationState animationState = ((ZombieHPActivity != null && ZombieHPActivity.IsActive) ? (ZombieHPActivity.IsActive ? GardenerGetAnimationStateForWork() : AnimationState.Idle) : AnimationState.Idle);
		if (curAnimState != animationState)
		{
			curAnimState = animationState;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
		}
	}

	public void GardenerStartWorkActivity(bool resetTicks = true)
	{
		WgoData wgoData = (attachedWgoData = MainGame.WorldData.GetWgoData(GardenerExecutingOrder.TargetWgoUniqueId));
		attachedWgoDataUniqueId.SetGuid(wgoData.UniqueId);
		ZombieHPActivity hpActivity = (ZombieHPActivity)(currentActivity = new ZombieHPActivity(AttachedWgoData, this));
		ZombieHPActivity.OnActiveStateChanged += GardenerOnWorkActivityStateChanged;
		GardenerOnWorkActivityStateChanged();
		attachedWgoData.TrySetWorker(this);
		MainGame.Instance.craftSystem.AddHPWorker(hpActivity);
		UpdateAttachedWgoViewWidgets();
		if (resetTicks)
		{
			AttachedWgoData.CraftComponent.ZombieSubTicks = 0;
		}
		AttachedWgoData.CraftComponent.TryContinueFromQueue();
		if (GardenBedNavigation.TryGetOpenApproach(AttachedWgoData, base.Position, out var _, out var direction))
		{
			base.direction.Value = direction.ConvertToVector2XZ();
			return;
		}
		DockPointData nearestDockPointData = AttachedWgoData.GetNearestDockPointData(base.Position, DockPointData.Availability.All);
		if (nearestDockPointData != null)
		{
			base.direction.Value = nearestDockPointData.Direction.ConvertToVector2XZ();
		}
	}

	public void GardenerStopWorkActivity(SGuid respawnedEmptyGardenBed = null, bool stopOnly = false)
	{
		if (ZombieHPActivity == null)
		{
			return;
		}
		MainGame.Instance.craftSystem.RemoveHPWorker(ZombieHPActivity);
		if (ZombieHPActivity != null)
		{
			ZombieHPActivity.OnActiveStateChanged -= GardenerOnWorkActivityStateChanged;
			curAnimState = AnimationState.Idle;
			this.OnAnimationStateChanged?.Invoke(curAnimState, arg2: false);
			currentActivity = null;
		}
		if (stopOnly)
		{
			return;
		}
		if (GardenerState == ZombieGardenerState.WaitingForWgoDeath)
		{
			GardenerTryExecuteGatherOrder();
			if (respawnedEmptyGardenBed != null)
			{
				GardenerTryGetNewOrderFromPrevTargetOrMoveToStation(typeof(PlantOrder), respawnedEmptyGardenBed);
			}
			else
			{
				GardenerTryGetNewOrderOrMoveToStation();
			}
		}
		else
		{
			GardenerTryGetNewOrderOrMoveToStation();
		}
	}

	private AnimationState GardenerGetAnimationStateForWork()
	{
		return (AnimationState)(AttachedWgoData.Definition.toolAction.actionableTool + 19);
	}

	private void GardenerTryExecutePlantOrder()
	{
		if (HasGardenerExecutingOrder)
		{
			if (GardenerExecutingOrder.TryExecuteOrder(new ZombieGardenerOrderExecutor(this), out var _))
			{
				OrderBase orderBase = GardenerExecutingOrder;
				GardenerTryStopOrderExecution();
				base.WorldZoneData.RemoveOrder(orderBase.UniqueId);
				if (AttachedWgoData != null)
				{
					AttachedWgoData.ClearWorker();
				}
				attachedWgoData = MainGame.Instance.GameSave.worldData.GetWgoData(gardenerStation);
				attachedWgoDataUniqueId.SetGuid(attachedWgoData.UniqueId);
			}
			else
			{
				GardenerTryStopOrderExecution();
				GardenerState = ZombieGardenerState.GoToStation;
			}
		}
		else
		{
			GardenerTryStopOrderExecution();
			GardenerState = ZombieGardenerState.GoToStation;
		}
	}

	private void GardenerTryExecuteGatherOrder()
	{
		if (HasGardenerExecutingOrder)
		{
			if (GardenerExecutingOrder.TryExecuteOrder(new ZombieGardenerOrderExecutor(this), out var _))
			{
				OrderBase orderBase = GardenerExecutingOrder;
				GardenerTryStopOrderExecution();
				base.WorldZoneData.RemoveOrder(orderBase.UniqueId);
				if (AttachedWgoData != null)
				{
					AttachedWgoData.ClearWorker();
				}
				attachedWgoData = null;
				attachedWgoDataUniqueId.SetGuid(gardenerStation);
			}
			else
			{
				GardenerTryStopOrderExecution();
				GardenerState = ZombieGardenerState.GoToStation;
			}
		}
		else
		{
			GardenerTryStopOrderExecution();
			GardenerState = ZombieGardenerState.GoToStation;
		}
	}

	private void GardenerTryMoveToStation()
	{
		GardenerState = ZombieGardenerState.GoToStation;
		if (AttachedWgoData == null || !AttachedWgoData.UniqueId.Equals(gardenerStation))
		{
			attachedWgoData = null;
			attachedWgoDataUniqueId.SetGuid(gardenerStation);
		}
		gardenerCurrentTargetUniqueId = gardenerStation;
		GardenerTryMoveToCurrentTarget();
	}

	private void GardenerTryMoveToCurrentTarget()
	{
		if (gardenerCurrentMovementTargetUniqueId?.Guid == gardenerCurrentTargetUniqueId?.Guid)
		{
			return;
		}
		if (GardenerState == ZombieGardenerState.FailedToFindPath)
		{
			if (gardenerTimeForCheckFailedPathAgain <= 0f)
			{
				GardenerState = gardenerPreviousState;
				gardenerTimeForCheckFailedPathAgain = 0f;
				MoveAction();
			}
		}
		else
		{
			MoveAction();
		}
		void MoveAction()
		{
			if (base.MovementComponent.IsMoving)
			{
				base.MovementComponent.ForceStop();
			}
			if (GardenerState == ZombieGardenerState.OnStation)
			{
				base.Position = AttachedWgoData.GetNearestDockPointDataWorldPositionOrMyPosition(base.Position, DockPointData.Availability.All, (DockPointData dockPointData, Vector3 parentPos) => !dockPointData.BakedData.DisableTargetingForCaretaker);
				base.direction.Value = AttachedWgoData.GetNearestDockPointData(base.Position, DockPointData.Availability.All)?.Direction.ConvertToVector2XZ() ?? Direction.Down.ConvertToVector2XZ();
			}
			Vector3 endPos;
			if (GardenBedNavigation.IsGardenPlot(GardenerCurrentTarget) && GardenBedNavigation.TryGetOpenApproach(GardenerCurrentTarget, base.Position, out var vector, out var _))
			{
				endPos = vector;
			}
			else
			{
				if (GardenBedNavigation.IsGardenPlot(GardenerCurrentTarget))
				{
					gardenerTimeForCheckFailedPathAgain = 1f;
					GardenerState = ZombieGardenerState.FailedToFindPath;
					return;
				}
				endPos = GardenerCurrentTarget.GetNearestDockPointDataWorldPositionOrMyPosition(base.Position, DockPointData.Availability.All, (DockPointData dockPointData, Vector3 parentPos) => !dockPointData.BakedData.DisableTargetingForCaretaker);
			}
			switch (base.MovementComponent.StartPath(endPos, base.WorldId, base.WorldId, MovementType.GDGraph))
			{
			case MovementComponent.StartPathResult.Started:
				gardenerCurrentMovementTargetUniqueId = gardenerCurrentTargetUniqueId;
				break;
			case MovementComponent.StartPathResult.AlreadyAtDestinationPoint:
				GardenerOnPathSuccess();
				break;
			case MovementComponent.StartPathResult.IncorrectMovementType:
				gardenerTimeForCheckFailedPathAgain = 1f;
				GardenerState = ZombieGardenerState.FailedToFindPath;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void GardenerTryPutGardenItemsToMultiInventory()
	{
		List<Item> list = new List<Item>();
		list.AddRange(WorkerInventory.GetItemsByGroupId("seed"));
		list.AddRange(WorkerInventory.GetItemsByGroupId("seedable"));
		list.AddRange(WorkerInventory.GetItemsByGroupId("crop"));
		MultiInventory multiInventory = new MultiInventory(base.WorldZoneData);
		bool flag = true;
		foreach (Item item in list)
		{
			int depositableCount = GetDepositableCount(multiInventory.inventoryList, item);
			if (depositableCount < item.Count)
			{
				flag = false;
			}
			if (depositableCount <= 0)
			{
				continue;
			}
			foreach (Item item2 in WorkerInventory.RemoveItemById(item.id, depositableCount))
			{
				TryAddItemPreferringSameItem(multiInventory.inventoryList, item2);
				if (item2.Count > 0)
				{
					WorkerInventory.AddItemToInventory(item2);
					flag = false;
				}
			}
		}
		GardenerState = ((!flag) ? ZombieGardenerState.CanNotPutItemToInventory : ZombieGardenerState.OnStation);
	}

	private int GetDepositableCount(List<Inventory> inventories, Item item)
	{
		int num = item.Count;
		int num2 = 0;
		foreach (Inventory inventory in inventories)
		{
			if (num <= 0)
			{
				break;
			}
			int num3 = inventory.Data.CanAddItemCountToInventory(item.Definition, num);
			if (num3 > 0)
			{
				num2 += num3;
				num -= num3;
			}
		}
		return num2;
	}

	private bool GardenerIsAnyMultiInventoryWithSpaceForPortableItemExists()
	{
		foreach (WgoData multiInventoryWgoData in base.WorldZoneData.MultiInventoryWgoDatas)
		{
			if (multiInventoryWgoData.Inventory.Data.CanAddItemToInventory(CaretakerPortableItem))
			{
				return true;
			}
		}
		return false;
	}

	public void GardenerOnToolChanged()
	{
		if (Hand.IsEmpty || Hand.Definition.type != ItemType.Shovel)
		{
			GardenerAbortWorkBecauseNoShovel();
		}
		UpdateAttachedWgoViewWidgets();
	}

	private void GardenerAbortWorkBecauseNoShovel()
	{
		if (ZombieHPActivity != null)
		{
			GardenerStopWorkActivity(null, stopOnly: true);
		}
		if (ZombieCraftActivity != null)
		{
			GardenerStopCraftActivity(stopOnly: true);
		}
		WgoData wgoData = AttachedWgoData;
		bool flag = wgoData?.UniqueId.Equals(gardenerStation) ?? false;
		if (GardenBedNavigation.IsGardenPlot(wgoData))
		{
			wgoData.ClearWorker();
		}
		GardenerTryStopOrderExecution();
		if (!(GardenerState == ZombieGardenerState.OnStation && flag))
		{
			GardenerTryMoveToStation();
		}
	}

	private void ConveyorTransporterUpdateBehaviour(float deltaTime)
	{
		switch (ConveyorTransporterState)
		{
		case ZombieConveyorTransporterState.OnStation:
		case ZombieConveyorTransporterState.GoToStation:
			ConveyorTransporterTryGetNewOrder();
			break;
		case ZombieConveyorTransporterState.FailedToFindPath:
			conveyorTransporterTimeForCheckFailedPathAgain -= deltaTime;
			ConveyorTransporterTryMoveToCurrentTarget();
			break;
		case ZombieConveyorTransporterState.CanNotPutItemToInventory:
			if (conveyorTransporterPreviousState == ZombieConveyorTransporterState.GoToStorageToPutItem)
			{
				ConveyorTransporterState = conveyorTransporterPreviousState;
				ConveyorTransporterTryPutPortableItemToInventory();
			}
			break;
		case ZombieConveyorTransporterState.GoToStationCellToPickUp:
		case ZombieConveyorTransporterState.GoToStorageToPutItem:
			break;
		}
	}

	private void ConveyorTransporterOnPathSuccess()
	{
		curAnimState = AnimationState.Idle;
		switch (ConveyorTransporterState)
		{
		case ZombieConveyorTransporterState.GoToStation:
		{
			ConveyorTransporterState = ZombieConveyorTransporterState.OnStation;
			GDPointData gDPointData = AttachedWgoData.GetGDPointData("zombie_porter_station_gd_point");
			base.Position = gDPointData.Position;
			direction.Value = gDPointData.Direction.ConvertToVector2XZ();
			break;
		}
		case ZombieConveyorTransporterState.GoToStationCellToPickUp:
			ConveyorTransporterTryExecutePickUpOrder();
			break;
		case ZombieConveyorTransporterState.GoToStorageToPutItem:
			ConveyorTransporterTryPutPortableItemToInventory();
			break;
		}
	}

	private void ConveyorTransporterTryGetNewOrder()
	{
		if (HasConveyorTransporterExecutingOrder)
		{
			return;
		}
		OrderBase orderForConveyorTransporter = base.WorldZoneData.GetOrderForConveyorTransporter();
		if (orderForConveyorTransporter == null)
		{
			if (ConveyorTransporterState != 0 && ConveyorTransporterState != ZombieConveyorTransporterState.GoToStation)
			{
				ConveyorTransporterTryMoveToStation();
			}
			return;
		}
		conveyorTransporterExecutingOrder = orderForConveyorTransporter.UniqueId;
		orderForConveyorTransporter.ExecutorUniqueId = base.UniqueId;
		ConveyorTransporterState = ZombieConveyorTransporterState.GoToStationCellToPickUp;
		conveyorTransporterCurrentTargetUniqueId = orderForConveyorTransporter.TargetWgoUniqueId;
		if (base.MovementComponent.IsMoving)
		{
			base.MovementComponent.ForceStop();
		}
		conveyorTransporterCurrentMovementTargetUniqueId = SGuid.Empty;
		ConveyorTransporterTryMoveToCurrentTarget();
	}

	private void ConveyorTransporterTryStopOrderExecution()
	{
		if (!conveyorTransporterExecutingOrder.IsEmpty)
		{
			if (ConveyorTransporterExecutingOrder != null)
			{
				ConveyorTransporterExecutingOrder.ExecutorUniqueId = SGuid.Empty;
			}
			conveyorTransporterExecutingOrder = SGuid.Empty;
		}
	}

	private void ConveyorTransporterTryExecutePickUpOrder()
	{
		string reasonIfNot;
		if (!HasConveyorTransporterExecutingOrder)
		{
			ConveyorTransporterTryMoveToStation();
		}
		else if (ConveyorTransporterExecutingOrder.TryExecuteOrder(new ZombieConveyorTransporterOrderExecutor(this), out reasonIfNot))
		{
			OrderBase orderBase = ConveyorTransporterExecutingOrder;
			ConveyorTransporterTryStopOrderExecution();
			base.WorldZoneData.RemoveOrder(orderBase.UniqueId);
			ConveyorTransporterState = ZombieConveyorTransporterState.GoToStorageToPutItem;
			ConveyorTransporterTryMoveToNearestStorageForPortableItem();
		}
		else
		{
			ConveyorTransporterTryStopOrderExecution();
			ConveyorTransporterTryMoveToStation();
		}
	}

	private void ConveyorTransporterTryPutPortableItemToInventory()
	{
		if (ConveyorTransporterCurrentTarget == null)
		{
			if (ConveyorTransporterGetNearestStorageWithSpace() != null)
			{
				ConveyorTransporterState = ZombieConveyorTransporterState.GoToStorageToPutItem;
				ConveyorTransporterTryMoveToNearestStorageForPortableItem();
			}
			else
			{
				ConveyorTransporterState = ZombieConveyorTransporterState.CanNotPutItemToInventory;
			}
			return;
		}
		ConveyorTransporterCurrentTarget.Inventory.AddItemToInventory(ConveyorTransporterPortableItem);
		if (ConveyorTransporterPortableItem.Count > 0)
		{
			if (ConveyorTransporterGetNearestStorageWithSpace() != null)
			{
				ConveyorTransporterState = ZombieConveyorTransporterState.GoToStorageToPutItem;
				ConveyorTransporterTryMoveToNearestStorageForPortableItem();
			}
			else
			{
				ConveyorTransporterState = ZombieConveyorTransporterState.CanNotPutItemToInventory;
			}
		}
		else
		{
			ConveyorTransporterPortableItem = Item.Empty;
			ConveyorTransporterTryMoveToStation();
		}
	}

	private WgoData ConveyorTransporterGetNearestStorageWithSpace()
	{
		float num = float.MaxValue;
		WgoData result = null;
		WorldZoneData worldZoneDataById = MainGame.WorldData.GetWorldZoneDataById("conveyor_storage");
		if (worldZoneDataById == null)
		{
			return null;
		}
		foreach (WgoData multiInventoryWgoData in worldZoneDataById.MultiInventoryWgoDatas)
		{
			if (multiInventoryWgoData.Inventory.CanAddItemToInventory(ConveyorTransporterPortableItem))
			{
				float num2 = Mathf.Abs(Vector3.Distance(multiInventoryWgoData.Position, base.Position));
				if (num2 < num)
				{
					num = num2;
					result = multiInventoryWgoData;
				}
			}
		}
		return result;
	}

	private void ConveyorTransporterTryMoveToNearestStorageForPortableItem()
	{
		WgoData wgoData = ConveyorTransporterGetNearestStorageWithSpace();
		if (wgoData == null)
		{
			conveyorTransporterCurrentTargetUniqueId = SGuid.Empty;
			ConveyorTransporterState = ZombieConveyorTransporterState.CanNotPutItemToInventory;
		}
		else
		{
			conveyorTransporterCurrentTargetUniqueId = wgoData.UniqueId;
			ConveyorTransporterTryMoveToCurrentTarget();
		}
	}

	private void ConveyorTransporterTryMoveToStation()
	{
		ConveyorTransporterState = ZombieConveyorTransporterState.GoToStation;
		conveyorTransporterCurrentTargetUniqueId = AttachedWgoData.UniqueId;
		ConveyorTransporterTryMoveToCurrentTarget();
	}

	private void ConveyorTransporterTryMoveToCurrentTarget()
	{
		if (conveyorTransporterCurrentMovementTargetUniqueId?.Guid == conveyorTransporterCurrentTargetUniqueId?.Guid)
		{
			return;
		}
		if (ConveyorTransporterState == ZombieConveyorTransporterState.FailedToFindPath)
		{
			if (conveyorTransporterTimeForCheckFailedPathAgain <= 0f)
			{
				ConveyorTransporterState = conveyorTransporterPreviousState;
				conveyorTransporterTimeForCheckFailedPathAgain = 0f;
				MoveAction();
			}
		}
		else
		{
			MoveAction();
		}
		void MoveAction()
		{
			DockPointData nearestDockPointData = ConveyorTransporterCurrentTarget.GetNearestDockPointData(base.Position, DockPointData.Availability.All);
			if (base.MovementComponent.IsMoving)
			{
				base.MovementComponent.ForceStop();
			}
			if (ConveyorTransporterState == ZombieConveyorTransporterState.OnStation)
			{
				base.Position = AttachedWgoData.GetNearestDockPointDataWorldPositionOrMyPosition(base.Position, DockPointData.Availability.OnlyOccupied, (DockPointData dockPointData, Vector3 parentPos) => !dockPointData.BakedData.DisableTargetingForCaretaker);
				direction.Value = AttachedWgoData.GetNearestDockPointData(base.Position, DockPointData.Availability.All)?.Direction.ConvertToVector2XZ() ?? Direction.Down.ConvertToVector2XZ();
			}
			Vector3 nearestDockPointDataWorldPositionOrMyPosition = ConveyorTransporterCurrentTarget.GetNearestDockPointDataWorldPositionOrMyPosition(base.Position, DockPointData.Availability.All, (DockPointData dockPointData, Vector3 parentPos) => !dockPointData.BakedData.DisableTargetingForCaretaker);
			GraphMask graphMask = NavigationGraphMaskUtils.ToGraphMask(base.WorldZoneData?.MovementGraphs, base.WorldZoneData?.navigationGraph ?? LazyConsts.Navigation.Graph.None);
			switch ((nearestDockPointData != null) ? base.MovementComponent.StartPath(nearestDockPointDataWorldPositionOrMyPosition, nearestDockPointData.BakedData, graphMask, base.WorldId) : base.MovementComponent.StartPath(nearestDockPointDataWorldPositionOrMyPosition, graphMask, base.WorldId))
			{
			case MovementComponent.StartPathResult.Started:
				conveyorTransporterCurrentMovementTargetUniqueId = conveyorTransporterCurrentTargetUniqueId;
				break;
			case MovementComponent.StartPathResult.AlreadyAtDestinationPoint:
				ConveyorTransporterOnPathSuccess();
				break;
			case MovementComponent.StartPathResult.IncorrectMovementType:
				conveyorTransporterTimeForCheckFailedPathAgain = 1f;
				ConveyorTransporterState = ZombieConveyorTransporterState.FailedToFindPath;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	private void ConveyorTransporterOnOrderRemoved(OrderBase order)
	{
		if (!(order.UniqueId != conveyorTransporterExecutingOrder))
		{
			if (ConveyorTransporterState == ZombieConveyorTransporterState.FailedToFindPath)
			{
				ConveyorTransporterState = conveyorTransporterPreviousState;
				conveyorTransporterTimeForCheckFailedPathAgain = 0f;
			}
			if (ConveyorTransporterState == ZombieConveyorTransporterState.CanNotPutItemToInventory)
			{
				ConveyorTransporterState = conveyorTransporterPreviousState;
			}
			if (ConveyorTransporterState == ZombieConveyorTransporterState.GoToStationCellToPickUp)
			{
				ConveyorTransporterTryStopOrderExecution();
				ConveyorTransporterTryMoveToStation();
			}
		}
	}

	public void FighterOnEquipmentChange()
	{
		this.OnEquipmentChanged?.Invoke(new Inventory(zombieItem));
	}
}
