using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Wgo : MonoBehaviour, IBuildRemovable, IBubbleDrawable, IChunkableObject, IChunkVisibilityStateReceiver, ICombatEntity, IKnockbackable
{
	private const string DefaultTalkText = "Lorem ipsum dolor sit amet";

	private string editorTalkText = "Lorem ipsum dolor sit amet";

	[SerializeField]
	protected WgoData data;

	[SerializeField]
	[HideInInspector]
	public List<WgoPart> additionalWgoParts = new List<WgoPart>();

	[NonSerialized]
	public DropPoint dropPoint;

	[NonSerialized]
	public InteractingItemPoint interactionItemPoint;

	private ChunkBoundsPair bounds;

	private bool boundsCalculated;

	private bool isVisible;

	private bool lastReadyToFinishQuest;

	private bool subscribedQuestFinishIndicator;

	private bool spawnCompleted;

	[SerializeField]
	[HideInInspector]
	private Vector3 initialBoundsPosition;

	private bool registeredInChunker;

	public StartReses startReses;

	public Direction startDirection = Direction.Down;

	private WgoMovementAdjustComponent wgoMovementAdjustComponent;

	private bool hasData;

	private bool isDespawning;

	private bool hasCustomDestroyMoment;

	private bool isPlayingDestroyAnim;

	private AnimationComponentBase animationComponent;

	private bool wgoPartsLoaded;

	private bool wgoPartsLoading;

	private bool visualBindingsInitialized;

	private bool animationBindingsBound;

	private bool pendingApplyDefaultState;

	private bool needsWorkbenchExtensionInit;

	private bool pendingRefreshAfterCraftHintCompletion;

	private bool pendingDespawnAfterCraftHintCompletion;

	private ChunkVisibilityState chunkVisibilityState;

	private AttackComponent attackComponent;

	[NonSerialized]
	public List<LazyWidgetDataBase> worldZoneWidgetsData = new List<LazyWidgetDataBase>();

	private IWGOInteractionHandler interactionHandler;

	private bool isOverheadActive;

	private UIInteractingItem interactingItem;

	private bool areCollidersLayersOverrode;

	private Dictionary<int, int> collidersPreviousLayer = new Dictionary<int, int>();

	private string cachedFuelContainerItemId;

	private bool fuelContainerItemIdResolved;

	private const string POWER_SOURCE_GEAR_ICON = "gear";

	private string WGOId
	{
		get
		{
			return data.id;
		}
		set
		{
			data.id = value;
		}
	}

	private string CustomTag
	{
		get
		{
			return data.CustomTag;
		}
		set
		{
			data.CustomTag = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return data.IsHidden;
		}
		set
		{
			data.IsHidden = value;
		}
	}

	public bool RegisteredInChunker
	{
		get
		{
			return registeredInChunker;
		}
		set
		{
			registeredInChunker = value;
		}
	}

	public WgoData Data
	{
		get
		{
			return data;
		}
		private set
		{
			data = value;
		}
	}

	public bool HasData => hasData;

	public string Id => WGOId;

	public WgoPart MainWgoPart { get; private set; }

	public List<WgoPart> AdditionalWgoParts => additionalWgoParts;

	public IWGOInteractionHandler InteractionHandler => interactionHandler;

	public bool IsDespawning => isDespawning;

	public IReadOnlyList<DockPoint> DockPoints => MainWgoPart?.DockPoints;

	public RiverBodyReceiver RiverBodyReceiver { get; private set; }

	public bool UpdatePosByData { get; set; } = true;


	public SGuid BubbleDrawableUniqueId => data.UniqueId;

	public List<LazyWidgetDataBase> BubbleDrawableWidgets => GetWidgetData();

	public Vector3 BubbleDrawablePosition => GetBubblePointPos();

	public MultiFlagOR<ChunkingIgnoreType> IgnoreMultiFlag { get; set; }

	public SGuid CombatEntityUID => Data.UniqueId;

	public LazyConsts.Fighting.TeamType TeamType
	{
		get
		{
			if (!Data.id.Contains("zmb") || Data.id.Contains("allie"))
			{
				return LazyConsts.Fighting.TeamType.Player;
			}
			return LazyConsts.Fighting.TeamType.WildZombie;
		}
	}

	public LazyConsts.Fighting.EntityType EntityType
	{
		get
		{
			if (!Data.id.Contains("npc"))
			{
				if (!Data.id.Contains("zmb"))
				{
					return LazyConsts.Fighting.EntityType.None;
				}
				return LazyConsts.Fighting.EntityType.Zombie;
			}
			return LazyConsts.Fighting.EntityType.Soldier;
		}
	}

	public Vector3 CombatEntityPosition => Data.Position;

	public bool IsActiveCombatant { get; set; }

	public HPComponent CombatEntityHpComponent => Data.HpComponent;

	public int CombatEntityQuality => (int)data.Definition.quality.EvaluateFloat(data) + data.GetGameResInt("excitement");

	public int AttackPriority { get; set; }

	public bool HasAnyDockPoint => Data.MainWgoPartData.DockPointsCount > 0;

	public int ArmorValue
	{
		get
		{
			if (attackComponent == null)
			{
				attackComponent = GetComponentInChildren<AttackComponent>();
			}
			if (attackComponent == null)
			{
				return 0;
			}
			return attackComponent.Armor;
		}
	}

	public static event Action<Wgo> OnWgoSpawn;

	public static event Action<Wgo> OnWgoDestroy;

	private void RuntimeDirection(Direction direction)
	{
		if (data != null)
		{
			data.direction.Value = direction.ConvertToVector2XZ();
		}
	}

	private void Editor_Talk()
	{
		if (data != null)
		{
			string text = (string.IsNullOrWhiteSpace(editorTalkText) ? "Lorem ipsum dolor sit amet" : editorTalkText);
			PhraseData phraseData = default(PhraseData);
			phraseData.text = text;
			phraseData.isPlayer = false;
			phraseData.npcWgoData = data;
			phraseData.speechType = SpeechBubbleType.Talk;
			Bubble.Talk(phraseData);
		}
	}

	public DockPointData GetDockPointData(DockPoint dockPoint)
	{
		return MainWgoPart?.GetDockPointData(dockPoint);
	}

	public static Wgo Spawn(WgoData data, Transform parentTransform, bool registerInChunkManagerIfStatic = true, bool ignoreChunkRegistration = false, bool applyDefaultWgoPartState = false, bool recheckVisibilityOnSpawn = false)
	{
		Wgo wgo = new GameObject().AddComponent<Wgo>();
		wgo.Data = data;
		wgo.WGOId = data.id;
		wgo.wgoMovementAdjustComponent = wgo.gameObject.AddComponent<WgoMovementAdjustComponent>();
		wgo.pendingApplyDefaultState = applyDefaultWgoPartState;
		Transform obj = wgo.transform;
		obj.parent = parentTransform;
		obj.position = wgo.Data.Position;
		obj.localScale = wgo.Data.Scale;
		wgo.InitDataBindings();
		if (!wgo.Data.isTempObject)
		{
			if (!wgo.Data.gdPointsRegistered)
			{
				wgo.Data.gdPointsRegistered = true;
				wgo.RegisterGDPointsFromBakedData();
			}
			else
			{
				GardenBedNavigation.TryRebuildOnViewRespawn(wgo.Data);
			}
			if (!wgo.Data.wasSpawnedAtLeastOnce)
			{
				wgo.Data.wasSpawnedAtLeastOnce = true;
				WGODef workbenchExtensionLogicDef = GameBalance.Me.GetWorkbenchExtensionLogicDef(wgo.Data.id);
				if ((workbenchExtensionLogicDef != null && GameBalance.Me.workbenchesWhichUseExtensions.Contains(workbenchExtensionLogicDef)) || GameBalance.Me.IsWorkbenchExtensionId(wgo.Data.id))
				{
					wgo.needsWorkbenchExtensionInit = true;
				}
			}
		}
		WorldZoneData worldZoneData = data.WorldZoneData;
		try
		{
			if (worldZoneData != null && (!worldZoneData.Definition.hasCustomQualityZones || (worldZoneData.Definition.hasCustomQualityZones && worldZoneData.ContainsCustomQualityZonePrecisely(data.Position))))
			{
				wgo.SetWorldZoneWidgets(worldZoneData);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError($"wgo spawn:[{data.id}] wz:[{worldZoneData?.id}] wzData.Definition:[{worldZoneData?.Definition == null}] exception:{ex}");
		}
		wgo.PrecomputeSerializedBounds();
		wgo.gameObject.SetActive(value: false);
		wgo.spawnCompleted = true;
		if (!ignoreChunkRegistration && ((wgo.Data.Definition?.isMovable ?? false) || registerInChunkManagerIfStatic))
		{
			wgo.TryRegisterInChunkManager();
			if (recheckVisibilityOnSpawn)
			{
				LazySingleton<ChunkManager>.Instance.RequestVisibilityRecheck(wgo);
			}
			else
			{
				wgo.UpdateChunkVisibility(isVisible: true);
			}
		}
		Wgo.OnWgoSpawn?.Invoke(wgo);
		return wgo;
	}

	public void RemoveWithData()
	{
		MainGame.WorldData.RemoveWgoDataFromGameScene(data.UniqueId);
	}

	public void DespawnAfterDataWasRemoved()
	{
		isDespawning = true;
		if (MainGame.PlayerController.PlayerInteractionComponent.WgoUnderInteraction == this)
		{
			interactionHandler.OnInteractionTargetExit();
		}
		SetWorldZoneWidgets(null);
		if (ShouldDelayForCraftHintCompletion())
		{
			pendingDespawnAfterCraftHintCompletion = true;
			WgoBubbleDisplayHandler.Hide(this);
			return;
		}
		DeInit();
		if (!hasCustomDestroyMoment)
		{
			Wgo.OnWgoDestroy?.Invoke(this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public List<LazyWidgetDataBase> GetWidgetData()
	{
		List<LazyWidgetDataBase> list = new List<LazyWidgetDataBase>();
		if (data.IsHidden)
		{
			return list;
		}
		if (WorkbenchAdditionWorldIconPresenter.TryGet(data.UniqueId, out var state))
		{
			list.Add(new UIWorkbenchAdditionWorldIconWidgetData(state.IconId, state.IsInRange));
		}
		if (ShouldShowConveyorNoPowerIcon())
		{
			list.Add(new UIConveyorNoPowerIconWidgetData());
		}
		if (ZombieDeliveryIndication.ShouldShowNoCaretakerAssigned(data))
		{
			list.Add(UIWorkbenchAdditionWorldIconWidgetData.StationWithoutCaretaker());
		}
		if (((data.HpComponent.WasDamagedAtLeastOnce && data.HpComponent.Hp > 0) || data.HpComponent.firstDamageWasMax) && !data.Definition.hasInfiniteHp)
		{
			data.HpComponent.firstDamageWasMax = false;
			if (!IsActiveCombatant)
			{
				list.Add(new HpBarWidgetData(data.HpComponent));
			}
			else
			{
				HpBarSimpleWidgetData.SpriteType spriteType = ((TeamType == LazyConsts.Fighting.TeamType.Player) ? HpBarSimpleWidgetData.SpriteType.Ally : HpBarSimpleWidgetData.SpriteType.Enemy);
				if (MainWgoPart == null || !MainWgoPart.TryGetComponent<HpWidgetDataCustomParameters>(out var component))
				{
					list.Add(new HpBarSimpleWidgetData(data.HpComponent, 30f, 5f, spriteType));
				}
				else
				{
					list.Add(new HpBarSimpleWidgetData(data.HpComponent, component.GetCustomWidthIfHasSet(out var width) ? width : 30f, component.GetCustomHeightIfHasSet(out var height) ? height : 5f, spriteType));
				}
			}
		}
		if (TryGetFuelContainerStoredItem(out var itemId, out var count))
		{
			list.Add(new UIQualityTooltipWidgetData(itemId, count.ToInvariantCultureString()));
		}
		if (TryGetPowerSourceGearOutput(out var iconName, out var count2))
		{
			list.Add(new UIQualityTooltipWidgetData(iconName, count2.ToInvariantCultureString()));
		}
		CraftElementBase currentCraftElement = data.CraftComponent.CurrentCraftElement;
		if (ZombieDeliveryIndication.IsCraftStalledWithoutCaretaker(data))
		{
			list.Add(UIWorkbenchAdditionWorldIconWidgetData.NoCaretakerInZone());
		}
		else if (currentCraftElement != null && !currentCraftElement.Def.isHidden && currentCraftElement.ParamsData.craftParamsType != CraftParamsData.CraftParamsType.GardenGrowing && (data.CraftComponent.IsStarted || data.CraftComponent.Status == CraftComponentStatus.ReadyToStartCraft || data.CraftComponent.Status == CraftComponentStatus.ReadyToFinishAutoCraft || data.CraftComponent.IsQueueDelayed || data.CraftComponent.Status == CraftComponentStatus.WaitingForWorkerPickUp || data.CraftComponent.Status == CraftComponentStatus.WaitingForOutputDrop))
		{
			list.Add(new UICraftHintWidgetData(data.CraftComponent));
		}
		bool isControlsEnabledForInteractionHints = MainGame.PlayerController.IsControlsEnabledForInteractionHints;
		bool flag = (lastReadyToFinishQuest = MainGame.Instance.GameSave.questSystemData.WgoHasReadyToFinishQuest(Id));
		if (interactionHandler != null && data.IsInteractable && ((data.Events.Count > 0 && isControlsEnabledForInteractionHints) || ((MainGame.PlayerController.PlayerInteractionComponent.WgoUnderInteraction == this || MainGame.PlayerController.PlayerWorkComponent.Wgo == this) && (interactionHandler.HasInteraction(MainGame.PlayerController) || interactionHandler.HasInteraction2(MainGame.PlayerController))) || flag))
		{
			InteractionInfos interactionInfos = interactionHandler.GetInteractionInfos();
			List<UIInteractionHintRowWidgetData> list2 = new List<UIInteractionHintRowWidgetData>();
			foreach (InteractionInfo item in interactionInfos.list)
			{
				if (!string.IsNullOrEmpty(item.text) || !string.IsNullOrEmpty(item.customIconId))
				{
					list2.Add(new UIInteractionHintRowWidgetData(item));
				}
			}
			if (list2.Count > 0)
			{
				list.Add(new UIInteractionHintWidgetData(list2));
			}
		}
		if (MainGame.PlayerData.CurrentWorldZoneData != null && data.WorldZoneData == MainGame.PlayerData.CurrentWorldZoneData)
		{
			list.AddRange(worldZoneWidgetsData);
		}
		return list;
	}

	public void FireEvent(string eventName)
	{
		Data.FireEvent(eventName);
	}

	public void SetCustomBubblePoint(Transform customBubblePoint)
	{
		if (!(MainWgoPart == null))
		{
			MainWgoPart.SetCustomBubblePoint(customBubblePoint);
		}
	}

	public bool TryRotate(bool isPrev = false)
	{
		if (MainWgoPart.Variations.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < AdditionalWgoParts.Count; i++)
		{
			if (AdditionalWgoParts[i].Variations.Count == 0)
			{
				return false;
			}
		}
		if (!MainWgoPart.Rotate(isPrev))
		{
			return false;
		}
		for (int j = 0; j < AdditionalWgoParts.Count; j++)
		{
			if (!MainWgoPart.Rotate(isPrev))
			{
				return false;
			}
		}
		return true;
	}

	public bool CanBeRotated()
	{
		if (!MainWgoPart.CanBeRotated())
		{
			return false;
		}
		for (int i = 0; i < AdditionalWgoParts.Count; i++)
		{
			if (!AdditionalWgoParts[i].CanBeRotated())
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateWgoPartState(bool applyDefaultWgoPartState = false)
	{
		if (applyDefaultWgoPartState || string.IsNullOrEmpty(data.MainWgoPartData.variationId))
		{
			MainWgoPart?.ApplyWgoPartState();
		}
		else
		{
			MainWgoPart?.ApplyWgoPartState(data.MainWgoPartData.variationId, data.MainWgoPartData.rotationIndex);
		}
		for (int i = 0; i < data.AdditionalWgoPartsData.Count; i++)
		{
			AdditionalWgoParts[i]?.ApplyWgoPartState();
		}
		data.RefreshCustomNavMeshCutUnit();
	}

	public void ValidateSpawnerComponent()
	{
	}

	public void SetSelectionTint(Color color, float amount)
	{
		MainWgoPart?.SetSelectionTint(color, amount);
		for (int i = 0; i < AdditionalWgoParts.Count; i++)
		{
			AdditionalWgoParts[i]?.SetSelectionTint(color, amount);
		}
	}

	public void PlayHPTickAnimation(bool isFirstHit)
	{
		Action<Object3DMesh> hpTickAnimMethod = null;
		if (!string.IsNullOrEmpty(data.Definition.sfxOnActionTick))
		{
			LazyAudio.PlayAtGameObject(data.Definition.sfxOnActionTick, base.transform, SpatialType.sound3D);
		}
		switch (data.Definition.wgoGroup)
		{
		default:
			return;
		case "stones":
			if (isFirstHit && !string.IsNullOrEmpty(data.Definition.worldFxOnHpFirstHit))
			{
				WorldFX.Spawn(BubbleDrawablePosition, data.Definition.worldFxOnHpFirstHit, null, data.Definition.worldFxOnHpFirstHitActionSize);
			}
			if (string.IsNullOrEmpty(data.Definition.worldFxOnHpActionTick))
			{
				return;
			}
			WorldFX.Spawn(base.transform.position, data.Definition.worldFxOnHpActionTick, null, data.Definition.worldFxOnHpActionSize);
			break;
		case "trees":
		case "bushes":
		case "collectable_bushes":
			if (isFirstHit && !string.IsNullOrEmpty(data.Definition.worldFxOnHpFirstHit))
			{
				WorldFX.Spawn(BubbleDrawablePosition, data.Definition.worldFxOnHpFirstHit, null, data.Definition.worldFxOnHpFirstHitActionSize);
			}
			hpTickAnimMethod = delegate(Object3DMesh o3DMesh)
			{
				o3DMesh.PlayAnimationChop(delegate
				{
					if (!string.IsNullOrEmpty(data.Definition.worldFxOnHpActionTick))
					{
						WorldFX.Spawn(base.transform.position, data.Definition.worldFxOnHpActionTick, null, data.Definition.worldFxOnHpActionSize);
					}
				});
			};
			break;
		}
		if (hpTickAnimMethod != null)
		{
			MainWgoPart.Object3D?.Object3DMeshes.ForEach(delegate(Object3DMesh o3DMesh)
			{
				hpTickAnimMethod(o3DMesh);
			});
		}
	}

	public void PlayDestroyAnimation()
	{
		Action<Object3DMesh> destructionMeshAnimMethod = null;
		Action<HorizontalSprite> destructionHorSpriteAnimMethod = null;
		bool isDestructionMeshAnimMethodCalled = false;
		if (!string.IsNullOrEmpty(data.Definition.sfxOnDie))
		{
			LazyAudio.PlayAtGameObject(data.Definition.sfxOnDie, base.transform, SpatialType.sound3D);
		}
		string wgoGroup = data.Definition.wgoGroup;
		if (wgoGroup == "trees" || wgoGroup == "bushes")
		{
			destructionMeshAnimMethod = delegate(Object3DMesh o3DMesh)
			{
				o3DMesh.PlayAnimationDestruction(delegate
				{
					if (!isDestructionMeshAnimMethodCalled)
					{
						isDestructionMeshAnimMethodCalled = true;
						if (string.IsNullOrEmpty(data.Definition.worldFxOnDie))
						{
							data.TriggerCustomDeathMoment();
						}
						else
						{
							(Vector3, Vector3) fxOnDieParams2 = GetFxOnDieParams();
							if (data.Definition.customDeathTime > 0f)
							{
								hasCustomDestroyMoment = true;
								WorldFX.Spawn(fxOnDieParams2.Item1, data.Definition.worldFxOnDie, delegate
								{
									hasCustomDestroyMoment = false;
									isPlayingDestroyAnim = false;
									DespawnAfterDataWasRemoved();
								}, fxOnDieParams2.Item2);
								MainGame.Instance.GameSave.wgoCustomDeathSystemData.AddCustomDeath(data);
							}
							else
							{
								WorldFX.Spawn(fxOnDieParams2.Item1, data.Definition.worldFxOnDie, delegate
								{
									isPlayingDestroyAnim = false;
									data.TriggerCustomDeathMoment();
								}, fxOnDieParams2.Item2);
							}
						}
					}
				});
			};
			destructionHorSpriteAnimMethod = delegate(HorizontalSprite horSprite)
			{
				LazyTimer.AddTimer(LazySingletonSO<GlobalResources>.Instance.fxSettings.treeDestroyGndSpriteDisableTime, delegate
				{
					if (horSprite != null)
					{
						horSprite.gameObject.SetActive(value: false);
					}
				});
			};
			data.SetCustomDeathMoment();
			if ((bool)MainWgoPart.Object3D)
			{
				MainWgoPart.Object3D.Object3DMeshes.ForEach(delegate(Object3DMesh o3DMesh)
				{
					destructionMeshAnimMethod(o3DMesh);
				});
				MainWgoPart.Object3D.HorizontalSprites.ForEach(delegate(HorizontalSprite horSprite)
				{
					destructionHorSpriteAnimMethod(horSprite);
				});
				isPlayingDestroyAnim = true;
			}
		}
		else if (!string.IsNullOrEmpty(data.Definition.worldFxOnDie))
		{
			(Vector3, Vector3) fxOnDieParams = GetFxOnDieParams();
			WorldFX.Spawn(fxOnDieParams.Item1, data.Definition.worldFxOnDie, null, fxOnDieParams.Item2);
		}
	}

	private (Vector3 position, Vector3 size) GetFxOnDieParams()
	{
		if (data.Definition.getFxOnDieSizeFromBuildCollider)
		{
			if (MainWgoPart == null)
			{
				return (position: base.transform.position, size: data.Definition.fxOnDieSize);
			}
			BoxCollider boxCollider = FindBuildArea((MainWgoPart.CurrentWgoPartState == null) ? MainWgoPart.gameObject : MainWgoPart.CurrentWgoPartState.gameObject, LayerMask.NameToLayer("BuildArea"));
			if (boxCollider != null)
			{
				return new ValueTuple<Vector3, Vector3>(item2: new Vector3(boxCollider.size.x, 1f, boxCollider.size.z), item1: boxCollider.transform.TransformPoint(boxCollider.center));
			}
		}
		return (position: base.transform.position, size: data.Definition.fxOnDieSize);
		static BoxCollider FindBuildArea(GameObject parent, int layer)
		{
			foreach (Transform item2 in parent.transform)
			{
				if (item2.gameObject.layer == layer)
				{
					return item2.gameObject.GetComponent<BoxCollider>();
				}
				BoxCollider boxCollider2 = FindBuildArea(item2.gameObject, layer);
				if (boxCollider2 != null)
				{
					return boxCollider2;
				}
			}
			return null;
		}
	}

	public DockPoint TryGetDockPointForWorker(bool findNearest = false, Vector3 searcherPosition = default(Vector3))
	{
		IReadOnlyList<DockPoint> dockPoints = DockPoints;
		if (dockPoints != null && dockPoints.Count == 0)
		{
			return null;
		}
		if (!findNearest)
		{
			foreach (DockPoint dockPoint2 in DockPoints)
			{
				if (dockPoint2.gameObject.activeInHierarchy && !dockPoint2.DontUseForWorkerPlacement && dockPoint2.IsForZombie)
				{
					return dockPoint2;
				}
			}
			foreach (DockPoint dockPoint3 in DockPoints)
			{
				if (!dockPoint3.DontUseForWorkerPlacement && dockPoint3.gameObject.activeInHierarchy)
				{
					return dockPoint3;
				}
			}
			return null;
		}
		DockPoint dockPoint = null;
		DockPoint result = null;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		foreach (DockPoint dockPoint4 in DockPoints)
		{
			if (!dockPoint4.gameObject.activeInHierarchy || dockPoint4.DontUseForWorkerPlacement)
			{
				continue;
			}
			float num3 = Vector3.Distance(dockPoint4.transform.position, searcherPosition);
			if (dockPoint4.IsForZombie)
			{
				if (num3 < num)
				{
					num = num3;
					dockPoint = dockPoint4;
				}
			}
			else if (num3 < num2)
			{
				num2 = num3;
				result = dockPoint4;
			}
		}
		if (!(dockPoint != null))
		{
			return result;
		}
		return dockPoint;
	}

	public void SetInteractableCollidersState(bool isActive)
	{
		MainWgoPart?.InteractableColliders.ForEach(delegate(Collider collider)
		{
			collider.enabled = isActive;
		});
	}

	public void SetLayerToAllColliders(int layer)
	{
		if (areCollidersLayersOverrode)
		{
			ResetLayerFromAllColliders();
		}
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			collidersPreviousLayer.TryAdd(collider.GetHashCode(), collider.gameObject.layer);
			collider.gameObject.layer = layer;
		}
		areCollidersLayersOverrode = true;
	}

	public void ResetLayerFromAllColliders()
	{
		if (!areCollidersLayersOverrode)
		{
			return;
		}
		areCollidersLayersOverrode = false;
		Collider[] componentsInChildren = base.gameObject.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collidersPreviousLayer.TryGetValue(collider.GetHashCode(), out var value))
			{
				collider.gameObject.layer = value;
			}
		}
		collidersPreviousLayer.Clear();
	}

	private void InitDataBindings()
	{
		hasData = true;
		IgnoreMultiFlag = new MultiFlagOR<ChunkingIgnoreType>();
		data.OnPositionChanged += HandleChangedPos;
		data.HpComponent.OnFirstDamageDealt += HandleFirstDamageDealt;
		data.HpComponent.OnFullHpRestored += DrawWidgets;
		data.CraftComponent.OnStatusChanged += HandleCraftStatusChange;
		data.CraftComponent.OnPreFinishHoldReleased += HandleCraftPreFinishHoldReleased;
		data.OnWorkerChanged += DrawWidgets;
		data.OnAdditionalWgoPartAdd += OnAdditionalWgoPartAdd;
		data.OnAdditionalWgoPartRemove += OnAdditionalWgoPartRemove;
		data.OnInteractionEventChanged += DrawWidgets;
		data.OnInteractableStateChanged += HandleInteractedStateChanged;
		data.OnHiddenStateChanged += HandleHiddenChanged;
		wgoMovementAdjustComponent.Init(data);
		data.OnToolTickApply += PlayHPTickAnimation;
		data.OnOccuredDeath += PlayDestroyAnimation;
		data.Inventory.OnItemsAdd += HandleItemInsertSound;
		if (data.Definition.isFuelContainer)
		{
			data.Inventory.OnItemsAdd += HandleFuelContainerInventoryChanged;
			data.Inventory.OnItemsRemove += HandleFuelContainerInventoryChanged;
		}
		SubscribeQuestFinishIndicator();
		if (data is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.OnAnimationStateChanged += HandleAnimationStateChanged;
			zombieWgoData.CrafterOnOrderAddedEvent += DrawWidgets;
			zombieWgoData.CrafterOnOrderRemovedEvent += DrawWidgets;
			zombieWgoData.OnSetOverheadItem += SetOverheadItem;
			zombieWgoData.OnRemoveOverheadItem += RemoveOverheadItem;
		}
	}

	private void InitVisualBindings()
	{
		if (!MainWgoPart)
		{
			return;
		}
		TryBindAnimationComponent();
		dropPoint = GetComponentInChildren<DropPoint>(includeInactive: true);
		interactionItemPoint = GetComponentInChildren<InteractingItemPoint>(includeInactive: true);
		RiverBodyReceiver = GetComponentInChildren<RiverBodyReceiver>(includeInactive: true);
		RiverBodyReceiver?.Init(this);
		interactionHandler = GetNewInteractionHandler();
		if (data is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.OnSetInteractingItem += SetInteractingItem;
			zombieWgoData.OnRemoveInteractingItem += RemoveInteractingItem;
			zombieWgoData.SyncPorterBackpackLayer();
			MainWgoPart?.TryAnimatePhysicsCollider();
			if (animationComponent != null)
			{
				if (MainGame.Instance.GameSave.militaryBaseData.ContainsFighter(data))
				{
					InitZombieFighter();
				}
				if (!data.MovementComponent.IsMoving)
				{
					zombieWgoData.InvokeOnAnimationStateChanged();
				}
			}
			Item carriedPortableItem = GetCarriedPortableItem(zombieWgoData);
			if (carriedPortableItem != null && !carriedPortableItem.IsEmpty)
			{
				if (carriedPortableItem.Definition.itemSize == ItemSize.Big)
				{
					SetOverheadItem(carriedPortableItem);
				}
				else
				{
					SetInteractingItem(carriedPortableItem);
				}
			}
		}
		if (data is ConveyorWgoData conveyorWgoData && MainGame.Instance.GameSave.conveyorSystemData.conveyorComponents.Contains(conveyorWgoData.ConveyorComponent))
		{
			GetComponentInChildren<ConveyorAnimator>()?.TryRegister();
		}
		CleanupChunkableComponents();
		WgoCustomComponentSerializer.RestoreComponents(this, data);
		BindGDPointViews();
		if (needsWorkbenchExtensionInit)
		{
			needsWorkbenchExtensionInit = false;
			WGODef workbenchExtensionLogicDef = GameBalance.Me.GetWorkbenchExtensionLogicDef(data.id);
			bool flag = workbenchExtensionLogicDef != null && GameBalance.Me.workbenchesWhichUseExtensions.Contains(workbenchExtensionLogicDef);
			Debug.Log($"Try register workbench extensions for WGO {data.id}, parent? :{flag}");
			StartCoroutine(TryRegisterWorkbenchExtensionDelayed(flag));
		}
		ConditionalDrawer[] componentsInChildren = base.gameObject.GetComponentsInChildren<ConditionalDrawer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].UpdateVisualsByConditions();
		}
		DoCheckDelayedAnimation();
		visualBindingsInitialized = true;
	}

	private void TryBindAnimationComponent()
	{
		if (animationBindingsBound)
		{
			return;
		}
		animationComponent = (MainWgoPart ? MainWgoPart.AnimationComponent : null);
		if (animationComponent == null && (bool)MainWgoPart && MainWgoPart.EnsureAnimationComponentInitialized())
		{
			animationComponent = MainWgoPart.AnimationComponent;
		}
		if (!(animationComponent == null))
		{
			data.MovementComponent.SetCallbacks(delegate(float speed)
			{
				animationComponent.SetWalkAnimationSpeedMultiplier(speed);
				animationComponent.SetState(AnimationState.Walk);
			}, delegate
			{
				animationComponent.SetState(AnimationState.Idle);
			}, delegate(Vector2 dir)
			{
				animationComponent.SetDirection(dir);
			});
			if (data.MovementComponent.IsMoving)
			{
				animationComponent.SetState(AnimationState.Walk);
				animationComponent.SetDirection(data.MovableDirection);
			}
			else
			{
				animationComponent.SetDirection(data.direction.Value);
			}
			data.OnAnimationTriggerSet += HandleAnimationSetTrigger;
			data.OnAnimationLayerSet += HandleAnimationSetLayerWeight;
			data.OnAnimationStateSet += HandleAnimationSetState;
			data?.TryFireSerializedTrigger();
			animationBindingsBound = true;
		}
	}

	private void DeInitVisualBindings()
	{
		visualBindingsInitialized = false;
		animationBindingsBound = false;
		if (data is ZombieWgoData zombieWgoData)
		{
			RemoveInteractingItem();
			RemoveOverheadItem();
			zombieWgoData.OnSetInteractingItem -= SetInteractingItem;
			zombieWgoData.OnRemoveInteractingItem -= RemoveInteractingItem;
		}
		WgoCustomComponentSerializer.ResetComponents(this);
		data?.MovementComponent?.DeInit();
		data.OnAnimationTriggerSet -= HandleAnimationSetTrigger;
		data.OnAnimationLayerSet -= HandleAnimationSetLayerWeight;
		data.OnAnimationStateSet -= HandleAnimationSetState;
		animationComponent = null;
		dropPoint = null;
		interactionItemPoint = null;
		RiverBodyReceiver?.DeInit();
		RiverBodyReceiver = null;
		GetComponentInChildren<ConveyorAnimator>()?.Unregister();
	}

	private bool ShouldDelayForCraftHintCompletion()
	{
		if (hasData && data != null && data.CraftComponent != null)
		{
			return data.CraftComponent.IsPreFinishHeld;
		}
		return false;
	}

	private void HandleCraftPreFinishHoldReleased()
	{
		if (pendingDespawnAfterCraftHintCompletion)
		{
			pendingDespawnAfterCraftHintCompletion = false;
			DespawnAfterDataWasRemoved();
		}
		else if (pendingRefreshAfterCraftHintCompletion)
		{
			pendingRefreshAfterCraftHintCompletion = false;
			RefreshVisuals();
		}
	}

	private void RefreshVisuals()
	{
		if (!spawnCompleted || !this || !base.gameObject)
		{
			return;
		}
		if (isVisible && !data.IsHidden)
		{
			if (wgoPartsLoaded)
			{
				base.gameObject.SetActive(value: true);
				if (!visualBindingsInitialized)
				{
					InitVisualBindings();
				}
				else
				{
					DoCheckDelayedAnimation();
				}
				DrawWidgets();
				return;
			}
			base.gameObject.SetActive(value: false);
			RequestWgoPartsLoad(pendingApplyDefaultState, async: false);
			pendingApplyDefaultState = false;
			ValidateSpawnerComponent();
			if (wgoPartsLoaded && !visualBindingsInitialized)
			{
				InitVisualBindings();
			}
			if (wgoPartsLoaded)
			{
				boundsCalculated = false;
			}
		}
		else if (ShouldDelayForCraftHintCompletion())
		{
			pendingRefreshAfterCraftHintCompletion = true;
			WgoBubbleDisplayHandler.Hide(this);
		}
		else if (chunkVisibilityState == ChunkVisibilityState.Prewarm)
		{
			base.gameObject.SetActive(value: false);
			if (!wgoPartsLoaded && !wgoPartsLoading)
			{
				RequestWgoPartsLoad(pendingApplyDefaultState);
				pendingApplyDefaultState = false;
			}
		}
		else if (chunkVisibilityState != ChunkVisibilityState.Visible || isVisible)
		{
			LazySingleton<WgoPartLoadManager>.Instance.CancelLoad(this);
			wgoPartsLoading = false;
			base.gameObject.SetActive(value: false);
			if (wgoPartsLoaded)
			{
				WgoBubbleDisplayHandler.Hide(this);
				DeInitVisualBindings();
				ReleaseWgoPartsToPool();
				wgoPartsLoaded = false;
			}
		}
	}

	private void RequestWgoPartsLoad(bool applyDefaultWgoPartState, bool async = true)
	{
		if (!wgoPartsLoading && !wgoPartsLoaded)
		{
			wgoPartsLoading = true;
			LazySingleton<WgoPartLoadManager>.Instance.RequestLoad(this, applyDefaultWgoPartState, async);
		}
	}

	public void CompleteVisualPartsLoad(WgoPart mainPart, List<WgoPart> additionalParts, List<WgoPartData> additionalData, bool applyDefaultWgoPartState)
	{
		bool flag = isVisible && data != null && !data.IsHidden;
		bool flag2 = chunkVisibilityState == ChunkVisibilityState.Prewarm;
		if ((!flag && !flag2) || data == null)
		{
			LazySingleton<WgoPartLoadManager>.Instance.CancelLoad(this);
			wgoPartsLoading = false;
			ReleaseIncomingParts(mainPart, additionalParts);
			return;
		}
		MainWgoPart = mainPart;
		MainWgoPart.InitVisuals(null, data.MainWgoPartData, data.Definition);
		AdditionalWgoParts.Clear();
		for (int i = 0; i < additionalParts.Count; i++)
		{
			WgoPart wgoPart = additionalParts[i];
			if (!(wgoPart == null))
			{
				WgoPartData wgoPartData = additionalData[i];
				wgoPart.InitVisuals(null, wgoPartData, data.Definition);
				AdditionalWgoParts.Add(wgoPart);
			}
		}
		if (flag)
		{
			base.gameObject.SetActive(value: true);
			InitVisualBindings();
		}
		wgoPartsLoading = false;
		wgoPartsLoaded = true;
		ReinitBalanceRelatedStuff();
		UpdateWgoPartState(applyDefaultWgoPartState);
		TryBindAnimationComponent();
		DoCheckDelayedAnimation();
		ApplyZombieAnimationStateAfterWgoPartState();
		ValidateSpawnerComponent();
		boundsCalculated = false;
		if (flag)
		{
			DrawWidgets();
		}
	}

	public void HandleVisualPartsLoadFailed()
	{
		wgoPartsLoading = false;
	}

	private void ApplyZombieAnimationStateAfterWgoPartState()
	{
		if (!(data is ZombieWgoData zombieWgoData) || MainWgoPart == null)
		{
			return;
		}
		AnimationComponentBase componentInChildren = MainWgoPart.GetComponentInChildren<AnimationComponentBase>();
		if (!(componentInChildren == null))
		{
			componentInChildren.SetDirection(data.direction.Value);
			if (zombieWgoData.ZombieType == ZombieType.Porter)
			{
				float weight = ((zombieWgoData.GetGameResInt("is_staying_at_porter_station") == 1) ? 0f : 1f);
				componentInChildren.SetLayerWeight(4, weight);
			}
		}
	}

	private void ReleaseIncomingParts(WgoPart mainPart, List<WgoPart> additionalParts)
	{
		if (mainPart != null)
		{
			if (!string.IsNullOrEmpty(mainPart.PooledAddressableKey))
			{
				ReleaseWgoPart(mainPart);
			}
			else
			{
				UnityEngine.Object.Destroy(mainPart.gameObject);
			}
		}
		for (int i = 0; i < additionalParts.Count; i++)
		{
			WgoPart wgoPart = additionalParts[i];
			if (!(wgoPart == null))
			{
				if (!string.IsNullOrEmpty(wgoPart.PooledAddressableKey))
				{
					ReleaseWgoPart(wgoPart);
				}
				else
				{
					UnityEngine.Object.Destroy(wgoPart.gameObject);
				}
			}
		}
	}

	private void ReleaseWgoPartsToPool()
	{
		if (MainWgoPart != null && !string.IsNullOrEmpty(MainWgoPart.PooledAddressableKey))
		{
			if (MainWgoPart.TryGetComponent<FightingAgent>(out var component))
			{
				component.DeInitForPool();
			}
			ReleaseWgoPart(MainWgoPart);
			MainWgoPart = null;
		}
		for (int i = 0; i < AdditionalWgoParts.Count; i++)
		{
			WgoPart wgoPart = AdditionalWgoParts[i];
			if (wgoPart != null && !string.IsNullOrEmpty(wgoPart.PooledAddressableKey))
			{
				ReleaseWgoPart(wgoPart);
			}
		}
		AdditionalWgoParts.Clear();
	}

	private void CleanupChunkableComponents()
	{
		ChunkableObjectComponent[] componentsInChildren = GetComponentsInChildren<ChunkableObjectComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		BakedChunkableObjectComponent[] componentsInChildren2 = GetComponentsInChildren<BakedChunkableObjectComponent>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[j]);
		}
	}

	public void OnZombieItemsChanged(Inventory zombieInventory)
	{
		AnimationComponent.Layers layer = AnimationComponent.Layers.Armor;
		Item itemByGroupId = zombieInventory.GetItemByGroupId("weapon");
		Item itemByType = zombieInventory.GetItemByType(ItemType.BodyArmor);
		AnimationComponent animationComponent = this.animationComponent as AnimationComponent;
		if (animationComponent == null)
		{
			return;
		}
		if (itemByType.IsEmpty)
		{
			animationComponent.ChangeSkinPreset(ZombieSkinHelper.GetPresetForWgoData(data, data.Definition.zombieRollDataId));
			animationComponent.ResetArmorLayers();
			return;
		}
		SkinPresetGK2 skinPresetGK = ZombieSkinHelper.CopySkinPresetAndChangeHead(SkinPresetGK2.Load("9019_zombie_ally"), data, "zombie_worker");
		skinPresetGK.arms.colorReplaceType = ColorReplaceType.USE_PALETTE_COLOR_REPLACE;
		WgoPartData mainWgoPartData = GameScene.GetWgoViewGlobal(SGuid.Parse(data.GameResStr.Get("fighters_flag"))).Data.MainWgoPartData;
		skinPresetGK.body.palette = ZombieCustomizationConfig.GetBodyReplacePalette(mainWgoPartData.variationId).palette;
		skinPresetGK.arms.palette = ZombieCustomizationConfig.GetArmsArmorReplacePalette(itemByType.Definition.id).palette;
		animationComponent.ChangeSkinPreset(skinPresetGK);
		if (!itemByGroupId.IsEmpty)
		{
			layer = ((itemByGroupId.Definition.type == ItemType.Bow) ? AnimationComponent.Layers.ArmorWithBow : AnimationComponent.Layers.ArmorWithPike);
		}
		animationComponent.ResetArmorLayers();
		animationComponent.SetLayerWeight(layer, 1f);
		animationComponent.Animator.Update(0f);
	}

	public void InitZombieFighter()
	{
		if (!(data is ZombieWgoData zombieWgoData))
		{
			Debug.LogError("Wgo: [" + data.id + "] is not zombie fighter");
			return;
		}
		OnZombieItemsChanged(new Inventory(zombieWgoData.ZombieItem));
		zombieWgoData.OnEquipmentChanged += OnZombieItemsChanged;
	}

	private void DeInit()
	{
		if (!hasData)
		{
			return;
		}
		hasData = false;
		RemoveInteractingItem();
		RemoveOverheadItem();
		TryUnregisterInChunkManager();
		data.DeInit();
		WgoBubbleDisplayHandler.Hide(this);
		if (data.Definition != null && data.Definition.conveyorType != 0)
		{
			OnConveyorObjectRemoved();
		}
		if (wgoPartsLoaded)
		{
			DeInitVisualBindings();
			wgoPartsLoaded = false;
		}
		LazySingleton<WgoPartLoadManager>.Instance.CancelLoad(this);
		wgoPartsLoading = false;
		DestroyParts();
		data.OnPositionChanged -= HandleChangedPos;
		data.HpComponent.OnFirstDamageDealt -= HandleFirstDamageDealt;
		data.HpComponent.OnFullHpRestored -= DrawWidgets;
		data.CraftComponent.OnStatusChanged -= HandleCraftStatusChange;
		data.CraftComponent.OnPreFinishHoldReleased -= HandleCraftPreFinishHoldReleased;
		data.OnWorkerChanged -= DrawWidgets;
		data.OnAdditionalWgoPartAdd -= OnAdditionalWgoPartAdd;
		data.OnAdditionalWgoPartRemove -= OnAdditionalWgoPartRemove;
		data.OnInteractionEventChanged -= DrawWidgets;
		data.OnInteractableStateChanged -= HandleInteractedStateChanged;
		data.OnHiddenStateChanged -= HandleHiddenChanged;
		data.OnToolTickApply -= PlayHPTickAnimation;
		data.OnOccuredDeath -= PlayDestroyAnimation;
		data.Inventory.OnItemsAdd -= HandleItemInsertSound;
		if (data.Definition.isFuelContainer)
		{
			data.Inventory.OnItemsAdd -= HandleFuelContainerInventoryChanged;
			data.Inventory.OnItemsRemove -= HandleFuelContainerInventoryChanged;
		}
		UnsubscribeQuestFinishIndicator();
		if (data is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.OnAnimationStateChanged -= HandleAnimationStateChanged;
			zombieWgoData.CrafterOnOrderAddedEvent -= DrawWidgets;
			zombieWgoData.CrafterOnOrderRemovedEvent -= DrawWidgets;
			zombieWgoData.OnSetOverheadItem -= SetOverheadItem;
			zombieWgoData.OnRemoveOverheadItem -= RemoveOverheadItem;
			if (animationComponent != null)
			{
				zombieWgoData.OnEquipmentChanged -= OnZombieItemsChanged;
			}
		}
	}

	private void DoCheckDelayedAnimation()
	{
		if (base.gameObject.activeInHierarchy && animationComponent != null)
		{
			StartCoroutine(CheckDelayedAnimation());
		}
	}

	private IEnumerator CheckDelayedAnimation()
	{
		yield return null;
		if (!this || !hasData || animationComponent == null)
		{
			yield break;
		}
		bool isMoving = data.MovementComponent.IsMoving;
		if (isMoving && animationComponent.GetState() != AnimationState.Walk)
		{
			animationComponent.SetState(AnimationState.Walk);
		}
		else if (!isMoving && data is ZombieWgoData zombieWgoData)
		{
			zombieWgoData.InvokeOnAnimationStateChanged();
			ConditionalDrawer[] componentsInChildren = GetComponentsInChildren<ConditionalDrawer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].UpdateVisualsByConditions();
			}
		}
		if (data is ZombieWgoData zombie)
		{
			RestoreZombiePortableItemVisuals(zombie);
		}
		animationComponent.SetDirection(isMoving ? data.MovableDirection : data.direction.Value);
	}

	private Item GetCarriedPortableItem(ZombieWgoData zombie)
	{
		if (zombie.ZombieType != ZombieType.ConveyorTransporter)
		{
			return zombie.CaretakerPortableItem;
		}
		return zombie.ConveyorTransporterPortableItem;
	}

	private void RestoreZombiePortableItemVisuals(ZombieWgoData zombie)
	{
		zombie.SyncPorterBackpackLayer();
		Item carriedPortableItem = GetCarriedPortableItem(zombie);
		if (carriedPortableItem != null && !carriedPortableItem.IsEmpty)
		{
			if (carriedPortableItem.Definition.itemSize == ItemSize.Big)
			{
				SetOverheadItem(carriedPortableItem);
			}
			else if (interactingItem == null)
			{
				SetInteractingItem(carriedPortableItem);
			}
			else if (animationComponent != null)
			{
				animationComponent.SetLayerWeight(5, 1f);
				animationComponent.DisableDropView();
			}
		}
	}

	public void ForceDeath()
	{
		if (data.Inventory?.Data != null)
		{
			DropItemsFromInventory();
			data.Inventory.Data.RemoveAllItems();
		}
		data.ForceDeath();
	}

	private void OnEnable()
	{
		DoCheckDelayedAnimation();
	}

	private void OnDestroy()
	{
		if (isPlayingDestroyAnim)
		{
			data.TriggerCustomDeathMoment();
		}
		TryUnregisterInChunkManager();
		DeInit();
	}

	private void ReinitBalanceRelatedStuff()
	{
		interactionHandler = GetNewInteractionHandler();
		if (!(interactionHandler is ReservoirInteractionHandler))
		{
			return;
		}
		List<FishingDef> list = GameBalance.Me.fishingDefs.FindAll((FishingDef x) => x.reservoirId == data.id);
		bool flag = data.IsGameResEmpty();
		GameSave gameSave = MainGame.Instance.GameSave;
		foreach (FishingDef fishingDef in list)
		{
			if (flag)
			{
				data.AddGameRes(fishingDef.fishId, fishingDef.baseCount);
			}
			string gameLogicName = fishingDef.id + "_restore";
			if (gameSave.gameLogicSystemData.gameLogics.Exists((GameLogicData x) => x.id == gameLogicName))
			{
				continue;
			}
			float periodTime = (float)gameSave.environmentData.Day + fishingDef.regenTime / gameSave.environmentData.EnvironmentEngine.gameplayDayInMinutes;
			CustomGameLogicData customGameLogicData = new CustomGameLogicData(gameLogicName, periodTime, delegate
			{
				if (data.GetGameResInt(fishingDef.fishId) < fishingDef.baseCount)
				{
					data.AddGameRes(fishingDef.fishId, 1);
				}
			});
			customGameLogicData.Init();
			gameSave.gameLogicSystemData.gameLogics.Add(customGameLogicData);
		}
	}

	private WgoPart LoadWgoPart(WgoPartData wgoPartData, bool isMain = false)
	{
		string text;
		if (isMain)
		{
			if (data.Definition == null)
			{
				Debug.LogError("Can not load main wgo part, Definition is null [" + WGOId + "]");
				return null;
			}
			text = data.Definition.ResolveAssetId(WGOId, data);
		}
		else
		{
			text = wgoPartData.id;
		}
		string text2 = "Assets/AddressableAssets/WGOs/" + text + ".prefab";
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.LoadAssetAsync<GameObject>(text2);
		GameObject gameObject = asyncOperationHandle.WaitForCompletion();
		WgoPart wgoPart;
		if (asyncOperationHandle.Status == AsyncOperationStatus.Failed || gameObject == null)
		{
			Debug.LogError("Can't spawn wgo with id [" + text + "] at " + text2);
			GameObject obj = new GameObject();
			obj.transform.parent = base.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.name = "[empty WgoPart] " + text;
			wgoPart = obj.AddComponent<WgoPart>();
		}
		else
		{
			wgoPart = UnityEngine.Object.Instantiate(gameObject.GetComponent<WgoPart>(), base.transform);
		}
		wgoPart?.InitVisuals(gameObject, wgoPartData, data.Definition);
		if (isMain)
		{
			MainWgoPart = wgoPart;
		}
		else
		{
			AdditionalWgoParts.Add(wgoPart);
		}
		boundsCalculated = false;
		return wgoPart;
	}

	private IWGOInteractionHandler GetNewInteractionHandler()
	{
		WGODef.InteractionType interactionType = data.Definition?.interactionType ?? WGODef.InteractionType.None;
		if (data.CraftComponent.CurrentCraftElement is CraftElement { Definition: not null } craftElement && craftElement.Definition.isObjDestroyCraft)
		{
			return new CraftInteractionHandler().Init(this);
		}
		if (!data.IsInteractable)
		{
			return new DefaultInteractionHandler().Init(this);
		}
		return interactionType switch
		{
			WGODef.InteractionType.Work => new WorkInteractionHandler().Init(this), 
			WGODef.InteractionType.Craft => new CraftInteractionHandler().Init(this), 
			WGODef.InteractionType.Script => new ScriptInteractionHandler().Init(this), 
			WGODef.InteractionType.CustomInteraction => new CustomInteractionHandler().Init(this), 
			WGODef.InteractionType.Builder => new BuildInteractionHandler().Init(this), 
			WGODef.InteractionType.Chest => new ChestInteractionHandler().Init(this), 
			WGODef.InteractionType.Ladder => new LadderInteractionHandler().Init(this), 
			WGODef.InteractionType.Grave => new GraveInteractionHandler().Init(this), 
			WGODef.InteractionType.PrayerStand => new PrayerStandInteractionHandler().Init(this), 
			WGODef.InteractionType.Autopsy => new AutopsyInteractionHandler().Init(this), 
			WGODef.InteractionType.Embalm => new EmbalmInteractionHandler().Init(this), 
			WGODef.InteractionType.Garden => new GardenInteractionHandler().Init(this), 
			WGODef.InteractionType.Zombie => new ZombieInteractionHandler().Init(this), 
			WGODef.InteractionType.Survey => new SurveyInteractionHandler().Init(this), 
			WGODef.InteractionType.Alchemy => new AlchemyInteractionHandler().Init(this), 
			WGODef.InteractionType.Barricade => new BarricadeInteractionHandler().Init(this), 
			WGODef.InteractionType.Reservoir => new ReservoirInteractionHandler().Init(this), 
			WGODef.InteractionType.Flag => new FlagInteractionHandler().Init(this), 
			WGODef.InteractionType.Station => new StationInteractionHandler().Init(this), 
			WGODef.InteractionType.ConveyorCell => new ConveyorCellInteractionHandler().Init(this), 
			WGODef.InteractionType.PowerSource => new PowerSourceInteractionHandler().Init(this), 
			WGODef.InteractionType.TownBuildingPlace => new TownBuildingPlaceInteractionHandler().Init(this), 
			WGODef.InteractionType.TakeAll => new TakeAllInteractionHandler().Init(this), 
			WGODef.InteractionType.FighterContainer => new FightersContainerInteractionHandler().Init(this), 
			WGODef.InteractionType.FightBuilder => new FightBuildInteractionHandler().Init(this), 
			WGODef.InteractionType.FlagStand => new FlagStandInteractionHandler().Init(this), 
			WGODef.InteractionType.TownPalette => new TownPaletteInteractionHandler().Init(this), 
			WGODef.InteractionType.ChoirPlace => new ChoirPlaceInteractionHandler().Init(this), 
			WGODef.InteractionType.ZombieCarrier => new ZombieCarrierInteractionHandler().Init(this), 
			WGODef.InteractionType.ZombieSawmill => new ZombieSawmillInteractionHandler().Init(this), 
			WGODef.InteractionType.ZombieMine => new ZombieMineInteractionHandler().Init(this), 
			WGODef.InteractionType.TeleportMilestone => new TeleportMilestoneInteractionHandler().Init(this), 
			WGODef.InteractionType.PorterStation => new PorterStationInteractionHandler().Init(this), 
			WGODef.InteractionType.ZombieClay => new ZombieClayInteractionHandler().Init(this), 
			WGODef.InteractionType.ZombieSand => new ZombieSandInteractionHandler().Init(this), 
			WGODef.InteractionType.GardenStation => new GardenStationInteractionHandler().Init(this), 
			WGODef.InteractionType.CargoLift => new CargoLiftInteractionHandler().Init(this), 
			WGODef.InteractionType.Crematorium => new CrematoriumInteractionHandler().Init(this), 
			WGODef.InteractionType.ConveyorTransporterStation => new ConveyorTransporterStationInteractionHandler().Init(this), 
			WGODef.InteractionType.PanicReductionMachine => new PanicReductionMachineInteractionHandler().Init(this), 
			WGODef.InteractionType.ResurrectionTable => new ResurrectionInteractionHandler().Init(this), 
			WGODef.InteractionType.WellUpgrade => new WellUpgradeInteractionHandler().Init(this), 
			WGODef.InteractionType.RiverDump => new RiverDumpInteractionHandler().Init(this), 
			_ => new DefaultInteractionHandler().Init(this), 
		};
	}

	private void HandleChangedPos(Vector3 newPosition)
	{
		if (UpdatePosByData)
		{
			base.transform.position = newPosition;
		}
	}

	private Vector3 GetBubblePointPos()
	{
		if (MainWgoPart == null)
		{
			return data.BubblePos;
		}
		if (RiverBodyReceiver != null && RiverBodyReceiver.UseDynamicBubble)
		{
			if (MainGame.PlayerController == null)
			{
				return data.BubblePos;
			}
			PlayerData playerData = MainGame.PlayerController.PlayerData;
			return RiverBodyReceiver.GetDynamicBubblePos(playerData.position.Value);
		}
		if (MainWgoPart.CustomBubblePoint != null)
		{
			return MainWgoPart.CustomBubblePoint.position;
		}
		if (MainWgoPart.BubblePoint != null)
		{
			return MainWgoPart.BubblePoint.position;
		}
		return data.BubblePos;
	}

	private void LoadWgoParts()
	{
		LoadWgoPart(data.MainWgoPartData, isMain: true);
		for (int i = 0; i < data.AdditionalWgoPartsData.Count; i++)
		{
			LoadWgoPart(data.AdditionalWgoPartsData[i]);
		}
	}

	private void DestroyParts()
	{
		if (MainWgoPart != null)
		{
			if (!string.IsNullOrEmpty(MainWgoPart.PooledAddressableKey) && !Data.isTempObject)
			{
				if (MainWgoPart.TryGetComponent<FightingAgent>(out var component))
				{
					component.DeInitForPool();
				}
				ReleaseWgoPart(MainWgoPart);
			}
			else
			{
				UnityEngine.Object.Destroy(MainWgoPart.gameObject);
			}
			MainWgoPart = null;
		}
		ReleaseAdditionalWgoParts();
	}

	private void ReleaseAdditionalWgoParts()
	{
		for (int i = 0; i < AdditionalWgoParts.Count; i++)
		{
			WgoPart wgoPart = AdditionalWgoParts[i];
			if (!(wgoPart == null))
			{
				if (!string.IsNullOrEmpty(wgoPart.PooledAddressableKey) && !Data.isTempObject)
				{
					ReleaseWgoPart(wgoPart);
				}
				else
				{
					UnityEngine.Object.Destroy(wgoPart.gameObject);
				}
			}
		}
		AdditionalWgoParts.Clear();
	}

	private void ReleaseWgoPart(WgoPart part)
	{
		part.KillPhysicsCollAnimTween();
		data.OnReleaseWgoPartToPool();
		LazySingleton<WgoPartPool>.Instance.Release(part.PooledAddressableKey, part);
	}

	private void HandleFirstDamageDealt()
	{
		Debug.Log("HandleFirstDamageDealt: " + WGOId);
		if (data.HpComponent.Hp == 0)
		{
			data.HpComponent.firstDamageWasMax = true;
		}
		DrawWidgets();
	}

	private void HandleCraftStatusChange(CraftComponentStatus craftStatusEvent)
	{
		if (craftStatusEvent == CraftComponentStatus.ReadyToStartCraft || craftStatusEvent == CraftComponentStatus.Started || craftStatusEvent == CraftComponentStatus.QueueDelayed || craftStatusEvent == CraftComponentStatus.Canceled || craftStatusEvent == CraftComponentStatus.Finished || craftStatusEvent == CraftComponentStatus.ReadyToFinishAutoCraft)
		{
			DrawWidgets();
		}
	}

	private async void OnAdditionalWgoPartAdd(WgoPartData wgoPartData)
	{
		if (!wgoPartsLoaded || wgoPartsLoading)
		{
			return;
		}
		string addressableKey = "Assets/AddressableAssets/WGOs/" + wgoPartData.id + ".prefab";
		WgoPart wgoPart = await LazySingleton<WgoPartPool>.Instance.GetAsync(addressableKey, this);
		if (!wgoPartsLoaded || wgoPartsLoading || !isVisible || data.IsHidden)
		{
			if (wgoPart != null && !string.IsNullOrEmpty(wgoPart.PooledAddressableKey))
			{
				ReleaseWgoPart(wgoPart);
			}
			return;
		}
		if (wgoPart != null)
		{
			wgoPart.InitVisuals(null, wgoPartData, data.Definition);
			AdditionalWgoParts.Add(wgoPart);
			wgoPart.ApplyWgoPartState();
		}
		data.WorldZoneData?.NotifyWgoDataChanged();
	}

	private void OnAdditionalWgoPartRemove(WgoPartData wgoPartData)
	{
		if (!wgoPartsLoaded)
		{
			return;
		}
		for (int i = 0; i < AdditionalWgoParts.Count; i++)
		{
			if (AdditionalWgoParts[i].Id == wgoPartData.id)
			{
				WgoPart wgoPart = AdditionalWgoParts[i];
				if (!string.IsNullOrEmpty(wgoPart.PooledAddressableKey))
				{
					ReleaseWgoPart(wgoPart);
				}
				else
				{
					UnityEngine.Object.Destroy(wgoPart.gameObject);
				}
				AdditionalWgoParts.RemoveAt(i);
				data.WorldZoneData?.NotifyWgoDataChanged();
				break;
			}
		}
	}

	private void HandleAnimationStateChanged(AnimationState animationState, bool playSound = false)
	{
		Debug.Log($"HandleAnimationStateChanged ZombieWgoData animationState:{animationState}");
		if (!(MainWgoPart?.AnimationComponent != null))
		{
			return;
		}
		if (data.MovementComponent.IsMoving)
		{
			Debug.Log($"[Wgo] Skip zombie anim state [{animationState}] for [{data.id}]: wgo is moving");
			return;
		}
		MainWgoPart.AnimationComponent.SetState(animationState);
		if (playSound && data is ZombieWgoData { CaretakerState: ZombieWgoData.ZombieCaretakerState.PickingUpOrderItemFromInventory })
		{
			LazyAudio.PlayAtGameObject("zombie_chest_rummage", base.transform, SpatialType.sound3D);
		}
	}

	public void SetOverheadItem(Item item, bool playSound = false)
	{
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(3, 1f);
			animationComponent.SetOverheadItem(item);
			if (playSound && item.id == "wood")
			{
				LazyAudio.PlayAtGameObject("oh_wood_grab", base.transform, SpatialType.sound3D);
			}
		}
		isOverheadActive = true;
	}

	private void RemoveOverheadItem()
	{
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(3, 0f);
			animationComponent.RemoveOverheadItem();
		}
		isOverheadActive = false;
	}

	private void SetInteractingItem(Item item)
	{
		if (interactionItemPoint == null)
		{
			return;
		}
		if (isOverheadActive && animationComponent != null)
		{
			animationComponent.SetLayerWeight(3, 0f);
			animationComponent.RemoveOverheadItem();
		}
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(5, 1f);
		}
		if (item != null)
		{
			if (interactingItem != null)
			{
				interactingItem.DisableBubble();
			}
			interactingItem = UIInteractingItem.ShowInteractingItem(new Item(item.id, item.Count), interactionItemPoint.transform);
			if (animationComponent != null)
			{
				animationComponent.DisableDropView();
			}
		}
	}

	private void RemoveInteractingItem()
	{
		if (!isOverheadActive && animationComponent != null)
		{
			animationComponent.SetLayerWeight(5, 0f);
		}
		if (animationComponent != null)
		{
			animationComponent.EnableDropView();
		}
		if (interactingItem != null)
		{
			interactingItem.DisableBubble();
		}
		interactingItem = null;
	}

	private void HandleAnimationSetTrigger(string triggerName)
	{
		if (animationComponent != null)
		{
			this.UpdateFlag(ChunkingIgnoreType.Animation, newValue: true);
			animationComponent.SetTrigger(triggerName);
		}
	}

	private void HandleAnimationSetLayerWeight(int layerIndex, float weight)
	{
		if (animationComponent != null)
		{
			animationComponent.SetLayerWeight(layerIndex, weight);
		}
	}

	private void HandleAnimationSetState(AnimationState animationState)
	{
		if (animationComponent != null)
		{
			this.UpdateFlag(ChunkingIgnoreType.Animation, newValue: true);
			animationComponent.SetState(animationState);
		}
	}

	private void HandleItemInsertSound(List<Item> items)
	{
		if (data.id == "wood_container")
		{
			LazyAudio.PlayAtGameObject("oh_wood_container_drop", base.transform, SpatialType.sound3D);
		}
		else if (data.id == "grave_empty")
		{
			LazyAudio.PlayAtGameObject("oh_corpse_grave_drop", base.transform, SpatialType.sound3D);
		}
	}

	private void HandleFuelContainerInventoryChanged(List<Item> items)
	{
		DrawWidgets();
	}

	private bool TryGetFuelContainerStoredItem(out string itemId, out int count)
	{
		itemId = null;
		count = 0;
		if (data?.Definition == null || !data.Definition.isFuelContainer)
		{
			return false;
		}
		itemId = data.Definition.fuelItemId;
		if (string.IsNullOrEmpty(itemId))
		{
			itemId = GetCachedFuelContainerItemId();
		}
		if (string.IsNullOrEmpty(itemId))
		{
			return false;
		}
		count = (data.Inventory?.Data?.GetTotalCountInInventory(itemId)).GetValueOrDefault();
		return true;
	}

	private string GetCachedFuelContainerItemId()
	{
		if (fuelContainerItemIdResolved)
		{
			return cachedFuelContainerItemId;
		}
		if (data.CraftComponent?.CraftableObject == null)
		{
			return null;
		}
		if (!GameBalance.Me.craftsInCache.TryGetValue(data.CraftComponent.CraftableObject.CraftableObjectId, out var value) || value == null)
		{
			fuelContainerItemIdResolved = true;
			return null;
		}
		List<string> list = MainGame.Instance?.GameSave?.knowledgeSystem?.blackListCrafts;
		foreach (CraftDefBase item in value)
		{
			if (item != null && item.isFuelCraft && item.FuelItemDef != null && (list == null || !list.Contains(item.id)))
			{
				cachedFuelContainerItemId = item.FuelItemDef.id;
				break;
			}
		}
		fuelContainerItemIdResolved = true;
		return cachedFuelContainerItemId;
	}

	private bool ShouldShowConveyorNoPowerIcon()
	{
		if (data?.Definition == null || data.id != "conveyor_cell" || data.isTempObject)
		{
			return false;
		}
		ConveyorSystem conveyorSystem = MainGame.Instance?.conveyorSystem;
		if (conveyorSystem != null)
		{
			return !conveyorSystem.HasEnoughPower;
		}
		return false;
	}

	private bool TryGetPowerSourceGearOutput(out string iconName, out int count)
	{
		iconName = null;
		count = 0;
		if (data?.Definition == null || data.Definition.conveyorType != ConveyorElementType.PowerSource)
		{
			return false;
		}
		iconName = data.WorldZoneData?.Definition?.qualityIcon;
		if (string.IsNullOrEmpty(iconName))
		{
			iconName = "gear";
		}
		count = (int)data.Quality;
		return true;
	}

	private void HandleHiddenChanged(bool newState)
	{
		RefreshVisuals();
	}

	private void HandleInteractedStateChanged(bool isInteractable)
	{
		Wgo wgoUnderInteraction = MainGame.PlayerController.PlayerInteractionComponent.WgoUnderInteraction;
		if (wgoUnderInteraction == this && !isInteractable)
		{
			interactionHandler.OnInteractionTargetExit();
		}
		interactionHandler = GetNewInteractionHandler();
		if (wgoUnderInteraction == this && isInteractable)
		{
			interactionHandler.OnInteractionTargetEnter();
		}
		DrawWidgets();
	}

	private void TestRegisterWorkbenchExtension()
	{
		StartCoroutine(TryRegisterWorkbenchExtensionDelayed(isParentWorkbench: false));
	}

	private IEnumerator TryRegisterWorkbenchExtensionDelayed(bool isParentWorkbench)
	{
		yield return new WaitForFixedUpdate();
		if (!this || !SpecialPhysicsCastUtils.TryGetWgosIntersectedByBuffCollider(this, isParentWorkbench, out var intersectedWgos))
		{
			yield break;
		}
		if (isParentWorkbench)
		{
			foreach (Wgo item in intersectedWgos)
			{
				item.Data.AddWorkbenchParent(Data.UniqueId);
				Data.AddWorkbenchExtension(item.Data.UniqueId);
			}
		}
		else
		{
			foreach (Wgo item2 in intersectedWgos)
			{
				item2.Data.AddWorkbenchExtension(Data.UniqueId);
				Data.AddWorkbenchParent(item2.Data.UniqueId);
			}
		}
		ConditionalDrawer[] componentsInChildren = base.gameObject.GetComponentsInChildren<ConditionalDrawer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SubscribeToParentCraftStatusChanged(Data);
		}
	}

	public void SetWorldZoneWidgets([CanBeNull] WorldZoneData worldZoneData)
	{
		worldZoneWidgetsData.Clear();
		if (worldZoneData != null && !string.IsNullOrEmpty(worldZoneData.id) && worldZoneData.Definition.displayType != 0 && Data.Definition.qualityDisplayType == WGODef.QualityDisplayType.Show)
		{
			worldZoneWidgetsData.Add(new UIQualityTooltipWidgetData(worldZoneData.Definition.qualityIcon, Data));
		}
		else if (worldZoneData == null)
		{
			worldZoneWidgetsData.Clear();
		}
	}

	public void DrawWidgets()
	{
		if (isVisible)
		{
			WgoBubbleDisplayHandler.Display(this);
		}
	}

	public void DrawMultiAnswerReadyWidget(List<Item> items)
	{
		DrawMultiAnswerReadyWidget();
	}

	public void DrawMultiAnswerReadyWidget(int dayNumber)
	{
		DrawMultiAnswerReadyWidget();
	}

	private void SubscribeQuestFinishIndicator()
	{
		if (!subscribedQuestFinishIndicator && FinishPhrasesByWgoParser.HasFinishPhrases(Id))
		{
			MainGame.PlayerData.inventory.OnItemsAdd += DrawMultiAnswerReadyWidget;
			MainGame.PlayerData.inventory.OnItemsRemove += DrawMultiAnswerReadyWidget;
			MainGame.PlayerData.OnGameResChanged += DrawMultiAnswerReadyWidget;
			EnvironmentEngine.OnNewDayStarted += DrawMultiAnswerReadyWidget;
			subscribedQuestFinishIndicator = true;
		}
	}

	private void UnsubscribeQuestFinishIndicator()
	{
		if (subscribedQuestFinishIndicator)
		{
			PlayerData playerData = MainGame.PlayerData;
			if (playerData != null)
			{
				playerData.inventory.OnItemsAdd -= DrawMultiAnswerReadyWidget;
				playerData.inventory.OnItemsRemove -= DrawMultiAnswerReadyWidget;
				playerData.OnGameResChanged -= DrawMultiAnswerReadyWidget;
			}
			EnvironmentEngine.OnNewDayStarted -= DrawMultiAnswerReadyWidget;
			subscribedQuestFinishIndicator = false;
			lastReadyToFinishQuest = false;
		}
	}

	public void DrawMultiAnswerReadyWidget()
	{
		if (isVisible)
		{
			bool flag = MainGame.Instance.GameSave.questSystemData.WgoHasReadyToFinishQuest(Id);
			if (flag != lastReadyToFinishQuest)
			{
				lastReadyToFinishQuest = flag;
				WgoBubbleDisplayHandler.Display(this);
			}
		}
	}

	public void TryDrawNpcWidget()
	{
		if (!string.IsNullOrEmpty(data.Definition.repResName))
		{
			UINpcWidgetData uINpcWidgetData = new UINpcWidgetData();
			uINpcWidgetData.NpcId = data.id;
			GUIElements.Instance.NpcWidget.Draw(uINpcWidgetData);
		}
	}

	public void HideWorldZoneWidgets()
	{
		foreach (LazyWidgetDataBase worldZoneWidgetsDatum in worldZoneWidgetsData)
		{
			WgoBubbleDisplayHandler.HideWidget(this, worldZoneWidgetsDatum);
		}
	}

	public bool IsBuildRemovable()
	{
		GameBalance.Me.buildableWgos.TryGetValue(WGOId, out var value);
		GameBalance.Me.removableWgos.TryGetValue(WGOId, out var value2);
		if (data.GetGameResInt("conveyor_build_is_not_removable") > 0)
		{
			return false;
		}
		if (data.GetGameResInt("lock_building_removal") > 0)
		{
			return false;
		}
		if (value != null || value2 != null)
		{
			if (value2 != null && value != null && value2.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.Strict && !string.IsNullOrEmpty(value.customBuildAreaId) && TryGetCustomBuildAreaRect(out var buildAreaRect))
			{
				WorldZoneData worldZoneData = data.WorldZoneData ?? LazySingleton<BuildManager>.Instance.WorldZone?.Data;
				if (worldZoneData != null)
				{
					foreach (WgoData item in worldZoneData.GetWgoDataByRect(buildAreaRect))
					{
						if (item != null && !(item.UniqueId == data.UniqueId) && !value.ShouldIgnoreWgoGroupAsObstacle(item.Definition.wgoGroup) && !IsFullCoverSoftSlotExtension(item) && (GameBalance.Me.buildableWgos.ContainsKey(item.id) || GameBalance.Me.removableWgos.ContainsKey(item.id)))
						{
							return false;
						}
					}
				}
			}
			if (value == null || !value.deleteInstantly)
			{
				return value2 != null;
			}
			return true;
		}
		return false;
	}

	private static bool IsFullCoverSoftSlotExtension(WgoData wgoData)
	{
		if (GameBalance.Me.buildableWgos.TryGetValue(wgoData.id, out var value))
		{
			return value.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.FullCoverSoft;
		}
		return false;
	}

	private bool TryGetCustomBuildAreaRect(out Rect buildAreaRect)
	{
		buildAreaRect = Rect.zero;
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		foreach (Collider collider in componentsInChildren)
		{
			if (!(collider == null) && collider.gameObject.layer == 19 && collider.TryGetComponent<BuildArea>(out var component) && !string.IsNullOrEmpty(component.Id) && !component.fullCoveringMode)
			{
				Bounds bounds = collider.bounds;
				buildAreaRect = new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
				return true;
			}
		}
		return false;
	}

	public bool DoBuildRemove()
	{
		GameBalance.Me.buildableWgos.TryGetValue(WGOId, out var value);
		GameBalance.Me.removableWgos.TryGetValue(WGOId, out var value2);
		if (value != null || value2 != null)
		{
			if (value != null && value.deleteInstantly)
			{
				switch (value.buildingMode)
				{
				case BuildingDef.BuildingMode.FightingPlace:
					MainGame.Instance.GameSave.militaryBaseData.RemoveFightBuilding(data);
					MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(data);
					return true;
				case BuildingDef.BuildingMode.FightBuilding:
					MainGame.Instance.GameSave.militaryBaseData.RemoveBaseBuilding(data);
					break;
				}
				List<ItemCount> list = new List<ItemCount>();
				foreach (NeedItemData needItem in value.needItems)
				{
					list.Add(new ItemCount(needItem.id, needItem.GetCount()));
				}
				DropItemsFromBuild(list);
				if (data.Inventory.Data != null)
				{
					DropItemsFromInventory();
				}
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(data);
				return true;
			}
			if (value2 != null)
			{
				if (value2.deleteInstantly)
				{
					if (value != null)
					{
						switch (value.buildingMode)
						{
						case BuildingDef.BuildingMode.FightingPlace:
							MainGame.Instance.GameSave.militaryBaseData.RemoveFightBuilding(data);
							MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(data);
							return true;
						case BuildingDef.BuildingMode.FightBuilding:
							MainGame.Instance.GameSave.militaryBaseData.RemoveBaseBuilding(data);
							break;
						}
					}
					DropItemsFromBuild(value2.outputItems.MakePreOutput(data));
					MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(data);
					if (data.Inventory.Data != null)
					{
						DropItemsFromInventory();
					}
					return true;
				}
				CraftComponent craftComponent = data.CraftComponent;
				if (craftComponent.CurrentCraftElement != null && craftComponent.CurrentCraftElement.CraftId == value2.startCraft)
				{
					craftComponent.RemoveDestroyCraft();
					interactionHandler = GetNewInteractionHandler();
					if (craftComponent.CraftElementsQueue.Count == 0)
					{
						WgoBubbleDisplayHandler.HideWidget<UICraftHintWidgetData>(this);
					}
					return false;
				}
				CraftElement craftElement = new CraftElement(value2.startCraft, 1, new CraftParamsData(value2.startCraft, data));
				List<Item> list2 = OutputItems.MakeOutput(value2.outputItems.MakePreOutput(data));
				CraftDefBase craftDefBase = null;
				foreach (CraftDefBase availableCraft in data.CraftComponent.AvailableCrafts)
				{
					if (availableCraft.isFuelCraft)
					{
						craftDefBase = availableCraft;
						break;
					}
				}
				if (data.Inventory.Data != null)
				{
					foreach (Item item in data.Inventory.Data.Inventory)
					{
						if (!item.IsEmpty && (craftDefBase == null || !item.Definition.isFuel))
						{
							list2.Add(item);
						}
					}
				}
				if (craftDefBase != null)
				{
					NeedItemData needItemData = ((craftDefBase.needItems.Count > 0) ? craftDefBase.needItems[0] : null);
					List<ItemCount> list3 = craftDefBase.addItemsToWgoOnFinish.MakePreOutput(data);
					int num = ((list3.Count > 0) ? list3[0].count : 0);
					if (needItemData != null && num > 0 && data.Inventory.Data != null)
					{
						foreach (Item item2 in data.Inventory.Data.Inventory)
						{
							if (!item2.IsEmpty && item2.Definition.isFuel)
							{
								list2.Add(new Item(needItemData.id, Mathf.Clamp(item2.Count / num * needItemData.GetCount(), 1, 999)));
							}
						}
					}
				}
				craftElement.SetCustomOutputItems(list2);
				data.CraftComponent.AddDestroyCraft(craftElement);
				interactionHandler = GetNewInteractionHandler();
				return false;
			}
		}
		return false;
	}

	private void DropItemsFromInventory()
	{
		foreach (Item item in data.Inventory.Data.Inventory)
		{
			data.MakeDrop(item);
		}
	}

	private void DropItemsFromBuild(List<ItemCount> itemCounts)
	{
		foreach (Item item in OutputItems.MakeOutput(itemCounts))
		{
			data.MakeDrop(item);
		}
	}

	public void OnConveyorObjectRemoved()
	{
		if (!(Data is ConveyorWgoData conveyorWgoData))
		{
			return;
		}
		conveyorWgoData.ConveyorComponent.DisconnectChilds();
		BoxCollider[] componentsInChildren = GetComponentsInChildren<BoxCollider>();
		BoxCollider boxCollider = null;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject.layer == 19)
			{
				boxCollider = componentsInChildren[i];
				break;
			}
		}
		if (boxCollider == null)
		{
			return;
		}
		Collider[] array = new Collider[20];
		Vector3 halfExtents = boxCollider.bounds.extents / 2f;
		halfExtents.y = 0.2f;
		if (Physics.OverlapBoxNonAlloc(boxCollider.bounds.center, halfExtents, array, Quaternion.identity, 134217728) > 0)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != null)
				{
					array[j].GetComponent<BuildConnector>()?.TryDisconnect(conveyorWgoData);
				}
			}
		}
		for (int num = conveyorWgoData.HardConnectedWGOs.Count - 1; num >= 0; num--)
		{
			MainGame.WorldData.RemoveWgoDataFromGameScene(conveyorWgoData.HardConnectedWGOs[num]);
		}
		MainGame.Instance.conveyorSystem.RemoveConveyorObject(conveyorWgoData.ConveyorComponent);
	}

	public BurstableBounds GetChunkableData()
	{
		if (!boundsCalculated)
		{
			if (HasUsableSerializedChunkBounds())
			{
				ChunkBoundsPair serializedBounds = data.SerializedBounds;
				Vector3 position = base.transform.position;
				bounds = new ChunkBoundsPair(new Bounds(serializedBounds.withShadows.center + position, serializedBounds.withShadows.size), new Bounds(serializedBounds.withoutShadows.center + position, serializedBounds.withoutShadows.size));
			}
			else if (wgoPartsLoaded)
			{
				bounds = ChunkSizeCalculator.CalculateChunkBounds(base.gameObject);
			}
			else
			{
				if (data != null && !data.HasSerializedBounds)
				{
					Debug.LogError("[Wgo] No baked bounds for [" + data.id + "]. Run 'GK2/Bake WgoPartBakedData prefabs for all prefabs'. Using fallback bounds.");
				}
				Vector3 position2 = base.transform.position;
				Vector3 size = new Vector3(2f, 2f, 2f);
				bounds = new ChunkBoundsPair(new Bounds(position2, size), new Bounds(position2, size));
			}
			initialBoundsPosition = base.transform.position;
			boundsCalculated = true;
		}
		Vector3 vector = base.transform.position - initialBoundsPosition;
		return new BurstableBounds(bounds.GetBounds().center + vector, bounds.GetBounds().size);
	}

	private bool HasUsableSerializedChunkBounds()
	{
		if (data == null || !data.HasSerializedBounds)
		{
			return false;
		}
		ChunkBoundsPair serializedBounds = data.SerializedBounds;
		if (!(serializedBounds.withShadows.size.sqrMagnitude > 0.0001f))
		{
			return serializedBounds.withoutShadows.size.sqrMagnitude > 0.0001f;
		}
		return true;
	}

	public void UpdateChunkVisibility(bool isVisible)
	{
		if ((bool)this)
		{
			bool num = this.isVisible != isVisible;
			if (num)
			{
				this.isVisible = isVisible;
			}
			if (num || (isVisible && spawnCompleted && !base.gameObject.activeSelf && data != null && !data.IsHidden))
			{
				RefreshVisuals();
			}
		}
	}

	public void UpdateChunkVisibilityState(ChunkVisibilityState state)
	{
		if ((bool)this && chunkVisibilityState != state)
		{
			chunkVisibilityState = state;
			if (!isVisible && state != ChunkVisibilityState.Visible)
			{
				RefreshVisuals();
			}
		}
	}

	private void TryRegisterInChunkManager()
	{
		if (Application.isPlaying && !registeredInChunker)
		{
			WGODef definition = data.Definition;
			if (definition != null && definition.isMovable)
			{
				LazySingleton<ChunkManager>.Instance.RegisterDynamicChunkableObject(this, ChunkManagerLayerType.DynamicWgo);
			}
			else
			{
				LazySingleton<ChunkManager>.Instance.RegisterStaticChunkableObject(this, ChunkManagerLayerType.StaticWgo);
			}
			registeredInChunker = true;
		}
	}

	private void TryUnregisterInChunkManager()
	{
		if (Application.isPlaying && registeredInChunker)
		{
			WGODef definition = data.Definition;
			if (definition != null && definition.isMovable)
			{
				LazySingleton<ChunkManager>.Instance.UnregisterDynamicChunkableObject(this, ChunkManagerLayerType.DynamicWgo);
			}
			else
			{
				LazySingleton<ChunkManager>.Instance.UnregisterStaticChunkableObject(this, ChunkManagerLayerType.StaticWgo);
			}
			registeredInChunker = false;
		}
	}

	private void PrecomputeSerializedBounds()
	{
		if (data.HasSerializedBounds)
		{
			return;
		}
		string mainWgoPartAssetId = GetMainWgoPartAssetId();
		WgoPartBakedData wgoPartBakedData = LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.Get(mainWgoPartAssetId);
		if (wgoPartBakedData == null || !wgoPartBakedData.HasChunkBounds)
		{
			return;
		}
		Bounds withShadows = wgoPartBakedData.ChunkBounds.withShadows;
		Bounds withoutShadows = wgoPartBakedData.ChunkBounds.withoutShadows;
		foreach (WgoPartData additionalWgoPartsDatum in data.AdditionalWgoPartsData)
		{
			WgoPartBakedData wgoPartBakedData2 = LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.Get(additionalWgoPartsDatum.id);
			if (wgoPartBakedData2 != null && wgoPartBakedData2.HasChunkBounds)
			{
				withShadows.Encapsulate(wgoPartBakedData2.ChunkBounds.withShadows);
				withoutShadows.Encapsulate(wgoPartBakedData2.ChunkBounds.withoutShadows);
			}
		}
		data.SetSerializedBounds(new ChunkBoundsPair(withShadows, withoutShadows));
	}

	public string GetMainWgoPartAssetId()
	{
		if (data.Definition == null)
		{
			return data.id;
		}
		return data.Definition.ResolveAssetId(data.id, data);
	}

	public void UpdateViewAsset()
	{
		if (data?.Definition == null)
		{
			return;
		}
		string mainWgoPartAssetId = GetMainWgoPartAssetId();
		string text = data.MainWgoPartData?.id;
		if (!(mainWgoPartAssetId == text))
		{
			int rotationIndex = data.MainWgoPartData?.rotationIndex ?? (-1);
			data.ReCreateMainWgoPartData(rotationIndex);
			if (wgoPartsLoaded)
			{
				SwapMainWgoPart();
			}
			else if (wgoPartsLoading)
			{
				LazySingleton<WgoPartLoadManager>.Instance.CancelLoad(this);
				wgoPartsLoading = false;
				RequestWgoPartsLoad(pendingApplyDefaultState, async: false);
				pendingApplyDefaultState = false;
			}
		}
	}

	private void SwapMainWgoPart()
	{
		WgoBubbleDisplayHandler.Hide(this);
		DeInitVisualBindings();
		if (MainWgoPart != null)
		{
			if (!string.IsNullOrEmpty(MainWgoPart.PooledAddressableKey) && !Data.isTempObject)
			{
				if (MainWgoPart.TryGetComponent<FightingAgent>(out var component))
				{
					component.DeInitForPool();
				}
				ReleaseWgoPart(MainWgoPart);
			}
			else
			{
				UnityEngine.Object.Destroy(MainWgoPart.gameObject);
			}
			MainWgoPart = null;
		}
		string addressableKey = WgoPartPool.GetAddressableKey(data.MainWgoPartData.id);
		MainWgoPart = LazySingleton<WgoPartPool>.Instance.GetSync(addressableKey, this);
		if (MainWgoPart == null)
		{
			wgoPartsLoaded = false;
			ReleaseAdditionalWgoParts();
			HandleVisualPartsLoadFailed();
			return;
		}
		MainWgoPart.InitVisuals(null, data.MainWgoPartData, data.Definition);
		int num;
		if (isVisible)
		{
			num = ((!data.IsHidden) ? 1 : 0);
			if (num != 0)
			{
				base.gameObject.SetActive(value: true);
				InitVisualBindings();
			}
		}
		else
		{
			num = 0;
		}
		UpdateWgoPartState();
		TryBindAnimationComponent();
		DoCheckDelayedAnimation();
		ApplyZombieAnimationStateAfterWgoPartState();
		ValidateSpawnerComponent();
		data.ClearSerializedBounds();
		boundsCalculated = false;
		PrecomputeSerializedBounds();
		data.RewriteGdPointsData(new List<GDPointData>());
		RegisterGDPointsFromBakedData();
		if (num != 0)
		{
			DrawWidgets();
		}
	}

	private void RegisterGDPointsFromBakedData()
	{
		string mainWgoPartAssetId = GetMainWgoPartAssetId();
		List<WgoPartBakedData.GDPointBakedData> list = new List<WgoPartBakedData.GDPointBakedData>();
		CollectGDPointsForVariation(mainWgoPartAssetId, data.MainWgoPartData, list);
		foreach (WgoPartData additionalWgoPartsDatum in data.AdditionalWgoPartsData)
		{
			CollectGDPointsForVariation(additionalWgoPartsDatum.id, additionalWgoPartsDatum, list);
		}
		if (list.Count == 0)
		{
			GardenBedNavigation.TryRebuild(data);
			return;
		}
		Vector3 sceneOffset = MainGame.Instance.GameSave.worldData.GetGameSceneDataById(data.WorldId)?.offset ?? Vector3.zero;
		Vector3 position = data.Position;
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		int num = -1073741824 + data.UniqueId.GetHashCode();
		for (int i = 0; i < list.Count; i++)
		{
			dictionary[list[i].id] = num + i;
		}
		List<GDPointData> list2 = new List<GDPointData>();
		foreach (WgoPartBakedData.GDPointBakedData item in list)
		{
			int syntheticInstanceId = dictionary[item.id];
			GDPointData gDPointData = new GDPointData(item, data.WorldId, position, sceneOffset, syntheticInstanceId);
			List<int> list3 = new List<int>();
			foreach (string nextGdPointId in item.nextGdPointIds)
			{
				if (dictionary.TryGetValue(nextGdPointId, out var value))
				{
					list3.Add(value);
				}
			}
			gDPointData.SetNextNodeInstanceIds(list3);
			list2.Add(gDPointData);
		}
		data.RewriteGdPointsData(list2);
		GardenBedNavigation.TryRebuild(data);
	}

	private static void CollectGDPointsForVariation(string assetId, WgoPartData partData, List<WgoPartBakedData.GDPointBakedData> target)
	{
		WgoPartBakedData wgoPartBakedData = LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.Get(assetId);
		if (wgoPartBakedData != null)
		{
			int stateHash = WgoPartData.GetStateHash(partData.variationId, partData.rotationIndex);
			if (wgoPartBakedData.VariationGDPointsDict.TryGetValue(stateHash, out var value))
			{
				target.AddRange(value);
			}
		}
	}

	public void BindGDPointViews()
	{
		if (data.gdPointsData.Count == 0)
		{
			return;
		}
		GDPoint[] componentsInChildren = GetComponentsInChildren<GDPoint>(includeInactive: true);
		foreach (GDPoint gDPoint in componentsInChildren)
		{
			GDPointData gDPointData = data.GetGDPointData(gDPoint.Id);
			if (gDPointData != null)
			{
				gDPoint.Init(gDPointData);
			}
		}
	}

	public float GetCombatEntityDistance(Vector3 from, LazyConsts.Fighting.TeamType teamType)
	{
		DockPointData dockPointData = null;
		Vector3 dockPointPosition = Vector3.zero;
		switch (teamType)
		{
		case LazyConsts.Fighting.TeamType.Player:
			dockPointData = data.MainWgoPartData.GetNearestDockPoint(data, from, out dockPointPosition, DockPointData.Availability.All, DockPointData.Filter.OnlyNotZombie);
			break;
		case LazyConsts.Fighting.TeamType.WildZombie:
			dockPointData = data.MainWgoPartData.GetNearestDockPoint(data, from, out dockPointPosition, DockPointData.Availability.All, DockPointData.Filter.OnlyZombie);
			break;
		}
		if (dockPointData != null)
		{
			return (from - dockPointPosition).magnitude;
		}
		return (from - data.Position).magnitude;
	}

	public float GetCombatEntityGameRes(string resId)
	{
		if (Data == null)
		{
			return 0f;
		}
		return Data.GetGameRes(resId);
	}

	public void OnOtherCombatTargetReachedToMe(ICombatEntity other)
	{
		DockPointData dockPointData = Data.MainWgoPartData?.GetNearestDockPoint(data, other.CombatEntityPosition, DockPointData.Availability.OnlyNotOccupied);
		if (dockPointData == null)
		{
			return;
		}
		if (other is Wgo wgo)
		{
			if (!SGuid.IsNullOrEmpty(wgo.Data.takenDockPointsParentSGuid))
			{
				if (wgo.Data.takenDockPointsParentSGuid == Data.UniqueId)
				{
					return;
				}
				WgoData wgoData = MainGame.Instance?.GameSave?.WorldData?.GetWgoData(wgo.Data.takenDockPointsParentSGuid);
				wgoData?.MainWgoPartData?.TryFreeDockPoint(wgoData.UniqueId, wgo.Data.UniqueId);
				wgo.Data.takenDockPointsParentSGuid = SGuid.Empty;
			}
			wgo.Data.takenDockPointsParentSGuid = Data.UniqueId;
		}
		dockPointData.Occupy(other.CombatEntityUID);
	}

	public void OnOtherCombatTargetHitMe(AttackContext ctx)
	{
		if (!MainWgoPart || !MainWgoPart.TryGetComponent<FightingAgent>(out var component))
		{
			return;
		}
		if ((bool)component.AttackComponent && component.AttackComponent.teamType == LazyConsts.Fighting.TeamType.WildZombie && Data.HpComponent.Hp <= 0)
		{
			foreach (LazyExpression item in component.FighterDef.expressionsOnCombatDeath)
			{
				item.EvaluateWithAttackerAttacksMe(ctx.attacker);
			}
		}
		component.ProvideReturnDamageLogic(ctx);
	}

	public void ApplyKnockback(AttackContext attackContext, float duration)
	{
		if (!MainWgoPart || !MainWgoPart.TryGetComponent<FightingAgent>(out var component))
		{
			return;
		}
		if (duration.More(0f))
		{
			Debug.Log($"Apply knockback to {CombatEntityUID}, direction: {attackContext.direction:F4}, duration: {duration}");
			component.DoKnockback(attackContext.direction, duration);
		}
		AnimationComponentBase animationComponentBase = MainWgoPart?.AnimationComponent;
		if ((bool)animationComponentBase && attackContext.Damage == 0 && animationComponentBase.AnimationState != AnimationState.AttackBlock)
		{
			if (component.IsExecutingCommand && component.MobCommand is MobCommandGoTo mobCommandGoTo)
			{
				mobCommandGoTo.SetCustomAnimationState(AnimationState.AttackBlock);
			}
			else if (!component.IsExecutingCommand)
			{
				MainWgoPart?.AnimationComponent.SetTrigger(AnimationComponentBase.ATTACK_BLOCK_TRIGGER);
			}
		}
	}
}
