using System;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;

[Serializable]
public class WgoData : ObjectLinkedToDefinition<WGODef>, IEquatable<WgoData>, IMovable, ICraftable
{
	public delegate void DelOnToolTickApply(bool isFirstHit);

	private static Vector3 NPC_BUBBLE_Y_OFFSET = Vector3.up * 1.5f;

	private Action customDeathCallback;

	[SerializeField]
	private SGuid uniqueId = new SGuid();

	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Vector3 scale = Vector3.one;

	[SerializeField]
	public VariableNotificator<Vector2> direction = new VariableNotificator<Vector2>();

	[SerializeField]
	private string worldId;

	[SerializeField]
	private string customTag;

	[SerializeField]
	private Inventory inventory = new Inventory();

	[SerializeField]
	protected Inventory craftInventory = new Inventory();

	[SerializeField]
	protected GameRes gameRes = new GameRes();

	[SerializeField]
	private GameResStr gameResStr = new GameResStr();

	[SerializeField]
	private string worldZoneDataId;

	[SerializeField]
	private List<InteractionEvent> events = new List<InteractionEvent>();

	[SerializeField]
	private List<DelayedEvent> delayedEvents = new List<DelayedEvent>();

	[SerializeField]
	private bool isInteractable = true;

	[SerializeField]
	private bool isHidden;

	[SerializeField]
	private string customAnimationTrigger;

	[SerializeField]
	private WgoPartData mainWgoPartData;

	[SerializeField]
	private List<WgoPartData> additionalWgoPartsData = new List<WgoPartData>();

	[SerializeField]
	protected List<PerkData> activePerks = new List<PerkData>();

	[SerializeField]
	private List<SGuid> workbenchParents = new List<SGuid>();

	[SerializeField]
	private List<SGuid> attachedWorkbenchExtensions = new List<SGuid>();

	[SerializeField]
	private SGuid workerId = SGuid.Empty;

	[SerializeField]
	private MovementComponent movementComponent = new MovementComponent();

	[SerializeField]
	private CraftComponent craftComponent = new CraftComponent();

	[SerializeField]
	private HPComponent hpComponent = new HPComponent();

	[SerializeField]
	private SpawnWgoComponent spawnWGOComponent = new SpawnWgoComponent();

	[SerializeField]
	private TownBuildingWgoComponent townBuildingWgoComponent = new TownBuildingWgoComponent();

	[SerializeField]
	private TownClusterRepairWgoComponent townClusterRepairWgoComponent = new TownClusterRepairWgoComponent();

	[SerializeField]
	private SGuid linkedToTownBuildingUniqueId = SGuid.Empty;

	[SerializeField]
	private SGuid linkedFromTownBuildingUniqueId = SGuid.Empty;

	[SerializeField]
	private bool customDeathWasTriggered;

	[HideInInspector]
	public StartReses startReses;

	[NonSerialized]
	private ChunkBoundsPair serializedBounds;

	[NonSerialized]
	private bool hasSerializedBounds;

	public SGuid takenDockPointsParentSGuid;

	public string occupiedPointOfInterest;

	public List<WgoCustomComponentData> customComponentsData = new List<WgoCustomComponentData>();

	[NonSerialized]
	public bool isTempObject;

	private bool isInitialized;

	public bool wasSpawnedAtLeastOnce;

	public bool isRemovingFromData;

	public bool gdPointsRegistered;

	public List<GDPointData> gdPointsData = new List<GDPointData>();

	[NonSerialized]
	public bool wasCustomAnimationFired;

	private Vector3 bubblePosOffset = NPC_BUBBLE_Y_OFFSET;

	private WorldZoneData worldZoneData;

	private bool isWorkerCutUnitAdded;

	private bool isCustomNavMeshCutTracked;

	private bool hasCustomNavMeshCutUnit;

	public GameResStr GameResStr => gameResStr;

	public Vector3 BubblePos => position + bubblePosOffset;

	public float Quality => GetQuality();

	public string WorldId
	{
		get
		{
			return worldId;
		}
		set
		{
			worldId = value;
		}
	}

	public WorldZoneData WorldZoneData
	{
		get
		{
			return worldZoneData ?? (worldZoneData = MainGame.Instance.GameSave.WorldData.GetGameSceneDataById(WorldId)?.GetWorldZoneDataById(worldZoneDataId));
		}
		set
		{
			worldZoneDataId = ((value == null) ? string.Empty : value.id);
			worldZoneData = value;
		}
	}

	public bool HasSerializedBounds => hasSerializedBounds;

	public ChunkBoundsPair SerializedBounds => serializedBounds;

	public WgoPartData MainWgoPartData => mainWgoPartData;

	public List<WgoPartData> AdditionalWgoPartsData => additionalWgoPartsData;

	public Inventory Inventory
	{
		get
		{
			if (base.Definition.hasRefToOtherWgoInventory)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(base.Definition.refToOtherWgoInventory);
				if (wgoData != null)
				{
					return wgoData.inventory;
				}
			}
			return inventory;
		}
	}

	public Inventory CraftInventory
	{
		get
		{
			if (base.Definition.hasRefToOtherWgoInventory)
			{
				WgoData wgoData = MainGame.WorldData.GetWgoData(base.Definition.refToOtherWgoInventory);
				if (wgoData != null)
				{
					return wgoData.CraftInventory;
				}
			}
			return craftInventory;
		}
	}

	public SGuid UniqueId => uniqueId;

	public string CustomTag
	{
		get
		{
			return customTag;
		}
		set
		{
			customTag = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return isHidden;
		}
		set
		{
			isHidden = value;
			this.OnHiddenStateChanged?.Invoke(isHidden);
		}
	}

	public List<InteractionEvent> Events => events;

	public MovementComponent MovementComponent => movementComponent;

	public HPComponent HpComponent
	{
		get
		{
			return hpComponent;
		}
		set
		{
			hpComponent = value;
		}
	}

	public CraftComponent CraftComponent => craftComponent;

	public SpawnWgoComponent SpawnWGOComponent => spawnWGOComponent;

	public TownBuildingWgoComponent TownBuildingWgoComponent
	{
		get
		{
			return townBuildingWgoComponent;
		}
		set
		{
			townBuildingWgoComponent = value;
		}
	}

	public TownClusterRepairWgoComponent TownClusterRepairWgoComponent
	{
		get
		{
			return townClusterRepairWgoComponent;
		}
		set
		{
			townClusterRepairWgoComponent = value;
		}
	}

	public SGuid LinkedToTownBuildingUniqueId
	{
		get
		{
			return linkedToTownBuildingUniqueId;
		}
		set
		{
			linkedToTownBuildingUniqueId = value;
		}
	}

	public SGuid LinkedFromTownBuildingUniqueId
	{
		get
		{
			return linkedFromTownBuildingUniqueId;
		}
		set
		{
			linkedFromTownBuildingUniqueId = value;
		}
	}

	public Vector3 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
			this.OnPositionChanged?.Invoke(value);
		}
	}

	public Vector3 Scale
	{
		get
		{
			if (!(scale == Vector3.zero))
			{
				return scale;
			}
			return Vector3.one;
		}
		set
		{
			scale = value;
		}
	}

	public List<PerkData> ActivePerks => activePerks;

	public bool IsInteractable
	{
		get
		{
			return isInteractable;
		}
		set
		{
			bool num = isInteractable;
			isInteractable = value;
			if (num != isInteractable)
			{
				this.OnInteractableStateChanged?.Invoke(isInteractable);
			}
		}
	}

	public Vector3 MovablePosition
	{
		get
		{
			return Position;
		}
		set
		{
			Position = value;
		}
	}

	public Vector3 MovablePositionWithoutDirectionChange
	{
		get
		{
			return Position;
		}
		set
		{
			Position = value;
		}
	}

	public Vector2 MovableDirection
	{
		get
		{
			return direction.Value;
		}
		set
		{
			direction.Value = value;
		}
	}

	public Direction Direction => MovableDirection.ConvertFromVector2();

	public string MovableObjectId => id;

	public string CraftableObjectId => id;

	public virtual Inventory CraftableObjectCraftInventory => CraftInventory;

	public Inventory CraftableObjectInventory => Inventory;

	public IWorker CraftableAttachedWorker => Worker;

	public float AutoCraftTickDuration
	{
		get
		{
			if (base.Definition != null)
			{
				return base.Definition.autocraftTickDuration.EvaluateFloat(this);
			}
			return 0f;
		}
	}

	public CraftableType CraftableType
	{
		get
		{
			if (base.Definition == null)
			{
				return CraftableType.Regular;
			}
			if (base.Definition.conveyorType != ConveyorElementType.Workbench)
			{
				return CraftableType.Regular;
			}
			return CraftableType.ConveyorWorkbench;
		}
	}

	public IWorker Worker => IWorker.FromId(workerId);

	public IReadOnlyList<SGuid> AttachedWorkbenchExtensions => attachedWorkbenchExtensions;

	public IReadOnlyList<SGuid> WorkbenchParents => workbenchParents;

	public Dictionary<string, List<CraftDefBase>> WorkbenchExtensionsCrafts
	{
		get
		{
			Dictionary<string, List<CraftDefBase>> dictionary = new Dictionary<string, List<CraftDefBase>>();
			foreach (SGuid attachedWorkbenchExtension in AttachedWorkbenchExtensions)
			{
				WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(attachedWorkbenchExtension);
				if (wgoData == null)
				{
					continue;
				}
				List<CraftDef> workbenchExtensionCrafts = GameBalance.Me.GetWorkbenchExtensionCrafts(id, wgoData.id);
				List<CraftDefBase> list = new List<CraftDefBase>();
				KnowledgeSystem knowledgeSystem = MainGame.Instance.GameSave.knowledgeSystem;
				foreach (CraftDef item in workbenchExtensionCrafts)
				{
					if (!knowledgeSystem.IsOneTimeCraftCompleted(item))
					{
						list.Add(item);
					}
				}
				dictionary.TryAdd(wgoData.id, list);
			}
			return dictionary;
		}
	}

	public event DelOnToolTickApply OnToolTickApply;

	public event Action OnOccuredDeath;

	public event Action<WgoPartData> OnAdditionalWgoPartAdd;

	public event Action<WgoPartData> OnAdditionalWgoPartRemove;

	public event Action<string> OnGameResChanged;

	public event Action<Vector3> OnPositionChanged;

	public event Action<Vector2> OnDirectionChanged;

	public event Action OnInteractionEventChanged;

	public static event Action<WgoData> OnAnyInteractionEventChanged;

	public event Action<bool> OnInteractableStateChanged;

	public event Action OnWorkerChanged;

	public event Action<string> OnAnimationTriggerSet;

	public event Action<bool> OnHiddenStateChanged;

	public event Action<AnimationState> OnAnimationStateSet;

	public event Action<int, float> OnAnimationLayerSet;

	public event Action OnTakenDockPointChanged;

	public event Action OnRemoveFromData;

	public void SetSerializedBounds(ChunkBoundsPair bounds)
	{
		serializedBounds = bounds;
		hasSerializedBounds = true;
	}

	public void ClearSerializedBounds()
	{
		hasSerializedBounds = false;
	}

	public WgoData()
	{
	}

	public WgoData(string id, Vector3 position)
		: this(id, position, "")
	{
	}

	public WgoData(string id, Vector3 position, string worldId)
		: this(id, position, worldId, "")
	{
	}

	public WgoData(string id, Vector3 position, string worldId, string customTag)
		: base(id)
	{
		uniqueId = new SGuid();
		Position = position;
		this.worldId = worldId;
		this.customTag = customTag;
		direction.Value = Vector2.zero;
		SetDataFromDefinition();
		TryCreateMainWgoPartData();
		SetupForcedNavigationHoleIfNeeded();
		PrepareForGame();
	}

	public WgoData(WgoData other)
		: this(other, new SGuid())
	{
	}

	public WgoData(WgoData other, SGuid sGuid)
	{
		id = other.id;
		uniqueId = sGuid;
		Position = other.Position;
		Scale = other.Scale;
		direction = other.direction;
		worldId = other.worldId;
		CustomTag = other.CustomTag;
		worldZoneDataId = other.worldZoneDataId;
		inventory = other.inventory;
		craftInventory = other.craftInventory;
		gameRes = other.gameRes;
		gameResStr = other.GameResStr;
		events = other.events;
		mainWgoPartData = new WgoPartData(other.mainWgoPartData);
		additionalWgoPartsData.AddRange(new List<WgoPartData>(other.additionalWgoPartsData.Select((WgoPartData partData) => new WgoPartData(partData)).ToList()));
		gdPointsRegistered = other.gdPointsRegistered;
		movementComponent = other.movementComponent;
		craftComponent = other.craftComponent;
		hpComponent = other.hpComponent;
		spawnWGOComponent = other.spawnWGOComponent;
		townBuildingWgoComponent = other.townBuildingWgoComponent;
		townClusterRepairWgoComponent = other.townClusterRepairWgoComponent;
		gdPointsData = other.gdPointsData;
		activePerks = other.activePerks;
		customComponentsData = other.customComponentsData;
		isHidden = other.isHidden;
		customComponentsData = new List<WgoCustomComponentData>(other.customComponentsData);
		SetDataFromDefinition();
		PrepareForGame();
	}

	public WgoData CreateDataFromMe(Vector3 globalOffset, string gameSceneId, bool copySGuid)
	{
		WgoData wgoData = new WgoData(this, copySGuid ? UniqueId : new SGuid());
		wgoData.Position += globalOffset;
		wgoData.WorldId = gameSceneId;
		return wgoData;
	}

	public void TryCreateMainWgoPartData()
	{
		if (mainWgoPartData == null)
		{
			string text = base.Definition.ResolveAssetId(base.Definition.id, this);
			mainWgoPartData = new WgoPartData(text);
		}
	}

	public void ReCreateMainWgoPartData(int rotationIndex = -1)
	{
		mainWgoPartData = null;
		TryCreateMainWgoPartData();
		if (mainWgoPartData != null)
		{
			if (rotationIndex > -1)
			{
				mainWgoPartData.rotationIndex = rotationIndex;
			}
			mainWgoPartData.PrepareForGame();
		}
	}

	public void BeginCustomNavMeshCutTracking()
	{
		if (Application.isPlaying && !(LazySingleton<GlobalNavigationManager>.Instance == null))
		{
			EnsureCustomNavMeshCutTracking();
			SyncCustomNavMeshCutUnit();
			if (!hasCustomNavMeshCutUnit && !GraphCustomNavMeshCutUnit.HasBakedCustomNavMeshCuts(this))
			{
				Debug.Log($"[GraphCustomNavMeshCutUnit] WGO [{id}] part [{mainWgoPartData.id}] hash [{mainWgoPartData.GetStateHash()}]: " + "no baked custom nav mesh cut prefabs");
			}
		}
	}

	public void RefreshCustomNavMeshCutUnit()
	{
		if (!isCustomNavMeshCutTracked)
		{
			if (GraphCustomNavMeshCutUnit.HasBakedCustomNavMeshCuts(this))
			{
				BeginCustomNavMeshCutTracking();
			}
		}
		else
		{
			SyncCustomNavMeshCutUnit();
		}
	}

	public void StopCustomNavMeshCutTracking()
	{
		if (!isCustomNavMeshCutTracked)
		{
			return;
		}
		isCustomNavMeshCutTracked = false;
		OnPositionChanged -= HandleCustomNavMeshCutPositionChanged;
		mainWgoPartData.OnStateChange -= HandleCustomNavMeshCutPartStateChanged;
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			additionalWgoPartsDatum.OnStateChange -= HandleCustomNavMeshCutPartStateChanged;
		}
		OnAdditionalWgoPartAdd -= HandleCustomNavMeshCutAdditionalPartAdd;
		OnAdditionalWgoPartRemove -= HandleCustomNavMeshCutAdditionalPartRemove;
		if (hasCustomNavMeshCutUnit && LazySingleton<GlobalNavigationManager>.Instance != null)
		{
			LazySingleton<GlobalNavigationManager>.Instance.RemoveCustomNavMeshCutUnit(UniqueId);
		}
		hasCustomNavMeshCutUnit = false;
	}

	private void EnsureCustomNavMeshCutTracking()
	{
		if (isCustomNavMeshCutTracked)
		{
			return;
		}
		isCustomNavMeshCutTracked = true;
		OnPositionChanged += HandleCustomNavMeshCutPositionChanged;
		mainWgoPartData.OnStateChange += HandleCustomNavMeshCutPartStateChanged;
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			additionalWgoPartsDatum.OnStateChange += HandleCustomNavMeshCutPartStateChanged;
		}
		OnAdditionalWgoPartAdd += HandleCustomNavMeshCutAdditionalPartAdd;
		OnAdditionalWgoPartRemove += HandleCustomNavMeshCutAdditionalPartRemove;
	}

	private void SyncCustomNavMeshCutUnit()
	{
		if (Application.isPlaying && isCustomNavMeshCutTracked && !(LazySingleton<GlobalNavigationManager>.Instance == null))
		{
			if (GraphCustomNavMeshCutUnit.HasBakedCustomNavMeshCuts(this))
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddCustomNavMeshCutUnit(UniqueId, Position, Scale, this);
				hasCustomNavMeshCutUnit = true;
			}
			else if (hasCustomNavMeshCutUnit)
			{
				LazySingleton<GlobalNavigationManager>.Instance.RemoveCustomNavMeshCutUnit(UniqueId);
				hasCustomNavMeshCutUnit = false;
			}
		}
	}

	private void SetupForcedNavigationHoleIfNeeded()
	{
		if (!Application.isPlaying || LazySingleton<GlobalNavigationManager>.Instance == null || mainWgoPartData == null)
		{
			return;
		}
		WGODef wGODef = base.Definition;
		if (wGODef != null && wGODef.forceSetNavigationHoleType == WGODef.ForceSetNavigationHoleType.SpawnHoleForce)
		{
			int stateHash = mainWgoPartData.GetStateHash();
			if (mainWgoPartData.BakedData.TryGetGraphUpdateSceneBoxData(stateHash, out var data))
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddGraphSceneUpdateUnit(uniqueId, Position, data);
			}
			if (mainWgoPartData.BakedData.TryGetPlannerMeshData(stateHash, out var data2))
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddCutUnit(uniqueId, Position, data2);
			}
			else
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddCutUnit(uniqueId, mainWgoPartData.GetCollisionBoundsRect(Position), Position.y);
			}
			BeginCustomNavMeshCutTracking();
		}
	}

	private void HandleCustomNavMeshCutPositionChanged(Vector3 _)
	{
		if (hasCustomNavMeshCutUnit && Application.isPlaying && !(LazySingleton<GlobalNavigationManager>.Instance == null))
		{
			LazySingleton<GlobalNavigationManager>.Instance.UpdateCustomNavMeshCutUnitTransform(UniqueId, Position, Scale);
		}
	}

	private void HandleCustomNavMeshCutPartStateChanged(string _, int __)
	{
		SyncCustomNavMeshCutUnit();
	}

	private void HandleCustomNavMeshCutAdditionalPartAdd(WgoPartData partData)
	{
		partData.OnStateChange += HandleCustomNavMeshCutPartStateChanged;
		SyncCustomNavMeshCutUnit();
	}

	private void HandleCustomNavMeshCutAdditionalPartRemove(WgoPartData partData)
	{
		partData.OnStateChange -= HandleCustomNavMeshCutPartStateChanged;
		SyncCustomNavMeshCutUnit();
	}

	public virtual void PrepareForGame()
	{
		if (isInitialized)
		{
			return;
		}
		mainWgoPartData.PrepareForGame();
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			additionalWgoPartsDatum.PrepareForGame();
		}
		isInitialized = true;
		direction.ValueChanged += HandleDirectionChanged;
		movementComponent.Init(this);
		craftComponent.Init(this);
		if (base.Definition == null)
		{
			Debug.LogError("Definition is null [" + id + "]");
			return;
		}
		if (base.Definition != null && base.Definition.reviveOnDie)
		{
			hpComponent.Init(HandleRevive, HandleDeath);
		}
		else
		{
			hpComponent.Init(HandleDeath);
		}
		hpComponent.OnHpChanged += HandleHpChanged;
		if (base.Definition.interactionType == WGODef.InteractionType.TownPalette)
		{
			TownSystem.OnClearTownPalettes += HandleOnClearTownPalettes;
		}
		if (!string.IsNullOrEmpty(base.Definition?.attachedScript))
		{
			WgoDataScriptsManager.CreateScript(this, base.Definition.attachedScript);
		}
	}

	public void ChangeId(string newId)
	{
		id = newId;
		List<Item> list = inventory.Data.Inventory;
		List<Item> list2 = craftInventory.Data.Inventory;
		SetDataFromDefinition();
		if (!string.IsNullOrEmpty(base.Definition.attachedScript))
		{
			WgoDataScriptsManager.DestroyScript(this);
		}
		if (!string.IsNullOrEmpty(base.Definition.attachedScript))
		{
			WgoDataScriptsManager.CreateScript(this, base.Definition.attachedScript);
		}
		for (int i = 0; i < list.Count; i++)
		{
			inventory.AddItemToInventory(list[i]);
		}
		for (int j = 0; j < list2.Count; j++)
		{
			craftInventory.AddItemToInventory(list2[j]);
		}
		if (base.Definition.reviveOnDie)
		{
			hpComponent.Init(HandleRevive, HandleDeath);
		}
		else
		{
			hpComponent.Init(HandleDeath);
		}
		if (GardenBedNavigation.IsGardenPlot(this))
		{
			GardenBedNavigation.UnlinkApproachPoints(this);
		}
		RewriteGdPointsData(new List<GDPointData>());
		gdPointsRegistered = false;
		wasSpawnedAtLeastOnce = false;
		int rotationIndex = mainWgoPartData.rotationIndex;
		ReCreateMainWgoPartData(rotationIndex);
		craftComponent.ResetCraftsFromBalanceCache();
	}

	public virtual void DeInit()
	{
		direction.ValueChanged -= HandleDirectionChanged;
		hpComponent.OnHpChanged -= HandleHpChanged;
		if (base.Definition != null && base.Definition.interactionType == WGODef.InteractionType.TownPalette)
		{
			TownSystem.OnClearTownPalettes -= HandleOnClearTownPalettes;
		}
		movementComponent.DeInit();
		isInitialized = false;
	}

	public void SetBubblePointOffset(Vector3 position)
	{
		bubblePosOffset = position;
	}

	public bool Equals(WgoData other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return uniqueId == other.uniqueId;
	}

	public void OnRemove(bool clearCraftComponent = true)
	{
		isRemovingFromData = true;
		StopCustomNavMeshCutTracking();
		foreach (SGuid workbenchParent in WorkbenchParents)
		{
			MainGame.Instance.GameSave.worldData.GetWgoData(workbenchParent)?.RemoveWorkbenchExtension(UniqueId);
		}
		if (clearCraftComponent)
		{
			RemoveFullCoverSoftSlotExtensions();
		}
		if (base.Definition.townQuality > 0)
		{
			MainGame.Instance.GameSave.townSystem.Quality -= base.Definition.townQuality;
		}
		if (!string.IsNullOrEmpty(base.Definition.npcLifeSimGroup))
		{
			NPCGroupPointOfInterestData groupById = MainGame.Instance.GameSave.npcLifeSimulatorData.GetGroupById(base.Definition.npcLifeSimGroup);
			if (groupById == null)
			{
				Debug.LogError("Can't remove wgo from npc life sim group:[" + base.Definition.npcLifeSimGroup + "]");
			}
			else
			{
				groupById.RemoveWgoFromGroup(this);
			}
		}
		if (!string.IsNullOrEmpty(occupiedPointOfInterest))
		{
			NPCPointOfInterestData pointById = MainGame.Instance.GameSave.npcLifeSimulatorData.GetPointById(occupiedPointOfInterest);
			occupiedPointOfInterest = string.Empty;
			pointById?.Deoccupy();
			MainGame.Instance.GameSave.npcLifeSimulatorData.TryRemoveAnimationData(UniqueId);
			MainGame.Instance.GameSave.npcLifeSimulatorData.TryRemoveActionData(UniqueId);
		}
		if (GardenBedNavigation.IsGardenPlot(this))
		{
			GardenBedNavigation.UnlinkApproachPoints(this);
		}
		RewriteGdPointsData(new List<GDPointData>());
		if (!string.IsNullOrEmpty(base.Definition.attachedScript))
		{
			WgoDataScriptsManager.DestroyScript(this);
		}
		MainGame.Instance.GameSave.wgoDelayedEventSystemData.wgoUniqueIds.Remove(UniqueId);
		if (clearCraftComponent)
		{
			craftComponent.Clear();
		}
		if (id.StartsWith("garden_") || id.StartsWith("vineyard_"))
		{
			WorldZoneData?.RemoveOrdersByTarget(UniqueId);
		}
		if (GardenBedNavigation.IsGardenPlot(this))
		{
			GardenBedNavigation.RefreshAround(this, this);
		}
		this.OnRemoveFromData?.Invoke();
	}

	public void FireEvent(string eventName)
	{
		WgoDataScriptsManager.FireEvent(this, eventName);
	}

	public void AddDelayedEvent(string eventName, float delayTime)
	{
		delayedEvents.Add(new DelayedEvent(eventName, delayTime));
		Debug.Log("AddDelayedEvent: [" + eventName + "] to WgoData: [" + id + "]");
		if (!MainGame.Instance.GameSave.wgoDelayedEventSystemData.wgoUniqueIds.Contains(UniqueId))
		{
			MainGame.Instance.GameSave.wgoDelayedEventSystemData.wgoUniqueIds.Add(UniqueId);
		}
	}

	public void RemoveDelayedEvent(string eventName)
	{
		delayedEvents.RemoveAll((DelayedEvent de) => de.eventName == eventName);
		if (delayedEvents.Count == 0)
		{
			MainGame.Instance.GameSave.wgoDelayedEventSystemData.wgoUniqueIds.Remove(UniqueId);
		}
	}

	public void UpdateDelayedEvents(float deltaTime)
	{
		for (int i = 0; i < delayedEvents.Count; i++)
		{
			DelayedEvent delayedEvent = delayedEvents[i];
			delayedEvent.delayTime -= deltaTime;
			if (delayedEvent.delayTime <= 0f)
			{
				FireEvent(delayedEvent.eventName);
				delayedEvents.RemoveAt(i);
				if (delayedEvents.Count == 0)
				{
					MainGame.Instance.GameSave.wgoDelayedEventSystemData.wgoUniqueIds.Remove(UniqueId);
				}
				break;
			}
		}
	}

	public void OnTransitionReached(string currentWorldId, string destinationWorldId)
	{
	}

	public virtual void OnPathComplete(MovementComponent component)
	{
		if (component.Completion == MovementComponent.CompletionState.Success && !string.IsNullOrEmpty(component.OnPathCompleteEvent))
		{
			FireEvent(component.OnPathCompleteEvent);
		}
		if (component.TypeDestination == MovementComponent.DestinationType.PointOfInterest)
		{
			MainGame.Instance.npcLifeSimulator.OnWgoReachedPointOfInterest(this);
		}
	}

	public void OnTeleportToTransitPoint(Vector3 from, Vector3 to)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(UniqueId);
		if (!(wgoViewGlobal == null) && wgoViewGlobal.gameObject.activeSelf)
		{
			WorldFX.Spawn(from, "puff_npc");
			WorldFX.Spawn(to, "puff_npc");
		}
	}

	public void OnAddToQueue(CraftElementBase ce)
	{
		if (!(ce is CraftElement craftElement))
		{
			return;
		}
		foreach (LazyExpression onCraftAddQueueExpression in craftElement.Definition.onCraftAddQueueExpressions)
		{
			onCraftAddQueueExpression.EvaluateBool(this);
		}
	}

	public void OnCraftStart(CraftElementBase ce)
	{
		gameRes.Set(ce.Def.setWgoParamsOnStart);
		gameRes.Add(ce.Def.addWgoParamsOnStart);
		SetGameRes("reached_gold", 0);
		SetGameRes("reached_silver", 0);
		SetGameRes("reached_bronze", 0);
		foreach (Item item in OutputItems.MakeOutput(ce.PreToWgoOnStartItems))
		{
			Inventory.AddItemToInventory(item);
		}
		if (ce is CraftElement craftElement)
		{
			foreach (LazyExpression onCraftStartExpression in craftElement.Definition.onCraftStartExpressions)
			{
				onCraftStartExpression.EvaluateBool(this);
			}
			CraftDef craftDef = craftElement.Definition;
			if (craftDef.transferDestinationStart != 0)
			{
				string destinationItemStart = craftDef.destinationItemStart;
				switch (craftDef.transferDestinationStart)
				{
				case TransferDestination.Wgo:
					if (craftDef.transferNeedsToDestinationOnStart)
					{
						Inventory.AddItemsToInventory(craftElement.CraftInput);
					}
					Inventory.AddItemsToInventory(OutputItems.MakeOutput(ce.PreToWgoOnStartItems));
					MakeDrop(Inventory.RemoveItems(craftDef.dropFromWgoItemsStart));
					break;
				case TransferDestination.ItemInside:
					if (craftDef.transferNeedsToDestinationOnStart)
					{
						Inventory.AddItemsToNestedItemById(destinationItemStart, craftElement.CraftInput);
					}
					Inventory.AddItemsToNestedItemById(destinationItemStart, OutputItems.MakeOutput(ce.PreToWgoOnStartItems));
					MakeDrop(Inventory.RemoveItemsFromNestedItemById(destinationItemStart, craftDef.dropFromWgoItemsStart));
					break;
				case TransferDestination.GroupItemInside:
					if (craftDef.transferNeedsToDestinationOnStart)
					{
						Inventory.AddItemsToNestedItemByGroupId(destinationItemStart, craftElement.CraftInput);
					}
					Inventory.AddItemsToNestedItemByGroupId(destinationItemStart, OutputItems.MakeOutput(ce.PreToWgoOnStartItems));
					MakeDrop(Inventory.RemoveItemsFromNestedItemByGroupId(destinationItemStart, craftDef.dropFromWgoItemsStart));
					break;
				}
			}
			if (ce.SucceededProgressTicks > 0)
			{
				OnSuccessfulTicksChange(0, ce.SucceededProgressTicks, ce);
			}
		}
		if (!(ce is CraftElementSermon))
		{
			return;
		}
		foreach (Item item2 in OutputItems.MakeOutput(ce.PreToWgoOnFinishItems))
		{
			Inventory.AddItemToInventory(item2);
		}
	}

	public virtual void OnCraftEnd(CraftElementBase ce)
	{
		WgoDelayedSpawnSystem wgoDelayedSpawnSystem = MainGame.Instance.wgoDelayedSpawnSystem;
		if (base.Definition.wgoGroup == "spawner" && !wgoDelayedSpawnSystem.Contains(this) && !wgoDelayedSpawnSystem.CanSpawn(this))
		{
			wgoDelayedSpawnSystem.Add(this, ce);
			return;
		}
		SetGameRes(ce.Def.setWgoParamsOnFinish);
		AddGameRes(ce.Def.addWgoParamsOnFinish);
		bool flag = ce.PreOutputItems.Find((ItemCount x) => x.Def.quality == 3) != null || ce.PreOutputItems.Find((ItemCount x) => x.Def.quality == 3) != null;
		bool flag2 = ce.PreOutputItems.Find((ItemCount x) => x.Def.quality == 2) != null || ce.PreOutputItems.Find((ItemCount x) => x.Def.quality == 2) != null;
		bool flag3 = ce.PreOutputItems.Find((ItemCount x) => x.Def.quality == 1) != null || ce.PreOutputItems.Find((ItemCount x) => x.Def.quality == 1) != null;
		SetGameRes("reached_gold", flag ? 1 : 0);
		SetGameRes("reached_silver", (flag2 || flag) ? 1 : 0);
		SetGameRes("reached_bronze", (flag3 || flag2 || flag) ? 1 : 0);
		if (ce.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.GardenPlanting)
		{
			SetGameRes("succeded_cells", ce.SucceededProgressTicks);
		}
		if (ce is CraftElementSurvey craftElementSurvey)
		{
			DoTechPointsReward(craftElementSurvey.Definition.techRed, craftElementSurvey.Definition.techGreen, craftElementSurvey.Definition.techBlue);
			EvaluateOnCraftEndExpressions(craftElementSurvey.Definition.onCraftEndExpressions);
			if (craftElementSurvey.Definition.isScienceFuelCraft)
			{
				Inventory.AddItemsToInventory(OutputItems.MakeOutput(ce.PreOutputItems));
			}
		}
		if (ce is CraftElementMix craftElementMix)
		{
			AlchemyMixDef alchemyMixDef = GameBalance.GetAlchemyMixDef(craftElementMix.CraftId);
			MainGame.Instance.GameSave.knowledgeSystem.DiscoverAlchemyMix(alchemyMixDef);
			DoTechPointsReward(alchemyMixDef.techRed.EvaluateInt(this), alchemyMixDef.techGreen.EvaluateInt(this), alchemyMixDef.techBlue.EvaluateInt(this));
			EvaluateOnCraftEndExpressions(alchemyMixDef.onCraftEndExpressions);
		}
		if (ce is CraftElement craftElement)
		{
			if (ce.Def.isAuto)
			{
				AddGameRes("game_res_tech_red", craftElement.Definition.techRed.EvaluateInt(this));
				AddGameRes("game_res_tech_green", craftElement.Definition.techGreen.EvaluateInt(this));
				AddGameRes("game_res_tech_blue", craftElement.Definition.techBlue.EvaluateInt(this));
			}
			else
			{
				DoTechPointsReward(craftElement.Definition.techRed.EvaluateInt(this), craftElement.Definition.techGreen.EvaluateInt(this), craftElement.Definition.techBlue.EvaluateInt(this));
			}
			if (ce.IsFailed)
			{
				return;
			}
			if (!string.IsNullOrEmpty(craftElement.Definition.globalScriptOnCraftEnd))
			{
				FireEvent(craftElement.Definition.globalScriptOnCraftEnd);
			}
			CraftDef craftDef = craftElement.Definition;
			if (craftDef.autopsyTypeCraft != 0)
			{
				switch (craftDef.autopsyTypeCraft)
				{
				case AutopsyTypeCraft.ExtractOrgan:
					DefineResultForExtractOrganCraft(craftDef, ce);
					break;
				case AutopsyTypeCraft.InsertOrgan:
					DefineResultForInsertOrganCraft(craftDef, ce);
					break;
				case AutopsyTypeCraft.ChangeOrgan:
					DefineResultForChangeOrganCraft(craftDef, ce);
					break;
				case AutopsyTypeCraft.PocketExtract:
					DefineResultForExtractItemFromPocketCraft(craftDef, ce);
					break;
				case AutopsyTypeCraft.Embalm:
					DefineResultForInsertToPocketCraft(craftDef, ce);
					break;
				}
			}
			else if (craftDef.transferDestinationEnd != 0)
			{
				string destinationItemEnd = craftDef.destinationItemEnd;
				switch (craftDef.transferDestinationEnd)
				{
				case TransferDestination.Wgo:
					if (craftDef.transferNeedsToDestinationOnFinish)
					{
						Inventory.AddItemsToInventory(craftElement.CraftInput);
					}
					Inventory.AddItemsToInventory(OutputItems.MakeOutput(ce.PreToWgoOnFinishItems));
					Inventory.RemoveItems(craftDef.removeItemsFromWgo);
					MakeDrop(Inventory.RemoveItems(craftDef.dropFromWgoItemsEnd));
					break;
				case TransferDestination.ItemInside:
					if (craftDef.transferNeedsToDestinationOnFinish)
					{
						Inventory.AddItemsToNestedItemById(destinationItemEnd, craftElement.CraftInput);
					}
					Inventory.AddItemsToNestedItemById(destinationItemEnd, OutputItems.MakeOutput(ce.PreToWgoOnFinishItems));
					Inventory.RemoveItemsFromNestedItemById(destinationItemEnd, craftDef.removeItemsFromWgo);
					MakeDrop(Inventory.RemoveItemsFromNestedItemById(destinationItemEnd, craftDef.dropFromWgoItemsEnd));
					break;
				case TransferDestination.GroupItemInside:
					if (craftDef.transferNeedsToDestinationOnFinish)
					{
						Inventory.AddItemsToNestedItemByGroupId(destinationItemEnd, craftElement.CraftInput);
					}
					Inventory.AddItemsToNestedItemByGroupId(destinationItemEnd, OutputItems.MakeOutput(ce.PreToWgoOnFinishItems));
					Inventory.RemoveItemsFromNestedItemByGroupId(destinationItemEnd, craftDef.removeItemsFromWgo);
					MakeDrop(Inventory.RemoveItemsFromNestedItemByGroupId(destinationItemEnd, craftDef.dropFromWgoItemsEnd));
					break;
				}
			}
			EvaluateOnCraftEndExpressions(craftElement.Definition.onCraftEndExpressions);
			if (!string.IsNullOrEmpty(craftElement.Definition.replaceWgoId))
			{
				if (craftElement.Definition.replaceWgoId == "0")
				{
					if (base.Definition.wgoGroup == "spawner")
					{
						MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(this);
					}
					else
					{
						this.OnOccuredDeath?.Invoke();
						customDeathCallback = null;
						MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(this);
					}
				}
				else
				{
					WgoData wgoData = this;
					if (craftElement.Definition.transferDataOnReplace)
					{
						MainGame.Instance.GameSave.worldData.ChangeWgoData(this, craftElement.Definition.replaceWgoId);
					}
					else
					{
						wgoData = MainGame.Instance.GameSave.worldData.ReplaceWgoData(this, craftElement.Definition.replaceWgoId);
					}
					foreach (LazyExpression item in craftElement.Definition.executeOnReplace)
					{
						item.EvaluateBool(wgoData);
					}
				}
				if (!string.IsNullOrEmpty(craftElement.Definition.worldFxOnReplace))
				{
					WorldFX.Spawn(Position, craftElement.Definition.worldFxOnReplace);
				}
			}
		}
		if (Worker is ZombieWgoData { ZombieType: var zombieType } zombieWgoData)
		{
			switch (zombieType)
			{
			case ZombieType.Crafter:
				zombieWgoData.CrafterOnAttachedWgoCraftEnd(ce);
				break;
			case ZombieType.ConveyorCrafter:
				zombieWgoData.ConveyorCrafterOnAttachedWgoCraftEnd(ce);
				break;
			}
		}
	}

	private void DefineResultForInsertOrganCraft(CraftDef craftDef, CraftElementBase craftElementBase)
	{
		string autopsyItemId = craftDef.autopsyItemId;
		List<Item> itemsToAdd = new List<Item>
		{
			new Item(autopsyItemId)
		};
		if (craftElementBase.SucceededProgressTicks >= craftDef.goldLevel)
		{
			Debug.Log("#craft# Insert Organ Craft:[" + craftDef.id + "] success result");
			SetGameRes("reached_gold", 1);
			SetGameRes("reached_silver", 1);
			SetGameRes("reached_bronze", 1);
			Inventory.AddItemsToNestedItemByGroupId(craftDef.destinationItemEnd, itemsToAdd);
		}
		else if (craftElementBase.SucceededProgressTicks >= craftDef.silverLevel)
		{
			Debug.Log("#craft# Insert Organ Craft:[" + craftDef.id + "] medium result");
			SetGameRes("reached_silver", 1);
			SetGameRes("reached_bronze", 1);
		}
		else
		{
			Debug.Log("#craft# Insert Organ Craft:[" + craftDef.id + "] failed result");
			ItemDef mistakeForThisItem = GameBalance.Me.GetData<ItemDef>(autopsyItemId).GetMistakeForThisItem();
			Inventory.AddItemsToNestedItemByGroupId(craftDef.destinationItemEnd, new List<Item>
			{
				new Item(mistakeForThisItem.id)
			});
		}
	}

	private void DefineResultForExtractOrganCraft(CraftDef craftDef, CraftElementBase craftElementBase)
	{
		string autopsyItemId = craftDef.autopsyItemId;
		List<NeedItemData> items = new List<NeedItemData>
		{
			new NeedItemData(autopsyItemId, 1)
		};
		if (craftElementBase.SucceededProgressTicks >= craftDef.goldLevel)
		{
			Debug.Log("#craft# Extract Organ Craft:[" + craftDef.id + "] success result");
			SetGameRes("reached_gold", 1);
			SetGameRes("reached_silver", 1);
			SetGameRes("reached_bronze", 1);
			MakeDrop(Inventory.RemoveItemsFromNestedItemByGroupId(craftDef.destinationItemEnd, items));
		}
		else if (craftElementBase.SucceededProgressTicks >= craftDef.silverLevel)
		{
			Debug.Log("#craft# Extract Organ Craft:[" + craftDef.id + "] medium result");
			SetGameRes("reached_silver", 1);
			SetGameRes("reached_bronze", 1);
			Inventory.RemoveItemsFromNestedItemByGroupId(craftDef.destinationItemEnd, items);
		}
		else
		{
			Debug.Log("#craft# Extract Organ Craft:[" + craftDef.id + "] failed result");
			Inventory.RemoveItemsFromNestedItemByGroupId(craftDef.destinationItemEnd, items);
			ItemDef mistakeForThisItem = GameBalance.Me.GetData<ItemDef>(autopsyItemId).GetMistakeForThisItem();
			Inventory.AddItemsToNestedItemByGroupId(craftDef.destinationItemEnd, new List<Item>
			{
				new Item(mistakeForThisItem.id)
			});
		}
	}

	private void DefineResultForChangeOrganCraft(CraftDef craftDef, CraftElementBase craftElementBase)
	{
		string autopsyItemId = craftDef.autopsyItemId;
		List<Item> list = new List<Item>
		{
			new Item(autopsyItemId)
		};
		List<NeedItemData> list2 = new List<NeedItemData>
		{
			new NeedItemData(craftElementBase.CustomItems[0].id, 1)
		};
		if (craftElementBase.SucceededProgressTicks >= craftDef.goldLevel)
		{
			Debug.Log($"#craft# Change Organ Craft:[{craftDef.id}] success result toRemove:[{list2[0]}] toAdd:[{list[0]}]");
			SetGameRes("reached_gold", 1);
			SetGameRes("reached_silver", 1);
			SetGameRes("reached_bronze", 1);
			Inventory.RemoveItemsFromNestedItemByGroupId(craftDef.destinationItemEnd, list2);
			Inventory.AddItemsToNestedItemByGroupId(craftDef.destinationItemEnd, list);
		}
		else
		{
			Debug.Log("#craft# Change Organ Craft:[" + craftDef.id + "] failed result");
		}
	}

	private void DefineResultForExtractItemFromPocketCraft(CraftDef craftDef, CraftElementBase craftElementBase)
	{
		List<NeedItemData> list = new List<NeedItemData>
		{
			new NeedItemData(craftElementBase.CustomItems[0].id, craftElementBase.CustomItems[0].Count)
		};
		Debug.Log($"#craft# Extract Item From Pocket Craft:[{craftDef.id}] toRemove:[{list[0]}]");
		MakeDrop(Inventory.RemoveItemsFromNestedItemByGroupId(craftDef.destinationItemEnd, list));
	}

	private void DefineResultForInsertToPocketCraft(CraftDef craftDef, CraftElementBase craftElementBase)
	{
		List<Item> list = new List<Item>
		{
			new Item(craftElementBase.CustomItems[0].id)
		};
		Debug.Log($"#craft# Insert Item To Pocket Craft:[{craftDef.id}] toAdd:[{list[0]}]");
		Inventory.AddItemsToNestedItemByGroupId(craftDef.destinationItemEnd, list);
	}

	public void EvaluateOnCraftEndExpressions(List<LazyExpression> onCraftEndExpressions)
	{
		foreach (LazyExpression onCraftEndExpression in onCraftEndExpressions)
		{
			onCraftEndExpression.EvaluateBool(this);
		}
	}

	private void DoTechPointsReward(int r, int g, int b)
	{
		if (Worker is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.DoTechPointsReward(this, r, g, b);
			return;
		}
		List<Item> list = new List<Item>();
		string text = "game_res_tech_red";
		string text2 = "game_res_tech_green";
		string text3 = "game_res_tech_blue";
		if (r > 0)
		{
			list.Add(new Item(text, r));
		}
		if (g > 0)
		{
			list.Add(new Item(text2, g));
		}
		if (b > 0)
		{
			list.Add(new Item(text3, b));
		}
		MakeDrop(list);
	}

	public void DropStoredTechPoints()
	{
		string text = "game_res_tech_red";
		string text2 = "game_res_tech_green";
		string text3 = "game_res_tech_blue";
		int gameResInt = GetGameResInt(text);
		int gameResInt2 = GetGameResInt(text2);
		int gameResInt3 = GetGameResInt(text3);
		DoTechPointsReward(gameResInt, gameResInt2, gameResInt3);
		SetGameRes(text, 0);
		SetGameRes(text2, 0);
		SetGameRes(text3, 0);
	}

	public void OnCraftCancel(CraftElementBase craftElement)
	{
	}

	public virtual MultiInventory GetCraftableMultiInventory(bool excludeWorkerInventory = false)
	{
		MultiInventory multiInventory = new MultiInventory();
		if (!excludeWorkerInventory && Worker != null)
		{
			multiInventory.Add(Worker.WorkerInventory);
		}
		if (WorldZoneData != null)
		{
			multiInventory.Add(new MultiInventory(WorldZoneData));
			if (!excludeWorkerInventory && !(Worker is ZombieWgoData))
			{
				multiInventory.Add(CraftInventory);
			}
			PlayerData playerData = MainGame.PlayerData;
			if (!excludeWorkerInventory && playerData.CurrentWorldZoneData == WorldZoneData && Worker is PlayerController != (bool)MainGame.PlayerController)
			{
				multiInventory.Add(playerData.Inventory);
			}
		}
		else
		{
			multiInventory.Add(new MultiInventory(new List<Inventory> { Inventory, CraftInventory }));
		}
		return multiInventory;
	}

	public void OnSuccessfulTicksChange(int startTick, int endTick, CraftElementBase craftElement)
	{
		if (!(craftElement is CraftElement craftElement2))
		{
			return;
		}
		foreach (GameResPerProgress item in craftElement2.Definition.gameresPerSuccessfulProgress)
		{
			if (item.sucessfulProgressTick > startTick && item.sucessfulProgressTick <= endTick)
			{
				AddGameRes(item.gameRes);
			}
		}
	}

	public void MakeDrop(List<Item> items)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (Item item in items)
		{
			if (item.id.EndsWith("tech_red"))
			{
				num += item.Count;
			}
			else if (item.id.EndsWith("tech_green"))
			{
				num2 += item.Count;
			}
			else if (item.id.EndsWith("tech_blue"))
			{
				num3 += item.Count;
			}
			else
			{
				MakeDrop(item);
			}
		}
		if (num + num2 + num3 > 0)
		{
			DockPointData nearestDockPointData = GetNearestDockPointData(MainGame.PlayerData.position.Value);
			Vector3 pos = ((nearestDockPointData == null) ? Position : (nearestDockPointData.GetDropPos(ItemSize.Small, 0.3f, 0.4f, 0.5f) + Position));
			MainGame.Instance.dropSystem.DropTechPoints(pos, num, num2, num3);
		}
	}

	public void DropHappiness()
	{
		TechPointsSpawner.CreateSpawner(Position, 0, 0, 0, GetGameResInt("happiness"));
		SetGameRes("happiness", 0);
	}

	public void MakeDrop(Item item)
	{
		Vector3 dropPos = GetDropPos(item);
		if (Worker is ZombieWgoData zombieWgoData)
		{
			if (zombieWgoData.ZombieType == ZombieType.Gardener)
			{
				if (zombieWgoData.WorkerInventory.CanAddItemToInventory(item))
				{
					zombieWgoData.WorkerInventory.AddItemToInventory(item);
				}
				else
				{
					MainGame.Instance.dropSystem.DropItem(item, worldId, dropPos);
				}
			}
			else if (item.Definition.itemGroupIds.Contains("town_box"))
			{
				MainGame.Instance.dropSystem.DropItem(item, worldId, dropPos);
			}
			else
			{
				zombieWgoData.CrafterAddCraftDrop(item);
			}
		}
		else
		{
			MainGame.Instance.dropSystem.DropItem(item, worldId, dropPos);
		}
	}

	public void ProcessReadyToFinishCraft()
	{
		CraftComponent.TryFinishCurCraft();
		List<Item> list = new List<Item>();
		list.AddRange(CraftableObjectCraftInventory.Data.RemoveAllItems());
		if (list.Count > 0)
		{
			MakeDrop(list);
		}
		DropStoredTechPoints();
	}

	[NetworkMethod(typeof(WgoDataCommand), "ApplyTool", new object[] { })]
	public void NotifyApplyTool(bool isFirstHit)
	{
		this.OnToolTickApply?.Invoke(isFirstHit);
	}

	public void SetCustomDeathMoment()
	{
		customDeathCallback = RunLogicsAfterDeath;
		customDeathWasTriggered = false;
	}

	public void TriggerCustomDeathMoment()
	{
		if (customDeathCallback == null)
		{
			Debug.LogWarning("No set custom death moment, but you trying call it. Do nothing.");
			return;
		}
		customDeathCallback();
		customDeathCallback = null;
		customDeathWasTriggered = true;
	}

	public Vector3 GetDockPointDataWorldPosition(DockPointData dockPointData)
	{
		if (dockPointData?.BakedData == null)
		{
			return Position;
		}
		WgoPartStateData wgoPartStateData = MainWgoPartData?.AvailableVariations?.Find((WgoPartStateData x) => x.rotationIndex == MainWgoPartData.rotationIndex);
		if (wgoPartStateData != null && wgoPartStateData.mirror)
		{
			return Position - dockPointData.BakedData.Position;
		}
		return Position + dockPointData.BakedData.Position;
	}

	public Vector3 GetFirstDockPointDataWorldPosition()
	{
		DockPointData dockPointByIndex = MainWgoPartData.GetDockPointByIndex(0);
		if (dockPointByIndex != null)
		{
			return GetDockPointDataWorldPosition(dockPointByIndex);
		}
		return Position;
	}

	public Vector3 GetNearestDockPointDataWorldPositionOrMyPosition(Vector3 positionFrom, DockPointData.Availability availability = DockPointData.Availability.OnlyNotOccupied, Func<DockPointData, Vector3, bool> additionalCheck = null)
	{
		DockPointData nearestDockPoint = MainWgoPartData.GetNearestDockPoint(this, positionFrom, availability);
		if (nearestDockPoint == null)
		{
			nearestDockPoint = MainWgoPartData.GetNearestDockPoint(this, positionFrom, DockPointData.Availability.All, DockPointData.Filter.All, additionalCheck);
		}
		if (nearestDockPoint == null)
		{
			return Position;
		}
		return GetDockPointDataWorldPosition(nearestDockPoint);
	}

	public DockPointData GetNearestDockPointData(Vector3 positionFrom, DockPointData.Availability availability = DockPointData.Availability.OnlyNotOccupied)
	{
		return MainWgoPartData.GetNearestDockPoint(this, positionFrom, availability);
	}

	public void AddWgoPart(string wgoPartId, string stateId = "", int rotationIndex = -1)
	{
		for (int i = 0; i < additionalWgoPartsData.Count; i++)
		{
			if (additionalWgoPartsData[i].id == wgoPartId)
			{
				Debug.LogWarning("Can not add wgo part [" + wgoPartId + "] to wgo [" + id + "]. Already has it");
				return;
			}
		}
		WgoPartData wgoPartData = new WgoPartData(wgoPartId, stateId, (rotationIndex == -1) ? mainWgoPartData.rotationIndex : rotationIndex);
		additionalWgoPartsData.Add(wgoPartData);
		this.OnAdditionalWgoPartAdd?.Invoke(wgoPartData);
	}

	public void RemoveWgoPart(string wgoPartId)
	{
		for (int i = 0; i < additionalWgoPartsData.Count; i++)
		{
			if (additionalWgoPartsData[i].id == wgoPartId)
			{
				WgoPartData obj = additionalWgoPartsData[i];
				additionalWgoPartsData.RemoveAt(i);
				this.OnAdditionalWgoPartRemove?.Invoke(obj);
				return;
			}
		}
		Debug.LogWarning("Can not remove wgo part [" + wgoPartId + "] from wgo [" + id + "]. Doesnt have it");
	}

	public void RemoveAllWgoParts()
	{
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			this.OnAdditionalWgoPartRemove?.Invoke(additionalWgoPartsDatum);
		}
		additionalWgoPartsData.Clear();
	}

	public void ApplyWgoPartState(string variationId, int rotationIndex = -1)
	{
		mainWgoPartData.TryApplyState(UniqueId, variationId, rotationIndex);
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			additionalWgoPartsDatum.TryApplyState(UniqueId, variationId, rotationIndex);
		}
	}

	public void ApplyRandomState()
	{
		if (mainWgoPartData.AvailableVariations.Count > 0)
		{
			WgoPartStateData random = mainWgoPartData.AvailableVariations.GetRandom();
			mainWgoPartData.TryApplyState(UniqueId, random.id, random.rotationIndex);
		}
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			if (additionalWgoPartsDatum.AvailableVariations.Count > 0)
			{
				WgoPartStateData random2 = additionalWgoPartsDatum.AvailableVariations.GetRandom();
				additionalWgoPartsDatum.TryApplyState(UniqueId, random2.id, random2.rotationIndex);
			}
		}
	}

	public void ApplyWgoPartState(string wgoPartId, string variationId, int rotationIndex = -1)
	{
		if (mainWgoPartData.id == wgoPartId)
		{
			mainWgoPartData.TryApplyState(UniqueId, variationId, rotationIndex);
			return;
		}
		foreach (WgoPartData additionalWgoPartsDatum in additionalWgoPartsData)
		{
			if (additionalWgoPartsDatum.id == wgoPartId)
			{
				additionalWgoPartsDatum.TryApplyState(UniqueId, variationId, rotationIndex);
				break;
			}
		}
	}

	public float GetGameRes(string id)
	{
		return gameRes.Get(id);
	}

	public int GetGameResInt(string id)
	{
		return gameRes.GetInt(id);
	}

	public void SetGameRes(string id, int value)
	{
		gameRes.Set(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public void SetGameRes(GameRes gameRes)
	{
		this.gameRes.Set(gameRes);
		foreach (GameResAtom item in gameRes.List)
		{
			this.OnGameResChanged?.Invoke(item.type);
		}
	}

	public void SetGameRes(string id, float value)
	{
		gameRes.Set(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public bool IsGameResEmpty()
	{
		return gameRes.IsEmpty();
	}

	public void AddGameRes(GameRes gameRes)
	{
		this.gameRes.Add(gameRes);
		foreach (GameResAtom item in gameRes.List)
		{
			this.OnGameResChanged?.Invoke(item.type);
		}
	}

	public void SubGameRes(string id, int value)
	{
		gameRes.Sub(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public void SubGameRes(string id, float value)
	{
		gameRes.Sub(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public void AddGameRes(string id, int value)
	{
		gameRes.Add(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public void AddGameRes(string id, float value)
	{
		gameRes.Add(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public void MultiplyGameRes(string id, float value)
	{
		gameRes.Multiply(id, value);
		this.OnGameResChanged?.Invoke(id);
	}

	public void AddInteractionEvent(string id, bool isFake = false)
	{
		InteractionEvent item = new InteractionEvent(id, isFake);
		events.Add(item);
		Debug.Log($"Added interaction event: {id}, wgo: {base.id}, events count: {events.Count}");
		NotifyInteractionEventChanged();
	}

	public void RemoveInteractionEvent(string id)
	{
		int num = events.FindIndex((InteractionEvent ev) => ev.str == id);
		if (num != -1)
		{
			Debug.Log($"Removed interaction event: {id}, wgo: {base.id}, events count: {events.Count}");
			events.RemoveAt(num);
			NotifyInteractionEventChanged();
		}
	}

	public bool FireInteractionEvent()
	{
		if (events.Count == 0)
		{
			return false;
		}
		InteractionEvent interactionEvent = events.PopFirst();
		if (!interactionEvent.isFake)
		{
			FireEvent(interactionEvent.str);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.CustomInteraction, id + ":" + interactionEvent.str);
			NotifyInteractionEventChanged();
			return true;
		}
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.Interaction, id ?? "");
		NotifyInteractionEventChanged();
		return false;
	}

	private void NotifyInteractionEventChanged()
	{
		this.OnInteractionEventChanged?.Invoke();
		WgoData.OnAnyInteractionEventChanged?.Invoke(this);
	}

	public InteractionEvent PeekFirstAddedEvent()
	{
		if (events.Count == 0)
		{
			return null;
		}
		return events[0];
	}

	public void RewriteGdPointsData(List<GDPointData> points)
	{
		if (gdPointsData.Count > 0)
		{
			MainGame.Instance.GameSave.worldData.gdPointsData.RemoveScenePoints(gdPointsData);
		}
		if (points.Count == 0)
		{
			gdPointsData = new List<GDPointData>();
			return;
		}
		gdPointsData = points;
		MainGame.Instance.GameSave.worldData.gdPointsData.AddScenePoints(points);
	}

	public GDPointData GetGDPointData(string id)
	{
		foreach (GDPointData gdPointsDatum in gdPointsData)
		{
			if (gdPointsDatum.Id == id)
			{
				return gdPointsDatum;
			}
		}
		return null;
	}

	public bool TrySetWorker(IWorker worker, DockPointData dockPointData = null)
	{
		this.OnWorkerChanged?.Invoke();
		if (!workerId.IsEmpty)
		{
			return false;
		}
		workerId.SetGuid(worker.Id);
		if (worker is ZombieWgoData zombieWgoData)
		{
			TryAddWorkerCutUnit(zombieWgoData);
			if (dockPointData == null)
			{
				dockPointData = MainWgoPartData.GetNearestDockPoint(this, zombieWgoData.Position, DockPointData.Availability.All, DockPointData.Filter.OnlyZombie);
			}
			if (dockPointData == null)
			{
				dockPointData = MainWgoPartData.GetNearestDockPoint(this, zombieWgoData.Position);
			}
			if (dockPointData != null)
			{
				dockPointData.Occupy(zombieWgoData.UniqueId);
				zombieWgoData.takenDockPointsParentSGuid = UniqueId;
			}
		}
		this.OnWorkerChanged?.Invoke();
		return true;
	}

	public void TryAddWorkerCutUnit(IWorker worker)
	{
		if (!isWorkerCutUnitAdded && worker is ZombieWgoData zombieWgoData)
		{
			isWorkerCutUnitAdded = true;
			float num = LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.Get(zombieWgoData.Definition.ResolveAssetId(zombieWgoData.Definition.id, zombieWgoData))?.RadiusSpehereCutter ?? (-1f);
			WorldZoneData worldZoneDataById = MainGame.Instance.GameSave.WorldData.GetWorldZoneDataById(worldZoneDataId);
			if (num > 0f && worldZoneDataById != null)
			{
				LazySingleton<GlobalNavigationManager>.Instance.AddCutUnit(zombieWgoData.UniqueId, worldZoneDataById.navigationGraph, zombieWgoData.Position, num);
			}
		}
	}

	public void TryRemoveWorkerCutUnit(IWorker worker)
	{
		if (isWorkerCutUnitAdded)
		{
			isWorkerCutUnitAdded = false;
			LazySingleton<GlobalNavigationManager>.Instance.RemoveCutUnit(worker.Id);
		}
	}

	public void ClearWorker()
	{
		if (!workerId.IsEmpty)
		{
			TryRemoveWorkerCutUnit(Worker);
			MainWgoPartData.TryFreeDockPoint(UniqueId, workerId);
		}
		workerId = SGuid.Empty;
		this.OnWorkerChanged?.Invoke();
	}

	public Vector3 GetDropPos(Item item)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(UniqueId);
		if (wgoViewGlobal == null)
		{
			return Position;
		}
		if (wgoViewGlobal.dropPoint != null)
		{
			return wgoViewGlobal.dropPoint.DropPos;
		}
		DockPointData nearestDockPoint = MainWgoPartData.GetNearestDockPoint(this, MainGame.PlayerData.position.Value);
		if (nearestDockPoint != null)
		{
			return nearestDockPoint.GetDropPos(item.Definition.itemSize, 0.3f, 0.4f, 0.5f) + Position;
		}
		return Position;
	}

	public void AddWorkbenchExtension(SGuid other)
	{
		if (!attachedWorkbenchExtensions.Contains(other))
		{
			attachedWorkbenchExtensions.Add(other);
			OnWorkbenchExtensionAttached();
		}
	}

	private void RemoveFullCoverSoftSlotExtensions()
	{
		for (int num = attachedWorkbenchExtensions.Count - 1; num >= 0; num--)
		{
			WgoData wgoData = MainGame.Instance.GameSave.worldData.GetWgoData(attachedWorkbenchExtensions[num]);
			if (wgoData != null && IsFullCoverSoftSlotExtension(wgoData))
			{
				wgoData.DropItemsOnCascadeRemove();
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(wgoData);
			}
		}
	}

	private void DropItemsOnCascadeRemove()
	{
		GameBalance.Me.removableWgos.TryGetValue(id, out var value);
		GameBalance.Me.buildableWgos.TryGetValue(id, out var value2);
		List<ItemCount> list = null;
		if (value != null)
		{
			list = value.outputItems.MakePreOutput(this);
		}
		else if (value2 != null)
		{
			list = new List<ItemCount>();
			foreach (NeedItemData needItem in value2.needItems)
			{
				list.Add(new ItemCount(needItem.id, needItem.GetCount()));
			}
		}
		if (list != null)
		{
			foreach (Item item in OutputItems.MakeOutput(list))
			{
				MakeDrop(item);
			}
		}
		if (Inventory.Data == null)
		{
			return;
		}
		foreach (Item item2 in Inventory.Data.Inventory)
		{
			MakeDrop(item2);
		}
	}

	private static bool IsFullCoverSoftSlotExtension(WgoData wgoData)
	{
		if (GameBalance.Me.buildableWgos.TryGetValue(wgoData.id, out var value))
		{
			return value.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft;
		}
		return false;
	}

	public void RemoveWorkbenchExtension(SGuid other)
	{
		attachedWorkbenchExtensions.Remove(other);
		OnWorkbenchExtensionRemoved();
	}

	public void AddWorkbenchParent(SGuid other)
	{
		if (!workbenchParents.Contains(other))
		{
			workbenchParents.Add(other);
		}
	}

	public void RemoveWorkbenchParent(SGuid other)
	{
		workbenchParents.Remove(other);
	}

	private void OnWorkbenchExtensionAttached()
	{
		if (id == "garden_empty" || id == "vineyard_empty")
		{
			GardenInteractionHandler.TryPlacePlantOrder(this);
		}
		else if ((id.StartsWith("garden_") || id.StartsWith("vineyard_")) && id.EndsWith("_ready"))
		{
			GardenInteractionHandler.TryPlaceGatherOrder(this);
		}
	}

	private void OnWorkbenchExtensionRemoved()
	{
		if (id.StartsWith("garden_") || id.StartsWith("vineyard_"))
		{
			WorldZoneData?.RemoveOrdersByTarget(UniqueId);
		}
	}

	public void ForceDeath()
	{
		this.OnOccuredDeath?.Invoke();
		if (customDeathCallback != null)
		{
			TriggerCustomDeathMoment();
		}
		else
		{
			RunLogicsAfterDeath();
		}
	}

	public void HandleDeath()
	{
		this.OnOccuredDeath?.Invoke();
		if (customDeathCallback == null)
		{
			RunLogicsAfterDeath();
		}
	}

	private void RunLogicsAfterDeath()
	{
		if ((LazyNetwork.IsInitialized && LazyNetwork.NetworkManager.IsCoopGame && !LazyNetwork.NetworkManager.IsHost) || customDeathWasTriggered)
		{
			return;
		}
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.WgoDead, id);
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.WgoCustomTagDead, customTag);
		MainWgoPartData?.TryDisableDockPoints(UniqueId);
		if (!SGuid.IsNullOrEmpty(takenDockPointsParentSGuid))
		{
			WgoData wgoData = MainGame.Instance.GameSave.WorldData.GetWgoData(takenDockPointsParentSGuid);
			if (wgoData != null)
			{
				wgoData.mainWgoPartData?.TryFreeDockPoint(wgoData.UniqueId, UniqueId);
				takenDockPointsParentSGuid = SGuid.Empty;
			}
		}
		if (base.Definition.deathChanceItems.HasOutput())
		{
			List<Item> list = OutputItems.MakeOutput(base.Definition.deathChanceItems.MakePreOutput(this));
			for (int i = 0; i < list.Count; i++)
			{
				MakeDrop(list[i]);
			}
		}
		if (base.Definition.dropInventoryOnDeath)
		{
			foreach (Item item in Inventory.Data.Inventory)
			{
				MakeDrop(item);
			}
			Inventory.Data.RemoveAllItems();
		}
		foreach (LazyExpression item2 in base.Definition.executeOnDeath)
		{
			item2.EvaluateBool(this);
		}
		if (base.Definition.inspirationOnDeath.HasExpression)
		{
			base.Definition.inspirationOnDeath.EvaluateBool(this);
		}
		if (CraftComponent.IsStarted && base.Definition.interactionType != WGODef.InteractionType.WellUpgrade)
		{
			CraftComponent.Cancel();
		}
		DoTechPointsReward(base.Definition.techRed, base.Definition.techGreen, base.Definition.techBlue);
		if (!base.Definition.reviveOnDie)
		{
			string text = base.Definition.replaceToWgoOnDie.Evaluate(this);
			if (!string.IsNullOrEmpty(text))
			{
				WgoData wgoData2 = this;
				if (base.Definition.transferDataToNewWgo)
				{
					MainGame.Instance.GameSave.WorldData.ChangeWgoData(this, text);
				}
				else
				{
					List<SpawnStage> list2 = new List<SpawnStage>();
					list2.AddRange(SpawnWGOComponent.SpawnStages);
					wgoData2 = MainGame.Instance.GameSave.WorldData.ReplaceWgoData(this, text);
					wgoData2.SpawnWGOComponent.SpawnStages = list2;
				}
				foreach (LazyExpression item3 in base.Definition.executeOnReplace)
				{
					item3.EvaluateBool(wgoData2);
				}
				if (Worker != null && Worker is ZombieWgoData { ZombieType: ZombieType.Gardener } zombieWgoData)
				{
					zombieWgoData.GardenerStopWorkActivity(wgoData2.UniqueId);
				}
				return;
			}
			MainGame.Instance.GameSave.WorldData.RemoveWgoDataFromGameScene(this);
		}
		LazySingleton<GlobalNavigationManager>.Instance.RemoveCutUnit(uniqueId);
		StopCustomNavMeshCutTracking();
	}

	private void HandleRevive()
	{
		hpComponent.RestoreFullHp();
	}

	public virtual void SetDataFromDefinition()
	{
		if (base.Definition == null)
		{
			return;
		}
		if (base.Definition.inventorySize > 0)
		{
			inventory = Inventory.Create(base.Definition.inventorySize, base.Definition.isFuelContainer, base.Definition.inventoryWhiteList, base.Definition.inventoryBlackList, id);
			foreach (NeedItemData startItem in base.Definition.startItems)
			{
				inventory.AddItemToInventory(new Item(startItem.id, startItem.GetCount()));
			}
		}
		else
		{
			inventory = Inventory.GetEmpty();
		}
		if (base.Definition.craftInventorySize > 0)
		{
			craftInventory = Inventory.GetCraftInventory(base.Definition.craftInventorySize);
		}
		hpComponent = ((base.Definition.startHpValue == -1) ? new HPComponent(base.Definition.hp) : new HPComponent(base.Definition.hp, base.Definition.startHpValue));
		SetupForcedNavigationHoleIfNeeded();
		if (!string.IsNullOrEmpty(base.Definition.zombieRollDataId))
		{
			ZombieSkinHelper.RollAndApplyZombieSkinToWgoData(this, base.Definition.zombieRollDataId);
		}
		if (base.Definition.townQuality > 0)
		{
			MainGame.Instance.GameSave.townSystem.Quality += base.Definition.townQuality;
		}
		if (!string.IsNullOrEmpty(base.Definition.npcLifeSimGroup))
		{
			NPCGroupPointOfInterestData groupById = MainGame.Instance.GameSave.npcLifeSimulatorData.GetGroupById(base.Definition.npcLifeSimGroup);
			if (groupById == null)
			{
				Debug.LogError("Can't add wgo to npc life sim group:[" + base.Definition.npcLifeSimGroup + "]");
			}
			else
			{
				groupById.AddWgoToGroup(this);
			}
		}
		if (!string.IsNullOrEmpty(base.Definition.npcLifeSimHome))
		{
			gameResStr.Set("npc_life_sim_home", base.Definition.npcLifeSimHome);
		}
		if (base.Definition.interactionType != WGODef.InteractionType.PorterStation)
		{
			return;
		}
		PorterStationDef data = GameBalance.Me.GetData<PorterStationDef>(base.Definition.id);
		if (data == null)
		{
			return;
		}
		foreach (NeedItemData item in data.items)
		{
			SetGameRes(item.id, (item.GetCount() > 0) ? 1 : 0);
		}
	}

	public void AddPerk(string id)
	{
		PerkData perkData = activePerks.Find((PerkData x) => x.id == id);
		if (perkData == null)
		{
			AddNewPerk(new PerkData(id));
			return;
		}
		switch (perkData.Definition.perkAddType)
		{
		case PerkAddType.Update:
			Debug.LogError($"Trying add perk:[{id}] to wgo:[{id}] with PerkAddType:[{PerkAddType.Update}], it is not supported for Wgo!!!");
			break;
		case PerkAddType.Sum:
			Debug.LogError($"Trying add perk:[{id}] to wgo:[{id}] with PerkAddType:[{PerkAddType.Sum}], it is not supported for Wgo!!!");
			break;
		case PerkAddType.AsNew:
			AddNewPerk(new PerkData(id));
			break;
		}
	}

	public void RemovePerk(string id)
	{
		PerkData perkData = activePerks.Find((PerkData x) => x.id == id);
		if (perkData != null)
		{
			RemovePerk(perkData);
		}
	}

	public void RemoveAllPerks()
	{
		for (int num = activePerks.Count - 1; num >= 0; num--)
		{
			RemovePerk(activePerks[num]);
		}
	}

	public void RemovePerk(PerkData perk)
	{
		activePerks.Remove(perk);
		if (perk.Definition.setGameResOnRemove.List.Count > 0)
		{
			SetGameRes(perk.Definition.setGameResOnRemove);
		}
		if (!perk.Definition.addGameResOnRemove.IsEmpty())
		{
			AddGameRes(perk.Definition.addGameResOnRemove);
		}
		foreach (LazyExpression onRemoveExpression in perk.Definition.onRemoveExpressions)
		{
			onRemoveExpression.EvaluateBool(this);
		}
		Debug.Log("Perk:[" + perk.id + "] removed from wgo:[" + id + "]");
	}

	public bool HasPerk(string id)
	{
		return activePerks.Find((PerkData x) => x.id == id) != null;
	}

	public PerkData GetPerk(string id)
	{
		return activePerks.Find((PerkData x) => x.id == id);
	}

	private void AddNewPerk(PerkData perk)
	{
		if (perk.Definition.duration > 0f)
		{
			Debug.LogError("Trying add perk:[" + perk.id + "] to wgo:[" + id + "] with duration, it is not supported for Wgo!!!");
			return;
		}
		perk.currentDuration = perk.Definition.duration;
		if (!perk.Definition.setGameResOnAdd.IsEmpty())
		{
			SetGameRes(perk.Definition.setGameResOnAdd);
		}
		if (!perk.Definition.addGameResOnAdd.IsEmpty())
		{
			AddGameRes(perk.Definition.addGameResOnAdd);
		}
		foreach (LazyExpression onAddExpression in perk.Definition.onAddExpressions)
		{
			onAddExpression.EvaluateBool(this);
		}
		activePerks.Add(perk);
		Debug.Log("Perk:[" + perk.id + "] added to wgo:[" + id + "]");
	}

	public virtual void OnReleaseWgoPartToPool()
	{
	}

	protected virtual float GetQuality()
	{
		if (id == "grave_ground")
		{
			return base.Definition.quality.EvaluateFloat(this) + Inventory.GetTotalQualityGrave();
		}
		if (id.StartsWith("npc_town_barracks_mercenary"))
		{
			if (!MainGame.Instance.GameSave.militaryBaseData.IsMercenaryPayed)
			{
				return 0f;
			}
			Item itemByType = inventory.GetItemByType(ItemType.BodyArmor);
			Item itemByGroupId = inventory.GetItemByGroupId("weapon");
			return ((!itemByType.IsEmpty) ? itemByType.Definition.quality : 0) + ((!itemByGroupId.IsEmpty) ? itemByGroupId.Definition.quality : 0);
		}
		if (MainGame.Instance.GameSave.militaryBaseData.ContainsFighter(this))
		{
			if (!(this is ZombieWgoData zombieWgoData))
			{
				return 0f;
			}
			if (zombieWgoData.Hand.IsEmpty || !zombieWgoData.Hand.Definition.isWeapon || zombieWgoData.Armor.IsEmpty)
			{
				return 0f;
			}
			return zombieWgoData.Hand.Definition.quality + zombieWgoData.Armor.Definition.quality;
		}
		return base.Definition.quality.EvaluateFloat(this) + (base.Definition.considerInventoryQuality ? Inventory.GetTotalQuality() : 0f);
	}

	private void HandleDirectionChanged(Vector2 direction)
	{
		this.OnDirectionChanged?.Invoke(direction);
	}

	public void SetTriggerToAnimator(string triggerName)
	{
		wasCustomAnimationFired = true;
		this.OnAnimationTriggerSet?.Invoke(triggerName);
	}

	public void SetLayerWeightToAnimator(int layerIndex, float weight)
	{
		this.OnAnimationLayerSet?.Invoke(layerIndex, weight);
	}

	public void SetStateToAnimator(AnimationState animationState)
	{
		wasCustomAnimationFired = true;
		this.OnAnimationStateSet?.Invoke(animationState);
	}

	public void TryFireSerializedTrigger()
	{
		if (!string.IsNullOrEmpty(customAnimationTrigger))
		{
			SetTriggerToAnimator(customAnimationTrigger);
		}
	}

	public void SetCustomAnimationTrigger(string triggerName = "")
	{
		customAnimationTrigger = triggerName;
	}

	public Vector3 GetTeleportPointPosition()
	{
		return MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById("tp_point_" + id)?.Position ?? Position;
	}

	public void OccupyDockPoint(DockPointData dockPoint, SGuid dockPointParent)
	{
		dockPoint.Occupy(UniqueId);
		takenDockPointsParentSGuid = dockPointParent;
		this.OnTakenDockPointChanged?.Invoke();
	}

	public void UnOccupyDockPoint(DockPointData dockPoint)
	{
		dockPoint.UnOccupy();
		takenDockPointsParentSGuid = SGuid.Empty;
		this.OnTakenDockPointChanged?.Invoke();
	}

	private void HandleHpChanged(HPComponent component)
	{
		if (GlobalEventsSystem.Me.GetEvents(GlobalEventsSystem.Event.Type.WgoCustomTagHpValueReached, out var dictionary))
		{
			List<string> list = null;
			foreach (KeyValuePair<string, GlobalEventsSystem.Event> item in dictionary)
			{
				string key = item.Key;
				int num = key.IndexOf(':');
				if (num >= 0 && num != key.Length - 1 && customTag != null && customTag.Length == num && (num <= 0 || string.CompareOrdinal(key, 0, customTag, 0, num) == 0) && TryParseIntInPlace(key, num + 1, out var value) && value < component.prevHp && value >= component.Hp)
				{
					(list ?? (list = new List<string>())).Add(key);
				}
			}
			if (list != null)
			{
				foreach (string item2 in list)
				{
					GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.WgoCustomTagHpValueReached, item2);
				}
			}
		}
		if (base.Definition.hpAction.isValidAction && component.Hp == base.Definition.hpAction.hp)
		{
			base.Definition.hpAction.EvaluateExpression(this);
		}
	}

	private static bool TryParseIntInPlace(string source, int startIndex, out int value)
	{
		value = 0;
		if (startIndex >= source.Length)
		{
			return false;
		}
		for (int i = startIndex; i < source.Length; i++)
		{
			char c = source[i];
			if (c < '0' || c > '9')
			{
				return false;
			}
			value = value * 10 + (c - 48);
		}
		return true;
	}

	private void HandleOnClearTownPalettes()
	{
		foreach (Item item in Inventory.Data.Inventory)
		{
			TownSystem.moneyForPalettes += item.Definition.basePrice * item.Count;
			foreach (LazyExpression item2 in item.Definition.expressionsOnSell)
			{
				item2.Evaluate(item);
			}
		}
		Inventory.Clear();
	}
}
