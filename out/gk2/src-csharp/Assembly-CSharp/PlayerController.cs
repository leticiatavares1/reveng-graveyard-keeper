using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

public class PlayerController : MonoBehaviour, IMovable, IWorker, ISoundZoneRecognizable
{
	private const int OVERLAP_COLLIDERS_MAX_COUNT = 50;

	private const string PLAYER_HP_CONSTS_ID = "player_hp";

	public Action OnWeaponEquiped;

	private static Collider[] overlapColliders = new Collider[50];

	[SerializeField]
	private PlayerInteractionComponent playerInteractionComponent;

	[SerializeField]
	private PlayerPhysicalBody playerPhysicalBody;

	[SerializeField]
	private PlayerView playerView;

	[SerializeField]
	private PlayerLocalAreaMovement playerLocalAreaMovement;

	[SerializeField]
	private PlayerColorCustomizationData characterCustomizationData;

	[SerializeField]
	private LadderClimbController ladderClimbController;

	[SerializeField]
	private float randPosRadiusForOverlapSearch = 1.4f;

	[SerializeField]
	private float positionRandomizationForOverlapSearch = 0.3f;

	[SerializeField]
	private float overlapRadiusForSearch = 0.22f;

	private MultiFlagAND<TakenControlType> playerTakenControlMultiFlag = new MultiFlagAND<TakenControlType>();

	private MultiFlagAND<DisabledStateType> playerDisabledStateMultiFlag = new MultiFlagAND<DisabledStateType>();

	private PlayerData playerData;

	private GameScene currentGameScene;

	private ToolComponent toolComponent;

	private PlayerWorkComponent playerWorkComponent;

	private PlayerFishingComponent fishingComponent;

	private AttackComponent attackComponent;

	private MovementComponent movementComponent = new MovementComponent();

	private PlayerInputHandler playerInputHandler;

	public Wgo attachedWgo;

	private SSM ssm;

	private bool isInitialized;

	private static Array controlsEnumArray;

	private static Array activeEnumArray;

	private bool isOverheadActive;

	private bool isArmorViewActive;

	private bool armorViewUsesHelmet = true;

	private PlayerActivity curWorkActivity;

	public PlayerData PlayerData => playerData;

	public bool IsControlsEnabled => playerTakenControlMultiFlag.ResultFlag;

	public bool IsControlsEnabledForInteractionHints => IsControlsEnabledExcept(TakenControlType.ByWork, TakenControlType.ByLadder, TakenControlType.ByAttack);

	public PlayerPhysicalBody PhysicalBody => playerPhysicalBody;

	public PlayerView View => playerView;

	public Transform BubblePoint => playerView.BubblePoint;

	public Transform LarryInPocketBubblePoint => playerView.LarryInPocketBubblePoint;

	public PlayerInteractionComponent PlayerInteractionComponent => playerInteractionComponent;

	public PlayerLocalAreaMovement PlayerLocalAreaMovement => playerLocalAreaMovement;

	public PlayerInputHandler PlayerInputHandler => playerInputHandler;

	public PlayerWorkComponent PlayerWorkComponent => playerWorkComponent;

	public PlayerFishingComponent FishingComponent => fishingComponent;

	public AttackComponent AttackComponent => attackComponent;

	public MovementComponent MovementComponent => movementComponent;

	public PlayerColorCustomizationData CharacterCustomizationData => characterCustomizationData;

	public SSM Ssm => ssm;

	public LadderClimbController LadderClimbController => ladderClimbController;

	public WispController WispController => playerView.WispController;

	public Item Sword => PlayerData.toolBeltInventory.Data.GetItemByType(ItemType.Sword);

	public Item Bow => PlayerData.toolBeltInventory.Data.GetItemByType(ItemType.Bow);

	[CanBeNull]
	public GameScene CurrentGameScene
	{
		get
		{
			if (currentGameScene == null)
			{
				Debug.LogWarning("[PlayerController]: no active game scene found");
				return null;
			}
			return currentGameScene;
		}
		set
		{
			TrySetCurrentGameScene(value);
		}
	}

	[CanBeNull]
	public RecastGraph SceneRecastGraph { get; private set; }

	public Vector3 MovablePosition
	{
		get
		{
			return playerData.position.Value;
		}
		set
		{
			playerPhysicalBody.MoveByPosition(value, MovableDirection);
		}
	}

	public Vector3 MovablePositionWithoutDirectionChange
	{
		get
		{
			return playerData.position.Value;
		}
		set
		{
			playerPhysicalBody.MoveByPosition(value, MovableDirection, needDirectionChange: false);
		}
	}

	public Vector2 MovableDirection
	{
		get
		{
			return playerData.Direction;
		}
		set
		{
			playerData.Direction = value;
		}
	}

	public Direction Direction => MovableDirection.ConvertFromVector2();

	public string MovableObjectId => "Player";

	public SGuid Id => playerData.Guid;

	public IWorkActivity WorkerActivity => curWorkActivity;

	public MultiInventory WorkerMultiInventory => new MultiInventory(playerData);

	public Inventory WorkerInventory => playerData.inventory;

	public Inventory WorkerToolInventory => playerData.toolBeltInventory;

	public event Action OnControlStateChanged;

	public event Action<bool> OnActiveStateChanged;

	public event Action OnDeathAnimFinished;

	public static event Action OnPlayerTeleported;

	public bool TryGetCurrentGameScene(out GameScene gameScene)
	{
		gameScene = currentGameScene;
		return gameScene != null;
	}

	public bool TrySetCurrentGameScene([CanBeNull] GameScene gameScene, string sceneId = null)
	{
		if (gameScene == null)
		{
			Debug.LogError("[PlayerController]: failed to set CurrentGameScene" + (string.IsNullOrEmpty(sceneId) ? "" : (" for scene [" + sceneId + "]")));
			return false;
		}
		currentGameScene = gameScene;
		List<int> recastGraphIndexByWorldId = GraphHelper.Instance.SceneGraphsData.GetRecastGraphIndexByWorldId(currentGameScene.Id);
		if (recastGraphIndexByWorldId.Count > 0)
		{
			SceneRecastGraph = AstarPath.active.data.graphs[recastGraphIndexByWorldId[0]] as RecastGraph;
		}
		return true;
	}

	public void ClearCurrentGameScene()
	{
		currentGameScene = null;
		SceneRecastGraph = null;
	}

	public void Initialize()
	{
		if (!isInitialized)
		{
			if (controlsEnumArray == null)
			{
				controlsEnumArray = Enum.GetValues(typeof(TakenControlType));
			}
			if (activeEnumArray == null)
			{
				activeEnumArray = Enum.GetValues(typeof(DisabledStateType));
			}
			playerTakenControlMultiFlag.Init(HandleControlChange, initialFlag: true);
			playerDisabledStateMultiFlag.Init(HandleActiveStateChangedChange, initialFlag: true);
			playerInputHandler = new PlayerInputHandler(this);
			playerPhysicalBody.Init();
			playerInteractionComponent.Init();
			toolComponent = new ToolComponent(playerView.PlayerAnimation);
			ladderClimbController.Init(playerView.PlayerAnimation);
			playerWorkComponent = GetComponent<PlayerWorkComponent>();
			attackComponent = GetComponent<AttackComponent>();
			fishingComponent = GetComponent<PlayerFishingComponent>();
			playerWorkComponent.Init(this, toolComponent, playerLocalAreaMovement);
			movementComponent.Init(this);
			playerLocalAreaMovement.Init(playerPhysicalBody);
			ssm = new SSM(new FreePlayerState(this));
			ssm.AddState(new WorkPlayerState(this));
			ssm.AddState(new LadderPlayerState(this, ladderClimbController));
			ssm.AddState(new BuildPlayerState(this));
			ssm.AddState(new PlantingPlayerState(this));
			ssm.AddState(new AttackSwordPlayerState(this));
			ssm.AddState(new AttackSwordDefaultPlayerState(this));
			ssm.AddState(new AttackSwordFocusedPlayerState(this));
			ssm.AddState(new AttackSwordContinuousPlayerState(this));
			ssm.AddState(new AttackBowFocusedPlayerState(this));
			ssm.AddState(new AttackBowDefaultPlayerState(this));
			ssm.AddState(new AttackBowAutoPlayerState(this));
			isInitialized = true;
			attackComponent.Init(PhysicalBody);
			AttackComponent obj = attackComponent;
			obj.OnWeaponChanged = (Action)Delegate.Combine(obj.OnWeaponChanged, new Action(UpdateArmorLayers));
		}
	}

	public void PreparePlayerForGame(PlayerData playerData)
	{
		SetPlayerData(playerData);
		playerData.hpComponent.SetCustomHpValue(GameBalance.Me.GetData<ConstDef>("player_hp").IntValue);
		ResetControlState();
		playerData.hpComponent.OnHpChanged += HandleHpChanged;
		HandleHpChanged(playerData.hpComponent);
	}

	public void UnPreparePlayerFromGame()
	{
		attackComponent.UnequipWeapon();
		playerView.PlayerAnimation.SetDirection(Direction.Down);
		playerView.PlayerAnimation.SetState(AnimationState.Idle);
		playerView.PlayerAnimation.Animator.Rebind();
		SetArmorView(isActive: false);
		RemoveTheFlag();
		RemoveOverheadItem();
		playerPhysicalBody.UnPrepareFromGame();
		playerData.hpComponent.OnHpChanged -= HandleHpChanged;
	}

	public void SetPlayerData(PlayerData playerData)
	{
		this.playerData = playerData;
		playerPhysicalBody.PrepareForGame(playerData);
		playerInteractionComponent.SetPlayerData(playerData);
	}

	public void SetPosition(Vector3 position, bool updateWispWgoData = true, bool instantCameraUpdate = true)
	{
		playerPhysicalBody.SetPosition(position);
		if (instantCameraUpdate && CameraSystem.Instance.ActiveCameraController.Target == playerPhysicalBody.PlayerView.transform)
		{
			CameraSystem.Instance.ActiveCameraController.UpdateTargetPosInstant();
		}
		if (playerView.ControllingWispView)
		{
			if (playerView.WispController.TargetTransform == null)
			{
				playerView.UpdateWispDirection();
			}
			playerView.WispController.TeleportToTarget(updateWispWgoData);
		}
	}

	public void SetControlTakenType(TakenControlType t, bool isEnabled)
	{
		Debug.Log($"Player SetControlTakenType:[{t}] isEnabled:[{isEnabled}]");
		playerTakenControlMultiFlag.UpdateFlag(t, isEnabled);
		if (t == TakenControlType.ByFlow || t == TakenControlType.ByBuilding)
		{
			playerPhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByFlowPlayerControl, isEnabled);
		}
		if (t != TakenControlType.BySelf && !isEnabled)
		{
			playerData.RemoveInteractingItem();
		}
		this.OnControlStateChanged?.Invoke();
	}

	public void SetDisabledStateType(DisabledStateType t, bool isEnabled)
	{
		Debug.Log($"Player SetDisabledStateType:[{t}] isEnabled:[{isEnabled}]");
		playerDisabledStateMultiFlag.UpdateFlag(t, isEnabled);
	}

	public bool IsControlsEnabledExcept(params TakenControlType[] controls)
	{
		return playerTakenControlMultiFlag.GetResultFlagExceptFlagTypesNonAlloc(controls);
	}

	public bool IsControlEnabledByType(TakenControlType t)
	{
		return playerTakenControlMultiFlag.GetFlag(t);
	}

	public void ResetControlState()
	{
		foreach (TakenControlType item in controlsEnumArray)
		{
			playerTakenControlMultiFlag.UpdateFlag(item, newValue: true);
		}
		this.OnControlStateChanged?.Invoke();
	}

	private void Update()
	{
		ssm.CustomUpdate();
		if ((bool)attachedWgo)
		{
			attachedWgo.Data.Position = playerData.position.Value;
		}
	}

	private void FixedUpdate()
	{
		ssm.CustomFixedUpdate();
	}

	private void HandleControlChange(bool isEnabled)
	{
		if (!isEnabled)
		{
			if (playerData != null && playerData.charState.Value == AnimationState.Walk)
			{
				playerPhysicalBody.StopMoving();
			}
			if (playerView.PlayerAnimation.GetState() == AnimationState.Walk)
			{
				playerView.PlayerAnimation.SetState(AnimationState.Idle);
			}
		}
		playerView.PlayerAnimation.EyesBlinkingMultiflag.UpdateFlag(PlayerAnimation.EyesBlinkingReason.ControlValue, isEnabled);
	}

	private void HandleActiveStateChangedChange(bool isEnabled)
	{
		this.OnActiveStateChanged?.Invoke(isEnabled);
		if (isEnabled)
		{
			Enable();
		}
		else
		{
			Disable();
		}
		playerView.PlayerAnimation.EyesBlinkingMultiflag.UpdateFlag(PlayerAnimation.EyesBlinkingReason.EnabledState, isEnabled);
	}

	private void Enable()
	{
		base.gameObject.SetActive(value: true);
	}

	private void Disable()
	{
		base.gameObject.SetActive(value: false);
	}

	private void HandleHpChanged(HPComponent hpComponent)
	{
		playerView.UpdateHpBarState(hpComponent);
		if (GlobalEventsSystem.Me.GetEvents(GlobalEventsSystem.Event.Type.PlayerHpValueReached, out var events))
		{
			List<string> list = null;
			foreach (KeyValuePair<string, GlobalEventsSystem.Event> item in events)
			{
				if (int.TryParse(item.Key, out var result) && result < hpComponent.prevHp && result >= hpComponent.Hp)
				{
					(list ?? (list = new List<string>())).Add(item.Key);
				}
			}
			if (list != null)
			{
				foreach (string item2 in list)
				{
					GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerHpValueReached, item2);
				}
			}
		}
		if (hpComponent.Hp == 0)
		{
			SetControlTakenType(TakenControlType.ByDeath, isEnabled: false);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerDead);
			playerView.PlayerAnimation.SetState(AnimationState.Death);
		}
	}

	public void SetOverheadItem(Item item)
	{
		if (item == null || item.IsEmpty)
		{
			RemoveOverheadItem();
			return;
		}
		SetOverheadItems(new Item[1] { item });
	}

	public void SetOverheadItems(IReadOnlyList<Item> items)
	{
		playerView.PlayerAnimation.SetLayerWeight(3, 1f);
		playerView.PlayerAnimation.SetOverheadItems(items);
		isOverheadActive = true;
	}

	public void RemoveOverheadItem()
	{
		playerView.PlayerAnimation.SetLayerWeight(3, 0f);
		playerView.PlayerAnimation.RemoveOverheadItem();
		isOverheadActive = false;
	}

	public void SetInteractingItem(Item item, int totalCount)
	{
		if (!playerData.HasMultipleOverheadItems)
		{
			if (playerData.HasOverheadItem)
			{
				playerData.DropOverheadItem();
			}
			playerView.PlayerAnimation.SetLayerWeight(5, 1f);
			if (item != null)
			{
				playerView.SetInteractingItem(item, totalCount);
			}
		}
	}

	public void RemoveInteractingItem()
	{
		if (!isOverheadActive)
		{
			playerView.PlayerAnimation.SetLayerWeight(5, 0f);
		}
		playerView.RemoveInteractingItem();
	}

	public void UpdateArmorLayers()
	{
		if (isArmorViewActive)
		{
			AnimationComponent.Layers layer = (armorViewUsesHelmet ? AnimationComponent.Layers.Armor : AnimationComponent.Layers.ArmorNoHelmet);
			if (armorViewUsesHelmet && attackComponent?.weapon != null)
			{
				layer = (attackComponent.IsRangedWeapon ? AnimationComponent.Layers.ArmorWithBow : AnimationComponent.Layers.ArmorWithSword);
			}
			View.PlayerAnimation.ResetArmorLayers();
			View.PlayerAnimation.SetLayerWeight(layer, 1f);
		}
	}

	private void TryEquipArmorFromInventory()
	{
		if (MainGame.PlayerData.toolBeltInventory.GetItemByType(ItemType.BodyArmor).IsEmpty)
		{
			Item itemByType = MainGame.PlayerData.inventory.GetItemByType(ItemType.BodyArmor);
			if (!itemByType.IsEmpty)
			{
				MainGame.PlayerData.EquipItem(itemByType);
			}
			else
			{
				Debug.LogWarning("[PlayerController]: No armor in inventory");
			}
		}
	}

	public void SetArmorView(bool isActive, int? armorIndex = null, bool withHelmet = true)
	{
		isArmorViewActive = isActive;
		if (isActive)
		{
			TryEquipArmorFromInventory();
			armorViewUsesHelmet = withHelmet;
			int index = armorIndex ?? GetEquippedArmorColorIndex();
			SkinPresetGK2 skinPreset = (withHelmet ? PlayerSkinHelper.ArmorPreset : PlayerSkinHelper.GetArmorNoHelmetPreset());
			playerView.SetPlayerPreset(skinPreset, onlyForCustomizationCharacter: false);
			PlayerSkinHelper.ApplyArmorColorsByIndex(index, withHelmet);
			UpdateArmorLayers();
		}
		else
		{
			armorViewUsesHelmet = true;
			View.SetPlayerPreset(PlayerSkinHelper.CurrentPreset, onlyForCustomizationCharacter: false);
			View.PlayerAnimation.ResetArmorLayers();
		}
		playerView.PlayerAnimation.EyesBlinkingMultiflag.UpdateFlag(PlayerAnimation.EyesBlinkingReason.ArmorEquippedEquippedState, !isActive || !withHelmet);
	}

	private int GetEquippedArmorColorIndex()
	{
		Item itemByType = PlayerData.toolBeltInventory.GetItemByType(ItemType.BodyArmor);
		if (itemByType == null || itemByType.IsEmpty)
		{
			return 0;
		}
		if (!TryParseIndexFromItemId(itemByType.id, out var index))
		{
			return 0;
		}
		return index;
	}

	private static bool TryParseIndexFromItemId(string itemId, out int index)
	{
		index = 0;
		if (string.IsNullOrEmpty(itemId))
		{
			return false;
		}
		int num = itemId.LastIndexOf('_');
		if (num < 0 || num >= itemId.Length - 1)
		{
			return false;
		}
		return int.TryParse(itemId.Substring(num + 1), out index);
	}

	public void DisableWispControl()
	{
		playerView.ControllingWispView = false;
	}

	public void EnableWispControl()
	{
		playerView.ControllingWispView = true;
	}

	public static bool Teleport(TeleportDataBase teleportData)
	{
		if (!teleportData.CanTeleport(out var error))
		{
			Debug.LogError(error);
			return false;
		}
		if (!string.IsNullOrEmpty(teleportData.soundOnTeleport))
		{
			LazyAudio.Play(teleportData.soundOnTeleport, checkDelay: false);
		}
		UIFade uiFade = LazyUI.Get<UIFade>();
		teleportData.environmentPreset = (string.IsNullOrEmpty(teleportData.environmentPreset) ? "outdoor" : teleportData.environmentPreset);
		Action<bool> tpLogic = delegate(bool dontFade)
		{
			string currentGameSceneId = MainGame.PlayerData.currentGameSceneId;
			if (teleportData.GetDestinationSceneData() != null && currentGameSceneId != teleportData.GetDestinationSceneData().id)
			{
				if (LazySingleton<GameSceneManager>.Instance.DisabledUnloadOnTeleport(currentGameSceneId))
				{
					OnSceneUnloaded();
				}
				else
				{
					LazySingleton<GameSceneManager>.Instance.UnloadScene(currentGameSceneId, delegate
					{
						OnSceneUnloaded();
					});
				}
			}
			else
			{
				SetPosAndUnFadeAsync(dontFade, keepGameFade: false).Forget();
			}
			void OnSceneUnloaded()
			{
				UILoadingOverlay uiLoadingOverlay2 = LazyUI.Get<UILoadingOverlay>();
				LoadingWindowData data = new LoadingWindowData(teleportData.GetDestinationSceneData().id, delegate
				{
					string destinationSceneId2 = teleportData.GetDestinationSceneData().id;
					LazySingleton<GameSceneManager>.Instance.LoadScene(destinationSceneId2, delegate
					{
						FinishCrossSceneTeleport(destinationSceneId2, dontFade, uiLoadingOverlay2).Forget();
					});
				}, isCrossSceneLoading: true);
				uiLoadingOverlay2.Draw(data);
			}
		};
		MainGame.PlayerController.View.Banner.SetEnabledClothFading(isEnabled: false);
		MainGame.PlayerController.SetControlTakenType(TakenControlType.ByTeleport, isEnabled: false);
		if (!teleportData.donNotFade)
		{
			uiFade.FadeIn(async delegate
			{
				SetNotDirectlyInGame(active: true);
				await Awaitable.WaitForSecondsAsync(teleportData.delayInFade);
				await Resources.UnloadUnusedAssets();
				GC.Collect();
				tpLogic(teleportData.donNotFade);
			});
		}
		else
		{
			SetNotDirectlyInGame(active: true);
			tpLogic(teleportData.donNotFade);
		}
		return true;
		async UniTaskVoid FinishCrossSceneTeleport(string destinationSceneId, bool dontFadeLocal, UILoadingOverlay uiLoadingOverlay)
		{
			GameScene gameScene = LazySingleton<GameSceneManager>.Instance.LoadedGameScenes.FirstOrDefault((GameScene x) => x.Id == destinationSceneId);
			if (!MainGame.PlayerController.TrySetCurrentGameScene(gameScene, destinationSceneId))
			{
				uiLoadingOverlay.Hide();
			}
			else
			{
				MainGame.PlayerData.currentGameSceneId = destinationSceneId;
				if (gameScene.IsInitialized)
				{
					gameScene.RestoreStaticObjectsAfterTeleport();
				}
				await SetPosAndUnFadeAsync(dontFadeLocal, keepGameFade: true);
				uiLoadingOverlay.Hide();
				teleportData.onGameSceneLoaded?.Invoke();
			}
		}
		async UniTask SetPosAndUnFadeAsync(bool dontFadeLocal, bool keepGameFade)
		{
			Action finalize = delegate
			{
				MainGame.PlayerController.View.Banner.SetEnabledClothFading(isEnabled: true);
				MainGame.PlayerController.SetControlTakenType(TakenControlType.ByTeleport, isEnabled: true);
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerTeleportAfterFadeOut, teleportData.GetDestinationId());
				if (!keepGameFade)
				{
					SetNotDirectlyInGame(active: false);
				}
			};
			MainGame.PlayerController.SetPosition(teleportData.GetPosition());
			PlayerController.OnPlayerTeleported?.Invoke();
			EnvironmentEngine.Instance.SetTimeOfDayPreset(teleportData.environmentPreset);
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.PlayerTeleport, teleportData.GetDestinationId());
			await LazySingleton<WsoConstructorPartsLoadManager>.Instance.WaitForAllRequestsAsync();
			await LazySingleton<WsoOptimizedStagesLoadManager>.Instance.WaitForAllRequestsAsync();
			await LazySingleton<WgoPartLoadManager>.Instance.WaitForAllRequestsAsync();
			if (!dontFadeLocal)
			{
				UniTaskCompletionSource fadeOutCompleted = new UniTaskCompletionSource();
				uiFade.FadeOut(delegate
				{
					finalize();
					fadeOutCompleted.TrySetResult();
				});
				await fadeOutCompleted.Task;
			}
			else
			{
				finalize();
			}
		}
	}

	private static void SetNotDirectlyInGame(bool active)
	{
		if (!(WeatherSystem.Instance == null))
		{
			WeatherSystem.Instance.AudioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.NotDirectlyInGame, active);
		}
	}

	public void TryTeleportPlayerToAnyFreePlace()
	{
		StartCoroutine(TryTeleportPlayerToAnyFreePlaceCoroutine());
	}

	private IEnumerator TryTeleportPlayerToAnyFreePlaceCoroutine()
	{
		yield return new WaitForFixedUpdate();
		if (!IsOverlappingSomething(PlayerData.position.Value, Vector3.zero, overlapRadiusForSearch))
		{
			yield break;
		}
		Vector3 vector = RandPos();
		bool flag = IsOverlappingSomething(PlayerData.position.Value + vector, Vector3.zero, overlapRadiusForSearch);
		int num = 15;
		if (flag)
		{
			while (flag && num > 0)
			{
				num--;
				vector = RandPos();
				flag = IsOverlappingSomething(PlayerData.position.Value + vector, Vector3.zero, overlapRadiusForSearch);
			}
		}
		if (!flag)
		{
			SetPosition(PlayerData.position.Value + vector, updateWispWgoData: true, instantCameraUpdate: false);
		}
		Vector3 RandPos()
		{
			return new Vector3(UnityEngine.Random.Range(0f - randPosRadiusForOverlapSearch, randPosRadiusForOverlapSearch), 0f, UnityEngine.Random.Range(0f - randPosRadiusForOverlapSearch, randPosRadiusForOverlapSearch)) * positionRandomizationForOverlapSearch;
		}
	}

	private static bool IsOverlappingSomething(Vector3 pos, Vector3 dir, float radius)
	{
		Collider[] array = Physics.OverlapSphere(pos + dir / 2f, radius, 257);
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].isTrigger)
			{
				return true;
			}
		}
		return false;
	}

	public void AttachTheFlag(Wgo flagWgo)
	{
		View.Banner.Show(isEnabled: true, flagWgo.Data.MainWgoPartData.variationId);
		attachedWgo = flagWgo;
		flagWgo.SetInteractableCollidersState(isActive: false);
		flagWgo.SetLayerToAllColliders(26);
		attachedWgo.MainWgoPart.ApplyWgoPartState("empty", 0);
		flagWgo.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
	}

	public Wgo RemoveTheFlag()
	{
		if (!attachedWgo)
		{
			View.Banner.Show(isEnabled: false);
			return null;
		}
		View.Banner.Show(isEnabled: false);
		Wgo wgo = attachedWgo;
		attachedWgo = null;
		wgo.Data.ApplyWgoPartState(View.Banner.GetCurVariationId(), 0);
		return wgo;
	}

	public void OnPathStart()
	{
		playerPhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByMovementComponent, isDynamic: false);
	}

	public void OnPathComplete(MovementComponent component)
	{
		playerPhysicalBody.SetNonKinematicFlag(PlayerDynamicType.ByMovementComponent, isDynamic: true);
		playerPhysicalBody.StopMoving();
	}

	public CraftStatus CheckWorkerDependentValues(CraftElement craftElement, float deltaTime = 1f, bool skipEnergyCheck = false, bool skipInsanityCheck = false)
	{
		CraftDef definition = craftElement.Definition;
		if (definition.insanityLock.HasExpression && !PlayerInsanityGameResSystem.GetSystem().IsEnoughValue(definition.insanityLock.EvaluateFloat()))
		{
			return CraftStatus.NotEnoughInsanity;
		}
		if (!skipInsanityCheck && definition.insanityPerTick.HasExpression && !PlayerInsanityGameResSystem.GetSystem().CanChangeInsanity(definition.insanityPerTick.EvaluateFloat() * deltaTime))
		{
			return CraftStatus.NotEnoughInsanity;
		}
		if (!craftElement.ParamsData.HasRequiredTool)
		{
			return CraftStatus.DoesntHaveRequiredTool;
		}
		if (!skipEnergyCheck && definition.energyPerTick.HasExpression && !PlayerEnergyGameResSystem.GetSystem().IsEnoughValue(definition.energyPerTick.EvaluateFloat() * deltaTime))
		{
			return CraftStatus.NotEnoughEnergy;
		}
		if (!definition.isStarCraft && !definition.isAutopsyCraft && craftElement.ParamsData.craftParamsType == CraftParamsData.CraftParamsType.Common && craftElement.ParamsData.MasteryValue < craftElement.ParamsData.MasteryLock)
		{
			return CraftStatus.NotEnoughMastery;
		}
		if ((definition.isStarCraft || definition.isAutopsyCraft) && craftElement.ParamsData.MasteryValue <= 0)
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
		playerData.AddRes(type, value);
	}

	public void MultiplyRes(string type, float value)
	{
		playerData.MultiplyRes(type, value);
	}

	public void SetRes(string stype, float value)
	{
		playerData.SetRes(stype, value);
	}

	public float GetRes(string stype, float defaultValue = 0f)
	{
		return playerData.GetRes(stype, defaultValue);
	}

	public int GetMasteryLevelForTalentBranch(string talentId, CraftDefBase craftDef = null)
	{
		int perksCraftMasteryBonusValue = GetPerksCraftMasteryBonusValue(craftDef);
		if (string.IsNullOrEmpty(talentId))
		{
			return 1 + perksCraftMasteryBonusValue;
		}
		int num = 0;
		foreach (Item item in WorkerToolInventory.Data.Inventory)
		{
			if (item.Definition.talentIds.Contains(talentId))
			{
				num += item.Definition.talentBonus;
			}
		}
		return MainGame.Instance.GameSave.talentSystemData.GetTalentBranch(talentId).curTalentValue + num + perksCraftMasteryBonusValue;
	}

	public bool HasToolForWork(WgoData wgoData, CraftDefBase craftDef)
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
		if (itemType == ItemType.None)
		{
			return true;
		}
		if (!WorkerToolInventory.Data.GetItemByType(itemType).IsEmpty)
		{
			return true;
		}
		return false;
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
			if (MainGame.Instance.GameSave.perkSystemData.HasPerk(linkedPerk))
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
			PerkData perkData = MainGame.Instance.GameSave.perkSystemData.activePerks.Find((PerkData x) => x.id == linkedPerk);
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
			PerkData perkData = MainGame.Instance.GameSave.perkSystemData.activePerks.Find((PerkData x) => x.id == linkedPerk);
			if (perkData != null)
			{
				num += perkData.Definition.craftTotalProgressTicksBonus;
			}
		}
		return num;
	}

	public float GetPerksEnergyBonusValue(CraftDefBase craftDef)
	{
		float num = 0f;
		foreach (string linkedPerk in craftDef.linkedPerks)
		{
			PerkData perkData = MainGame.Instance.GameSave.perkSystemData.activePerks.Find((PerkData x) => x.id == linkedPerk);
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
			PerkData perkData = MainGame.Instance.GameSave.perkSystemData.activePerks.Find((PerkData x) => x.id == linkedPerk);
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
		foreach (Item item in WorkerToolInventory.Data.Inventory)
		{
			if (item.Definition.type == itemType)
			{
				return item;
			}
		}
		return Item.Empty;
	}

	public void SetHPActivity(WgoData wgoData)
	{
		if (curWorkActivity != null)
		{
			curWorkActivity.WgoData.ClearWorker();
		}
		curWorkActivity = new PlayerHPActivity(playerData, wgoData);
		wgoData.TrySetWorker(this);
	}

	public void SetCraftActivity(WgoData wgoData)
	{
		if (curWorkActivity != null)
		{
			curWorkActivity.WgoData.ClearWorker();
		}
		curWorkActivity = new PlayerCraftActivity(playerData, wgoData);
		wgoData.TrySetWorker(this);
	}

	public void ClearWorkActivity()
	{
		if (curWorkActivity != null)
		{
			curWorkActivity.WgoData.ClearWorker();
			curWorkActivity = null;
		}
	}
}
