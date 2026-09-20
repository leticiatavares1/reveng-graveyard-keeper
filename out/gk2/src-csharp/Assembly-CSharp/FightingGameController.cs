using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using Pathfinding;
using Pathfinding.RVO;
using UnityEngine;

public class FightingGameController : LazySingleton<FightingGameController>
{
	private const float VICTORY_ENEMY_KILL_DURATION = 0.8f;

	private Action customFinishCallback;

	[SerializeField]
	private ZombieGroupViewController groupViewController;

	[SerializeField]
	private FightingLevelPresetProcessor presetProcessor;

	[SerializeField]
	private FightingLevel currentLevel;

	[Tooltip("A general controller for agents that don't belong to any specific line (e.g., have broken through).")]
	[SerializeField]
	private AgentsGroupBehaviourController baseDefenseAgentsController;

	[SerializeField]
	private FightEffectsManager fightEffectsManager;

	public RVOSimulator rvoSimulator;

	private Coroutine gameLoopCoroutine;

	private Coroutine spawnQueueCoroutine;

	private Coroutine victoryEnemyKillCoroutine;

	private bool isPaused;

	private readonly Queue<(string id, FightingLevelPreset.FightingLineData line)> spawnQueue = new Queue<(string, FightingLevelPreset.FightingLineData)>();

	private Collider[] overlapResults = new Collider[20];

	private FightState fightState;

	public HashSet<AgentsGroupFlagController> customFlagControllers = new HashSet<AgentsGroupFlagController>();

	[SerializeField]
	private LazyConsts.Fighting.TeamType debugDummyTeam;

	[SerializeField]
	private int debugDummyMaxHp = 25;

	[SerializeField]
	private int debugDummyStartHp = 25;

	[SerializeField]
	private int debugDummyQuality = 1;

	[SerializeField]
	private LazyConsts.Fighting.TargetAttackPriority debugDummyAttackPriority = LazyConsts.Fighting.TargetAttackPriority.High;

	[SerializeField]
	private bool debugDummyIsFightingMember = true;

	[SerializeField]
	private Vector3 debugDummySpawnOffset = new Vector3(0f, 0f, 2f);

	[SerializeField]
	private bool parentDebugDummyToLevel = true;

	[SerializeField]
	private bool selectDebugDummyOnSpawn = true;

	private string lastPlayedLevelId;

	private string lastPlayedFightPlaylist;

	private bool wasDisabledChunkerForFighting;

	private bool wasFinishedOnce;

	private bool wasLevelLoaded;

	private bool isClearingFightEnvironment;

	private readonly List<Item> pendingChainedFightRewards = new List<Item>();

	private RecastGraph recastGraph;

	private UIFightingOverlayData uiFightingOverlayData;

	private FightingLevel[] allLevels;

	private HashSet<WgoData> destroyOnStopWgos = new HashSet<WgoData>();

	private readonly List<DebugDummyAgent> debugDummyAgents = new List<DebugDummyAgent>();

	public FightingTargetsDatabase TargetsDatabase { get; } = new FightingTargetsDatabase();


	public FightingLevelPreset CurrentLevelPreset => currentLevel?.FightingLevelPreset;

	public FightState CurrentFightState => fightState;

	public AgentsGroupBehaviourController BaseDefenseAgentsController => baseDefenseAgentsController;

	public HashSet<IChunkableObject> AllStaticObjectsInZone { get; private set; }

	public HashSet<IChunkableObject> AllDynamicObjectsInZone { get; private set; }

	public string CurrentLevelId => currentLevel.id;

	public FightingLevel CurrentLevel => currentLevel;

	public HashSet<FightingLine> BreachedLines { get; } = new HashSet<FightingLine>();


	private List<ICombatEntity> CustomDecoyTargets { get; set; } = new List<ICombatEntity>();


	public HashSet<FlagStandComponent> FlagStandComponents { get; set; } = new HashSet<FlagStandComponent>();


	public bool IsPaused => isPaused;

	public FightEffectsManager FightEffectsManager => fightEffectsManager;

	public UIFightingOverlayData UIFightingOverlayData => uiFightingOverlayData;

	public bool IsClearingFightEnvironment => isClearingFightEnvironment;

	public RecastGraph RecastGraph
	{
		get
		{
			if (recastGraph == null)
			{
				recastGraph = AstarPath.active.graphs[12] as RecastGraph;
			}
			return recastGraph;
		}
	}

	public event Action<FightState> OnFightStateChanged;

	public void Play()
	{
		if (fightState == FightState.ActiveFight)
		{
			Stop();
		}
		if (!(currentLevel == null))
		{
			SetFightState(FightState.ActiveFight);
			lastPlayedLevelId = currentLevel.id;
			gameLoopCoroutine = StartCoroutine(GameLoopCoroutine());
			lastPlayedFightPlaylist = (currentLevel.IsInsideDungeon ? "sewer_fight" : "fight");
			LazyAudio.StopPlaylist("gameplay");
			LazyAudio.PlayPlaylist(lastPlayedFightPlaylist);
			Debug.Log(string.Format("{0}.Play: {1}, {2} lines.", "FightingGameController", CurrentLevelPreset.name, CurrentLevelPreset.lines.Count));
			MainGame.PlayerController.PhysicalBody.PlayerView.PlayerAnimation.SetCustomDeathAnimationFinishedCallback(null);
			wasFinishedOnce = false;
			MainGame.PlayerController.View.PlayerAnimation.SetCustomDeathAnimationFinishedCallback(OnPlayerDeathAnimationFinished);
			MainGame.PlayerController.View.UpdateStaminaBarState();
			uiFightingOverlayData = new UIFightingOverlayData(this);
			LazyUI.GetElement<UIFightingOverlay>().Draw(uiFightingOverlayData);
			currentLevel.OnPlay();
			currentLevel.ApplyStageId(4);
			LazyAudio.PlayAndForget("fight_start");
		}
	}

	public void Play(string id)
	{
		StartPreFight(id);
		Play();
	}

	public void Stop(bool hasCustomAfterFightPos = false, bool stopAsWon = false)
	{
		if (customFinishCallback != null)
		{
			customFinishCallback?.Invoke();
			customFinishCallback = null;
		}
		MainGame.PlayerController.View.PlayerAnimation.SetCustomDeathAnimationFinishedCallback(null);
		SetFightState(FightState.Disabled);
		HUD hUD = LazyUI.Get<HUD>();
		if (hUD.Mode == HUDMode.Fight)
		{
			hUD.HideFightingTimeline();
			hUD.SetMode(HUDMode.Common);
		}
		presetProcessor.StopPreset();
		presetProcessor.OnEnemiesSpawn -= HandleEnemiesSpawn;
		presetProcessor.OnPresetFinished -= OnPresetFinished;
		if (gameLoopCoroutine != null)
		{
			StopCoroutine(gameLoopCoroutine);
			gameLoopCoroutine = null;
		}
		if (spawnQueueCoroutine != null)
		{
			StopCoroutine(spawnQueueCoroutine);
			spawnQueueCoroutine = null;
		}
		if (victoryEnemyKillCoroutine != null)
		{
			StopCoroutine(victoryEnemyKillCoroutine);
			victoryEnemyKillCoroutine = null;
		}
		spawnQueue.Clear();
		if (!wasFinishedOnce)
		{
			pendingChainedFightRewards.Clear();
		}
		ClearDebugDummyAgents();
		TargetsDatabase.PruneDeadTargets(TryRemoveChunkableObjectFromIgnore);
		foreach (TargetInfo targetInfo in new List<TargetInfo>(TargetsDatabase.AllTargets))
		{
			if (targetInfo == null || targetInfo.IsPersistent)
			{
				continue;
			}
			if (!FightingTargetsDatabase.IsCombatEntityAlive(targetInfo.entity))
			{
				TargetsDatabase.RemoveEntry(targetInfo);
				continue;
			}
			if (MainGame.PlayerData.Guid.Guid != targetInfo.entity.CombatEntityUID.Guid && MainGame.Instance.GameSave.militaryBaseData.fighters.Find((MilitaryBaseData.MilitaryBaseFighter x) => x.uniqueId.Guid == targetInfo.entity.CombatEntityUID.Guid) == null)
			{
				MainGame.Instance.GameSave.worldData.RemoveWgoDataFromGameScene(targetInfo.entity.CombatEntityUID);
			}
			if (targetInfo.entity is Wgo wgo)
			{
				CleanupCustomDecoyComponents(wgo);
			}
			UnregisterTarget(targetInfo.entity);
		}
		TargetsDatabase.Clear();
		string text = currentLevel?.id;
		if ((bool)currentLevel)
		{
			currentLevel.graphWasInitialized = false;
			currentLevel.SetActive(isActive: false);
			currentLevel.SetBorderObjectsActive(isActive: false);
			currentLevel.Deactivate();
			currentLevel.SetEnabledFlagControllers(isEnabled: false);
			currentLevel.ClearFlagStands();
			foreach (AgentsGroupFlagController customFlagController in customFlagControllers)
			{
				customFlagController.DeInit();
				customFlagController.SetEnabled(isEnabled: false);
			}
			customFlagControllers.Clear();
			ClearEnvironment();
		}
		ResetNotIgnoredStateForDynamicChunkableObjects();
		DeactivateAllies();
		baseDefenseAgentsController.DeInit();
		groupViewController.DespawnAllZombies();
		currentLevel?.OnStop();
		SetPlayerDefault(hasCustomAfterFightPos, stopAsWon);
		currentLevel = null;
		ClearTemporaryWgos();
		DestroyFlagStandComponents();
		LazyAudio.StopPlaylist(lastPlayedFightPlaylist);
		LazyAudio.PlayPlaylist("gameplay");
		Debug.Log("FightingGameController.Stop");
		if (wasLevelLoaded && !string.IsNullOrEmpty(text))
		{
			if (MainGame.WorldData.TryGetGameSceneDataForContent(text, out var sceneData, out var _))
			{
				sceneData.RemoveFightingLevelData(text);
			}
			wasLevelLoaded = false;
		}
		MainGame.PlayerData.hpComponent.RestoreFullHp();
		MainGame.PlayerController.PhysicalBody.PlayerView.PlayerAnimation.SetCustomDeathAnimationFinishedCallback(null);
		MainGame.PlayerController.View.UpdateStaminaBarState();
		BuildController buildController = BuildController.Instance;
		UIBuildingWindow window = LazyUI.GetWindow<UIBuildingWindow>();
		if (buildController.IsBuildModeActive)
		{
			buildController.DisableBuildMode();
		}
		if (window.IsShown)
		{
			window.Close();
		}
		if (uiFightingOverlayData != null)
		{
			LazyUI.GetWindow<UIFightingOverlay>().Hide();
			uiFightingOverlayData = null;
		}
	}

	public void SetPauseState(bool isPaused)
	{
		this.isPaused = isPaused;
	}

	public void AddWgoAsCustomDecoy(Wgo wgo, bool setAsAgent = false, LazyConsts.Fighting.TargetAttackPriority priority = LazyConsts.Fighting.TargetAttackPriority.High)
	{
		wgo.IsActiveCombatant = true;
		TargetInfo targetInfo = TargetsDatabase.GetTargetInfo(wgo);
		DecoyComponent decoyComponent = null;
		if (targetInfo != null)
		{
			if (targetInfo.entity is UnityEngine.Object @object && @object is MonoBehaviour monoBehaviour)
			{
				decoyComponent = monoBehaviour.GetComponentInChildren<DecoyComponent>();
				if (!decoyComponent)
				{
					decoyComponent = wgo.MainWgoPart.gameObject.AddComponent<DecoyComponent>();
					decoyComponent.Initialize(wgo);
				}
			}
		}
		else
		{
			decoyComponent = wgo.MainWgoPart.GetComponentInChildren<DecoyComponent>();
			if (!decoyComponent)
			{
				decoyComponent = wgo.MainWgoPart.gameObject.AddComponent<DecoyComponent>();
				decoyComponent.Initialize(wgo);
			}
			if ((bool)wgo.MainWgoPart.GetComponentInChildren<AnimationComponentBase>() && setAsAgent)
			{
				baseDefenseAgentsController.AddWgoAsAgent(wgo);
			}
			int lineId = -1;
			int sectorId = -1;
			int num = Physics.OverlapBoxNonAlloc(decoyComponent.transform.position, Vector3.one, overlapResults);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					Collider collider = overlapResults[i];
					if (!(collider == null) && collider.TryGetComponent<FightingSector>(out var component))
					{
						lineId = component.fightingLine.lineIdx;
						sectorId = component.sectorIdx;
						break;
					}
				}
			}
			NavmeshCut navmeshCut = wgo.gameObject.AddComponent<NavmeshCut>();
			navmeshCut.type = NavmeshCut.MeshType.Capsule;
			navmeshCut.radiusExpansionMode = NavmeshCut.RadiusExpansionMode.DontExpand;
			navmeshCut.center = Vector3.zero;
			navmeshCut.circleRadius = 0.25f;
			wgo.AttackPriority = (int)priority;
			RegisterTargetNonPersistent(wgo, lineId, sectorId);
		}
		if (wgo.Data.HpComponent.MaxHpValue == 0)
		{
			wgo.Data.HpComponent.SetCustomHpValue(10);
		}
		TryForceRetargetAgentsByAddedDecoy(decoyComponent);
	}

	public void RemoveWgFromCustomDecoy(Wgo wgo)
	{
		TryForceRetargetAgentsByRemovedDecoy(wgo);
		CustomDecoyTargets.Remove(wgo);
		CleanupCustomDecoyComponents(wgo);
		UnregisterTarget(wgo);
	}

	private static void CleanupCustomDecoyComponents(Wgo wgo)
	{
		if (!wgo)
		{
			return;
		}
		if ((bool)wgo.MainWgoPart)
		{
			DecoyComponent componentInChildren = wgo.MainWgoPart.GetComponentInChildren<DecoyComponent>();
			if ((bool)componentInChildren)
			{
				UnityEngine.Object.Destroy(componentInChildren);
			}
		}
		NavmeshCut component = wgo.GetComponent<NavmeshCut>();
		if ((bool)(UnityEngine.Object)(object)component && IsRuntimeCustomDecoyNavmeshCut(component))
		{
			UnityEngine.Object.Destroy((UnityEngine.Object)(object)component);
		}
	}

	private static bool IsRuntimeCustomDecoyNavmeshCut(NavmeshCut navmeshCut)
	{
		if (navmeshCut.type == NavmeshCut.MeshType.Capsule && navmeshCut.radiusExpansionMode == NavmeshCut.RadiusExpansionMode.DontExpand)
		{
			return navmeshCut.circleRadius == 0.25f;
		}
		return false;
	}

	public void SpawnDebugDummyAgent()
	{
		if (!Application.isPlaying)
		{
			Debug.LogWarning("Cannot spawn debug dummy agent outside of play mode.", this);
			return;
		}
		if (!currentLevel)
		{
			Debug.LogWarning("Cannot spawn debug dummy agent without an active fighting level.", this);
			return;
		}
		debugDummyStartHp = Mathf.Clamp(debugDummyStartHp, 0, debugDummyMaxHp);
		debugDummyQuality = Mathf.Max(0, debugDummyQuality);
		Vector3 debugDummySpawnPosition = GetDebugDummySpawnPosition();
		DebugDummyAgent debugDummyAgent = CreateDebugDummyAgent(debugDummySpawnPosition);
		debugDummyAgent.AssignOwner(this);
		debugDummyAgent.Configure(debugDummyTeam, debugDummyMaxHp, debugDummyStartHp, debugDummyQuality, (int)debugDummyAttackPriority, debugDummyIsFightingMember);
		debugDummyAgent.EnsureInitialized();
		debugDummyAgents.Add(debugDummyAgent);
		RegisterTarget(debugDummyAgent);
	}

	public void DestroyAllDebugDummyAgents()
	{
		ClearDebugDummyAgents();
	}

	private Vector3 GetDebugDummySpawnPosition()
	{
		Vector3 vector = base.transform.position;
		if (MainGame.PlayerController != null)
		{
			vector = MainGame.PlayerController.transform.position;
		}
		else if ((bool)currentLevel)
		{
			FightingCapturePoint baseCapturePoint = currentLevel.BaseCapturePoint;
			vector = ((baseCapturePoint != null) ? baseCapturePoint.transform.position : currentLevel.transform.position);
		}
		return vector + debugDummySpawnOffset;
	}

	private DebugDummyAgent CreateDebugDummyAgent(Vector3 spawnPosition)
	{
		GameObject obj = new GameObject($"DebugDummyAgent_{debugDummyAgents.Count + 1}");
		Transform parent = base.transform;
		if (parentDebugDummyToLevel && (bool)currentLevel)
		{
			parent = currentLevel.transform;
		}
		obj.transform.SetParent(parent, worldPositionStays: false);
		obj.transform.position = spawnPosition;
		obj.transform.rotation = Quaternion.identity;
		return obj.AddComponent<DebugDummyAgent>();
	}

	private void ClearDebugDummyAgents()
	{
		if (debugDummyAgents.Count == 0)
		{
			return;
		}
		for (int num = debugDummyAgents.Count - 1; num >= 0; num--)
		{
			DebugDummyAgent debugDummyAgent = debugDummyAgents[num];
			TryUnregisterDebugDummy(debugDummyAgent);
			if ((bool)debugDummyAgent)
			{
				UnityEngine.Object.Destroy(debugDummyAgent.gameObject);
			}
		}
		debugDummyAgents.Clear();
	}

	private void TryUnregisterDebugDummy(DebugDummyAgent dummy)
	{
		if ((bool)dummy && TargetsDatabase.GetTargetInfo(dummy) != null)
		{
			TargetsDatabase.Unregister(dummy);
		}
	}

	internal void OnDebugDummyDestroyed(DebugDummyAgent dummy)
	{
		if ((bool)dummy)
		{
			debugDummyAgents.Remove(dummy);
			if (TargetsDatabase.GetTargetInfo(dummy) != null)
			{
				TargetsDatabase.Unregister(dummy);
			}
		}
	}

	public void RegisterTarget(ICombatEntity entity, int lineId = -1, int sectorId = -1, bool updateInfoIfExists = true)
	{
		TryAddChunkableObjectToIgnore(entity);
		TargetsDatabase.Register(entity, lineId, sectorId, updateInfoIfExists, isPersistent: true);
		TryActivateAddedAlly(entity);
	}

	public void RegisterCombatantTarget(Wgo wgo, bool updateInfoIfExists = true)
	{
		int lineId = -1;
		int sectorId = -1;
		if (IsTowerOrTurretCombatant(wgo))
		{
			TryResolveLineSectorAt(wgo.Data.Position, out lineId, out sectorId);
		}
		RegisterTarget(wgo, lineId, sectorId, updateInfoIfExists);
	}

	public bool TryResolveLineSectorAt(Vector3 worldPos, out int lineId, out int sectorId)
	{
		lineId = -1;
		sectorId = -1;
		if (currentLevel == null)
		{
			return false;
		}
		IReadOnlyList<FightingLine> fightingLines = currentLevel.FightingLines;
		for (int i = 0; i < fightingLines.Count; i++)
		{
			FightingLine fightingLine = fightingLines[i];
			if (fightingLine == null)
			{
				continue;
			}
			for (int j = 0; j < fightingLine.sectors.Count; j++)
			{
				FightingSector fightingSector = fightingLine.sectors[j];
				if (!(fightingSector == null))
				{
					if ((bool)fightingSector.sectorTrigger && ContainsPositionXZ(fightingSector.sectorTrigger.bounds, worldPos))
					{
						lineId = fightingLine.lineIdx;
						sectorId = fightingSector.sectorIdx;
						return true;
					}
					if ((bool)fightingSector.point && fightingSector.point.ContainsPosition(worldPos))
					{
						lineId = fightingLine.lineIdx;
						sectorId = fightingSector.sectorIdx;
						return true;
					}
				}
			}
		}
		return false;
	}

	private static bool IsTowerOrTurretCombatant(Wgo wgo)
	{
		if (wgo?.Data?.Definition == null)
		{
			return false;
		}
		string wgoGroup = wgo.Data.Definition.wgoGroup;
		if (!string.IsNullOrEmpty(wgoGroup) && wgoGroup.Contains("towers"))
		{
			return true;
		}
		AutomaticTurret component;
		if (wgo.MainWgoPart != null)
		{
			return wgo.MainWgoPart.TryGetComponent<AutomaticTurret>(out component);
		}
		return false;
	}

	private static bool ContainsPositionXZ(Bounds bounds, Vector3 worldPos)
	{
		if (worldPos.x >= bounds.min.x && worldPos.x <= bounds.max.x && worldPos.z >= bounds.min.z)
		{
			return worldPos.z <= bounds.max.z;
		}
		return false;
	}

	public void RegisterTargetNonPersistent(ICombatEntity entity, int lineId = -1, int sectorId = -1, bool updateInfoIfExists = true)
	{
		TryAddChunkableObjectToIgnore(entity);
		TargetsDatabase.Register(entity, lineId, sectorId, updateInfoIfExists);
		TryActivateAddedAlly(entity);
	}

	public void UnregisterTarget(ICombatEntity entity)
	{
		if (!FightingTargetsDatabase.IsCombatEntityAlive(entity))
		{
			TargetsDatabase.Unregister(entity);
			return;
		}
		entity.IsActiveCombatant = false;
		TargetsDatabase.Unregister(entity);
		TryRemoveChunkableObjectFromIgnore(entity);
	}

	public void UpdateTargetLocation(ICombatEntity entity, int newLineId = -1, int newSectorId = -1)
	{
		TargetInfo targetInfo = TargetsDatabase.GetTargetInfo(entity);
		if (targetInfo != null)
		{
			targetInfo.LineId = newLineId;
			targetInfo.SectorId = newSectorId;
		}
		else
		{
			Debug.LogWarning($"Trying to update location for a non-registered target: {entity}.");
		}
	}

	public void TryForceRetargetAgentsByAddedDecoy(DecoyComponent decoy)
	{
		foreach (TargetInfo allTarget in TargetsDatabase.AllTargets)
		{
			if (!((allTarget.entity.CombatEntityPosition - decoy.transform.position).XZ().magnitude > decoy.enemyRetargetRange) && allTarget.entity is UnityEngine.Object @object && @object is Wgo wgo)
			{
				FightingAgent componentInChildren = wgo.GetComponentInChildren<FightingAgent>();
				if ((bool)componentInChildren)
				{
					componentInChildren.StopCommandExecution(reportAlsoAsCompletion: true);
				}
			}
		}
	}

	public void TryForceRetargetAgentsByRemovedDecoy(ICombatEntity removedEntity)
	{
		foreach (TargetInfo allTarget in TargetsDatabase.AllTargets)
		{
			if (allTarget.entity is UnityEngine.Object @object && @object is GameObject gameObject && gameObject.TryGetComponent<FightingAgent>(out var component) && component.MobCommand?.TargetEntity == removedEntity)
			{
				component.StopCommandExecution(reportAlsoAsCompletion: true);
			}
		}
	}

	public void ActivateCustomSpawner(string spawnZoneName)
	{
		EnemySpawnZone enemySpawnZone = CurrentLevel.customSpawnZones.Find((EnemySpawnZone z) => z.name == spawnZoneName);
		if (!enemySpawnZone)
		{
			return;
		}
		foreach (EnemyData customEnemyDatum in enemySpawnZone.customEnemyData)
		{
			for (int i = 0; i < customEnemyDatum.count; i++)
			{
				List<PathfindingPenalty> penalties;
				Vector3 randomPosFromZone = enemySpawnZone.GetRandomPosFromZone(out penalties);
				WgoData wgoData = new WgoData(customEnemyDatum.id, randomPosFromZone, MainGame.PlayerData.currentGameSceneId);
				Wgo wgo = groupViewController.SpawnZombie(wgoData);
				RegisterTargetNonPersistent(wgo);
				groupViewController.Init(wgo, penalties);
				wgo.IsActiveCombatant = GameBalance.Me.fighterWgoIdsCache.Contains(wgo.Id);
				_ = wgo.IsActiveCombatant;
			}
		}
	}

	public void StartPreFight(string levelId, Action onAfterStageApplied = null)
	{
		if (fightState == FightState.InPreFight)
		{
			return;
		}
		EnsureFightingLevelLoaded(levelId);
		currentLevel = MainGame.GetFightingLevel(levelId);
		if (currentLevel == null)
		{
			Debug.LogError("Can't get fighting level with id: " + levelId);
			return;
		}
		currentLevel.ApplyStageId(3);
		onAfterStageApplied?.Invoke();
		MainGame.Instance.GameSave.environmentData.EnvironmentEngine.IsPaused = true;
		currentLevel.SetActive(isActive: true);
		CurrentLevel.SetBorderObjectsActive(isActive: true);
		if (!currentLevel.graphWasInitialized)
		{
			SetNotIgnoredStateForChunkableObjects();
			currentLevel.graphWasInitialized = true;
			currentLevel.InitGraph();
			ResetNotIgnoredStateForStaticChunkableObjects();
		}
		SetFightState(FightState.InPreFight);
		LazySingleton<FightingGameController>.Instance.CurrentLevel.SpawnAllies();
		MainGame.PlayerData.RemoveInteractingItem();
		MainGame.PlayerController.SetArmorView(isActive: true);
		bool num = MainGame.PlayerController.Sword.id != "empty";
		bool flag = MainGame.PlayerController.Bow.id != "empty";
		ItemDef itemDef = null;
		if (num)
		{
			itemDef = MainGame.PlayerController.Sword.Definition;
		}
		else if (flag)
		{
			itemDef = MainGame.PlayerController.Bow.Definition;
		}
		else
		{
			Debug.LogError("Player did not equipped sword or bow, cant set current weapon!!!");
		}
		if (itemDef != null)
		{
			MainGame.PlayerController.AttackComponent.EquipWeapon(itemDef);
		}
		else
		{
			MainGame.PlayerController.AttackComponent.EquipWeapon(ItemType.Sword);
		}
		MainGame.PlayerData.staminaSystem.SetMax();
		MainGame.PlayerController.View.UpdateStaminaBarState();
		HUD hUD = LazyUI.Get<HUD>();
		hUD.DrawFightingTimeline(new UIFightingTimelineRendererData(currentLevel.FightingLevelPreset, presetProcessor, currentLevel));
		hUD.SetMode(HUDMode.Fight);
		if (!MainGame.PlayerData.sawFightTutorialOnce && MainGame.PlayerData.GetRes("battle_tutorial_available") > 0f)
		{
			MainGame.PlayerData.sawFightTutorialOnce = true;
			UITutorialWindowData data = new UITutorialWindowData("tut_battle_2_hdr");
			LazyUI.GetWindow<UITutorialWindow>().Open(data);
		}
	}

	public void CancelPreFight()
	{
		SetFightState(FightState.Disabled);
		currentLevel.ApplyStageId(2);
		currentLevel.SetBorderObjectsActive(isActive: false);
		currentLevel.SetActive(isActive: false);
		currentLevel.graphWasInitialized = false;
		HUD hUD = LazyUI.Get<HUD>();
		if (hUD.Mode == HUDMode.Fight)
		{
			hUD.HideFightingTimeline();
			hUD.SetMode(HUDMode.Common);
		}
		foreach (AgentsGroupFlagController customFlagController in customFlagControllers)
		{
			if ((bool)customFlagController)
			{
				customFlagController.DeInit();
				customFlagController.SetEnabled(isEnabled: false);
			}
		}
		customFlagControllers.Clear();
		SetPlayerDefault();
		ClearEnvironment();
		DestroyFlagStandComponents();
		MainGame.PlayerController.View.UpdateStaminaBarState();
		ResetNotIgnoredStateForDynamicChunkableObjects();
	}

	public bool CanStartFight()
	{
		if (MainGame.PlayerData.HasMultipleOverheadItems)
		{
			return false;
		}
		Item itemByType = MainGame.PlayerData.toolBeltInventory.GetItemByType(ItemType.BodyArmor);
		Item itemByType2 = MainGame.PlayerData.toolBeltInventory.GetItemByType(ItemType.Sword);
		if (!itemByType.IsEmpty)
		{
			return !itemByType2.IsEmpty;
		}
		return false;
	}

	public void RegisterCurrentGameSceneTargets()
	{
		foreach (Wgo wgo in MainGame.PlayerController.CurrentGameScene.Wgos)
		{
			wgo.IsActiveCombatant = GameBalance.Me.fighterWgoIdsCache.Contains(wgo.Id);
			if (wgo.IsActiveCombatant && !wgo.Data.IsHidden && !MainGame.Instance.GameSave.militaryBaseData.ContainsFighter(wgo.Data, checkMercenaries: true))
			{
				RegisterCombatantTarget(wgo, updateInfoIfExists: false);
			}
		}
		foreach (AlliesSpawn alliesSpawn in CurrentLevel.AlliesSpawns)
		{
			IReadOnlyList<FightingAgent> readOnlyList = alliesSpawn?.FlagController?.AgentsController?.Agents;
			if (readOnlyList == null)
			{
				continue;
			}
			foreach (FightingAgent item in readOnlyList)
			{
				item.Wgo.IsActiveCombatant = GameBalance.Me.fighterWgoIdsCache.Contains(item.Wgo.Id);
				if (item.Wgo.IsActiveCombatant && !item.Wgo.Data.IsHidden)
				{
					RegisterTarget(item.Wgo);
				}
			}
		}
		MainGame.PlayerController.PhysicalBody.IsActiveCombatant = true;
		RegisterTarget(MainGame.PlayerController.PhysicalBody);
		Debug.Log($"SetTargetsForEnemies: AllyTargets: {TargetsDatabase.GetTargetCountByTeam(LazyConsts.Fighting.TeamType.Player)}, " + $"EnemyTargets: {TargetsDatabase.GetTargetCountByTeam(LazyConsts.Fighting.TeamType.WildZombie)}.");
	}

	public void FinishAsLost()
	{
		if (wasFinishedOnce)
		{
			return;
		}
		wasFinishedOnce = true;
		pendingChainedFightRewards.Clear();
		Debug.Log("GameIsLost");
		FightEndWindowData data = new FightEndWindowData(GameBalance.Me.GetData<FightDef>(currentLevel.id));
		LazyUI.GetWindow<FightLoseWindow>().Open(data, delegate
		{
			LazyUI.Get<UIFade>().Fade(1f, delegate
			{
				ApplyLostFightStages();
				Stop();
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightLost, lastPlayedLevelId);
			});
		});
		LazyAudio.PlayAndForget("fight_lose");
	}

	public void FinishAsWon(int customStageid = -1)
	{
		if (wasFinishedOnce)
		{
			return;
		}
		wasFinishedOnce = true;
		gameLoopCoroutine = null;
		spawnQueue.Clear();
		Debug.Log("GameIsWon");
		if (MainGame.Instance.GameSave.militaryBaseData.IsMercenaryPayed)
		{
			if (!string.IsNullOrEmpty(MainGame.Instance.GameSave.militaryBaseData.MercenariesPaymentId))
			{
				MercenariesDef data = GameBalance.Me.GetData<MercenariesDef>(MainGame.Instance.GameSave.militaryBaseData.MercenariesPaymentId);
				if (data != null)
				{
					foreach (LazyExpression item in data.afterWinExpr)
					{
						item.Evaluate();
					}
				}
			}
			MainGame.Instance.GameSave.militaryBaseData.IsMercenaryPayed = false;
		}
		List<Item> list = new List<Item>();
		foreach (NeedItemData reward in GameBalance.Me.GetData<FightDef>(currentLevel.id).rewards)
		{
			list.Add(new Item(reward.id, reward.GetCount()));
		}
		FightDef data2 = GameBalance.Me.GetData<FightDef>(currentLevel.id);
		FightEndWindowData data3 = new FightEndWindowData(data2);
		string chainedNextFightId = data2.onWinNextFightId;
		bool hasChainedFight = !string.IsNullOrEmpty(chainedNextFightId);
		AccumulateFightRewards(list);
		LazyUI.GetWindow<FightWinWindow>().Open(data3, delegate
		{
			if (hasChainedFight)
			{
				if (!TryOpenChainedPrefightWindow(chainedNextFightId, (customStageid != -1) ? customStageid : 5))
				{
					Fade();
				}
			}
			else
			{
				Fade();
			}
		});
		victoryEnemyKillCoroutine = StartCoroutine(KillRemainingEnemiesOnVictory(0.8f));
		LazyAudio.PlayAndForget("fight_win");
		void Fade(bool grantRewards = true)
		{
			LazyUI.Get<UIFade>().Fade(1f, delegate
			{
				CurrentLevel.ApplyStageId((customStageid != -1) ? customStageid : 5);
				Stop(hasCustomAfterFightPos: false, stopAsWon: true);
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightWon, lastPlayedLevelId);
			}, delegate
			{
				if (grantRewards)
				{
					GrantPendingChainedFightRewards();
				}
			});
		}
	}

	public void SetCustomFinishCallback(Action callback)
	{
		customFinishCallback = callback;
	}

	public void AddTemporaryWgoData(WgoData wgoData)
	{
		destroyOnStopWgos.Add(wgoData);
	}

	public void SetWasInPreFightState(bool wasInPreFight)
	{
		SetFightState(wasInPreFight ? FightState.InPreFight : FightState.Disabled);
	}

	public void TryCloseFightEndWindows()
	{
		FightDeadWindow window = LazyUI.GetWindow<FightDeadWindow>();
		FightLoseWindow window2 = LazyUI.GetWindow<FightLoseWindow>();
		FightWinWindow window3 = LazyUI.GetWindow<FightWinWindow>();
		if (window.IsShown)
		{
			window.CloseWithoutCallback();
		}
		if (window2.IsShown)
		{
			window2.CloseWithoutCallback();
		}
		if (window3.IsShown)
		{
			window3.CloseWithoutCallback();
		}
	}

	public void PauseAgentsMovement()
	{
		if (currentLevel == null)
		{
			return;
		}
		foreach (TargetInfo allTarget in TargetsDatabase.AllTargets)
		{
			if (allTarget.entity is UnityEngine.Object @object && @object != null && @object is Wgo wgo)
			{
				FightingAgent componentInChildren = wgo.GetComponentInChildren<FightingAgent>();
				if (componentInChildren != null)
				{
					componentInChildren.PauseMovement();
				}
			}
		}
	}

	public void UnpauseAgentsMovement()
	{
		if (currentLevel == null)
		{
			return;
		}
		foreach (TargetInfo allTarget in TargetsDatabase.AllTargets)
		{
			if (allTarget.entity is UnityEngine.Object @object && @object != null && @object is Wgo wgo)
			{
				FightingAgent componentInChildren = wgo.GetComponentInChildren<FightingAgent>();
				if (componentInChildren != null)
				{
					componentInChildren.UnpauseMovement();
				}
			}
		}
	}

	private void DoRewardForTheFight(List<Item> items)
	{
		foreach (Item item in items)
		{
			PlayerData playerData = MainGame.PlayerData;
			Vector2 direction = playerData.Direction;
			MainGame.Instance.dropSystem.DropItem(item, MainGame.PlayerData.currentGameSceneId, playerData.position.Value + new Vector3(direction.x, 0f, direction.y));
		}
	}

	private void EnsureFightingLevelLoaded(string levelId)
	{
		if (!MainGame.WorldData.TryGetGameSceneDataForContent(levelId, out var sceneData, out var config))
		{
			Debug.LogError("Can't find game scene data for fighting level: " + levelId);
		}
		else if (!config.IsSceneContentDataLoaded(levelId))
		{
			sceneData.AddFightingLevelData(levelId);
			wasLevelLoaded = true;
		}
	}

	private void AccumulateFightRewards(List<Item> rewards)
	{
		foreach (Item reward in rewards)
		{
			Item item = pendingChainedFightRewards.Find((Item i) => i.id == reward.id);
			if (item != null)
			{
				item.Count += reward.Count;
			}
			else
			{
				pendingChainedFightRewards.Add(new Item(reward.id, reward.Count));
			}
		}
	}

	private void GrantPendingChainedFightRewards()
	{
		if (pendingChainedFightRewards.Count != 0)
		{
			DoRewardForTheFight(pendingChainedFightRewards);
			pendingChainedFightRewards.Clear();
		}
	}

	private void OnPlayerDeathAnimationFinished()
	{
		if (wasFinishedOnce)
		{
			return;
		}
		wasFinishedOnce = true;
		pendingChainedFightRewards.Clear();
		FightEndWindowData data = new FightEndWindowData(GameBalance.Me.GetData<FightDef>(currentLevel.id));
		LazyUI.GetWindow<FightDeadWindow>().Open(data, delegate
		{
			LazyUI.Get<UIFade>().Fade(1f, delegate
			{
				ApplyLostFightStages();
				Stop();
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightLost, lastPlayedLevelId);
				MainGame.PlayerController.View.PlayerAnimation.SetTrigger(MainGame.PlayerController.View.PlayerAnimation.PlayerDeathExitTriggerId);
			}, delegate
			{
				MainGame.PlayerController.SetControlTakenType(TakenControlType.ByDeath, isEnabled: true);
			});
		});
		LazyAudio.PlayAndForget("fight_lose");
	}

	private void SetFightState(FightState newState)
	{
		if (fightState != newState)
		{
			fightState = newState;
			this.OnFightStateChanged?.Invoke(newState);
		}
	}

	private new void Awake()
	{
		rvoSimulator = GetComponent<RVOSimulator>();
		if ((bool)(UnityEngine.Object)(object)rvoSimulator)
		{
			rvoSimulator.desiredSimulationFPS = 30;
		}
	}

	private IEnumerator GameLoopCoroutine()
	{
		SetNotIgnoredStateForChunkableObjects();
		yield return currentLevel.Init();
		baseDefenseAgentsController.Init();
		currentLevel.InitLines();
		currentLevel.SetActive(isActive: true);
		currentLevel.SetBorderObjectsActive(isActive: true);
		currentLevel.SetEnabledFlagControllers(isEnabled: true);
		foreach (AgentsGroupFlagController customFlagController in customFlagControllers)
		{
			customFlagController.Init();
			customFlagController.SetEnabled(isEnabled: true);
		}
		RegisterCurrentGameSceneTargets();
		ActivateAllies();
		presetProcessor.OnEnemiesSpawn += HandleEnemiesSpawn;
		presetProcessor.OnPresetFinished += OnPresetFinished;
		presetProcessor.StartPreset(CurrentLevelPreset);
		yield return null;
		yield return null;
		ResetNotIgnoredStateForStaticChunkableObjects();
		spawnQueueCoroutine = StartCoroutine(ProcessSpawnQueue());
		while (presetProcessor.IsPlaying)
		{
			if (WereWeLost())
			{
				FinishAsLost();
				break;
			}
			yield return null;
		}
	}

	private void OnPresetFinished()
	{
		MainGame.Instance.GameSave.militaryBaseData.fighterContainersSelectedForFight.Clear();
		foreach (FightingLine fightingLine in currentLevel.FightingLines)
		{
			foreach (FightingSector sector in fightingLine.sectors)
			{
				if (sector.point.isBasePoint && sector.point.OwnedByTeam != 0)
				{
					FinishAsLost();
					return;
				}
			}
		}
		FinishAsWon();
		gameLoopCoroutine = StartCoroutine(CheckWinConditionAfterPreset());
	}

	private IEnumerator CheckWinConditionAfterPreset()
	{
		while (TargetsDatabase.GetTargetCountByTeam(LazyConsts.Fighting.TeamType.WildZombie) > 0)
		{
			if (WereWeLost())
			{
				FinishAsLost();
				break;
			}
			yield return null;
		}
	}

	private IEnumerator KillRemainingEnemiesOnVictory(float totalDuration)
	{
		List<FightingAgent> enemies = CollectLivingEnemyAgents();
		if (enemies.Count == 0)
		{
			victoryEnemyKillCoroutine = null;
			yield break;
		}
		foreach (FightingAgent item in enemies)
		{
			item.ClearCommand();
			item.PauseMovement();
		}
		float step = totalDuration / (float)enemies.Count;
		for (int i = 0; i < enemies.Count; i++)
		{
			FightingAgent fightingAgent = enemies[i];
			if (!(fightingAgent == null))
			{
				fightingAgent.ParentController?.RemoveAgent(fightingAgent.Wgo.Data.UniqueId);
				fightingAgent.ClearCommand();
				fightingAgent.PlayDying();
				if (i < enemies.Count - 1 && step > 0f)
				{
					yield return new WaitForSeconds(step);
				}
			}
		}
		victoryEnemyKillCoroutine = null;
	}

	private List<FightingAgent> CollectLivingEnemyAgents()
	{
		List<FightingAgent> list = new List<FightingAgent>();
		foreach (ICombatEntity item in TargetsDatabase.GetTargetsByTeam(LazyConsts.Fighting.TeamType.WildZombie))
		{
			if (item is Wgo wgo && (bool)wgo.MainWgoPart && wgo.MainWgoPart.TryGetComponent<FightingAgent>(out var component))
			{
				list.Add(component);
			}
		}
		return list;
	}

	private IEnumerator ProcessSpawnQueue()
	{
		while (true)
		{
			if (spawnQueue.Count > 0)
			{
				(string, FightingLevelPreset.FightingLineData) tuple = spawnQueue.Peek();
				int num = CurrentLevelPreset.lines.IndexOf(tuple.Item2);
				if (num < 0)
				{
					Debug.LogError("Could not find line index for the spawn request. Something is wrong.");
					spawnQueue.Dequeue();
					continue;
				}
				if (TargetsDatabase.GetEnemyCountOnLine(num) < tuple.Item2.maxSpawnedCountAtOnce)
				{
					spawnQueue.Dequeue();
					if (currentLevel.GetAvailablePositionForSpawning(num, out var position, out var penalties))
					{
						WgoData wgoData = new WgoData(tuple.Item1, position, MainGame.PlayerData.currentGameSceneId);
						Wgo wgo = groupViewController.SpawnZombie(wgoData);
						RegisterTargetNonPersistent(wgo, num);
						groupViewController.Init(wgo, penalties);
						wgo.IsActiveCombatant = GameBalance.Me.fighterWgoIdsCache.Contains(wgo.Id);
						if (!wgo.IsActiveCombatant)
						{
							continue;
						}
						currentLevel.GetFightingLine(num).AddSpawnedEnemy(wgo);
					}
				}
			}
			yield return null;
		}
	}

	private void Update()
	{
		if (!MainGame.IsGamePaused)
		{
			if (fightState != 0)
			{
				UpdateLines();
				MainGame.PlayerData.staminaSystem.UpdateStaminaLogic(Time.deltaTime);
			}
			if (uiFightingOverlayData != null)
			{
				uiFightingOverlayData.UpdateData(MainGame.PlayerController.PhysicalBody.transform.position);
			}
			if (fightState == FightState.ActiveFight)
			{
				UpdateTargets();
			}
		}
	}

	private void HandleEnemiesSpawn(string id, int count, FightingLevelPreset.FightingLineData line)
	{
		for (int i = 0; i < count; i++)
		{
			spawnQueue.Enqueue((id, line));
		}
	}

	private void ActivateAllies()
	{
		AutomaticTurret[] array = UnityEngine.Object.FindObjectsByType<AutomaticTurret>(FindObjectsSortMode.None);
		Debug.Log($"Turrets is going to be activated in count: {array.Length}");
		AutomaticTurret[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Activate();
		}
	}

	private void TryActivateAddedAlly(ICombatEntity combatEntity)
	{
		if (combatEntity is Wgo { IsActiveCombatant: not false } wgo && wgo.MainWgoPart != null && wgo.MainWgoPart.TryGetComponent<AutomaticTurret>(out var component))
		{
			component.Activate();
		}
	}

	private void DeactivateAllies()
	{
		AutomaticTurret[] array = UnityEngine.Object.FindObjectsByType<AutomaticTurret>(FindObjectsSortMode.None);
		Debug.Log($"Turrets is going to be deactivated in count: {array.Length}");
		AutomaticTurret[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].Deactivate();
		}
	}

	private void UpdateLines()
	{
		if (currentLevel == null)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		foreach (FightingLine fightingLine in currentLevel.FightingLines)
		{
			fightingLine.CustomUpdate(deltaTime);
		}
		foreach (AlliesSpawn alliesSpawn in currentLevel.AlliesSpawns)
		{
			AgentsGroupFlagController flagController = alliesSpawn.FlagController;
			if (flagController != null && !customFlagControllers.Contains(flagController))
			{
				flagController.CustomUpdate(deltaTime);
			}
		}
		foreach (AgentsGroupFlagController customFlagController in customFlagControllers)
		{
			customFlagController.CustomUpdate(deltaTime);
		}
	}

	private void UpdateTargets()
	{
		TargetsDatabase.PruneDeadTargets(TryRemoveChunkableObjectFromIgnore);
	}

	private void TryAddChunkableObjectToIgnore(ICombatEntity entity)
	{
		if (entity is IChunkableObject chunkableObject)
		{
			AllDynamicObjectsInZone.Add(chunkableObject);
			chunkableObject.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
		}
	}

	private void TryRemoveChunkableObjectFromIgnore(ICombatEntity entity)
	{
		if (entity is IChunkableObject chunkableObject)
		{
			AllDynamicObjectsInZone.Remove(chunkableObject);
			chunkableObject.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: false);
		}
	}

	private void SetNotIgnoredStateForChunkableObjects()
	{
		if (wasDisabledChunkerForFighting)
		{
			return;
		}
		wasDisabledChunkerForFighting = true;
		AllStaticObjectsInZone = new HashSet<IChunkableObject>();
		AllDynamicObjectsInZone = new HashSet<IChunkableObject>();
		foreach (BoxCollider zoneDefineCollider in currentLevel.ZoneDefineColliders)
		{
			HashSet<IChunkableObject> allChunkableObjectsInBoundsForSelectedLayers = LazySingleton<ChunkManager>.Instance.GetAllChunkableObjectsInBoundsForSelectedLayers(new List<ChunkManagerLayerType>
			{
				ChunkManagerLayerType.DynamicWgo,
				ChunkManagerLayerType.StaticWgo
			}, zoneDefineCollider.bounds);
			foreach (IChunkableObject allChunkableObjectsInBoundsForSelectedLayer in LazySingleton<ChunkManager>.Instance.GetAllChunkableObjectsInBoundsForSelectedLayers((from ChunkManagerLayerType layerType in Enum.GetValues(typeof(ChunkManagerLayerType))
				where layerType != ChunkManagerLayerType.DynamicWgo && layerType != ChunkManagerLayerType.StaticWgo
				select layerType).ToList(), zoneDefineCollider.bounds))
			{
				if (IsChunkableObjectAlive(allChunkableObjectsInBoundsForSelectedLayer) && AllStaticObjectsInZone.Add(allChunkableObjectsInBoundsForSelectedLayer))
				{
					allChunkableObjectsInBoundsForSelectedLayer.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
				}
			}
			foreach (IChunkableObject item in allChunkableObjectsInBoundsForSelectedLayers)
			{
				if (IsChunkableObjectAlive(item) && AllDynamicObjectsInZone.Add(item))
				{
					item.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: true);
				}
			}
		}
	}

	private void ResetNotIgnoredStateForStaticChunkableObjects()
	{
		if (AllStaticObjectsInZone == null)
		{
			return;
		}
		foreach (IChunkableObject item in AllStaticObjectsInZone)
		{
			if (IsChunkableObjectAlive(item))
			{
				item.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: false);
			}
		}
		AllStaticObjectsInZone.Clear();
		wasDisabledChunkerForFighting = false;
	}

	private void ResetNotIgnoredStateForDynamicChunkableObjects()
	{
		if (AllDynamicObjectsInZone == null)
		{
			return;
		}
		foreach (IChunkableObject item in AllDynamicObjectsInZone)
		{
			if (IsChunkableObjectAlive(item))
			{
				item.UpdateFlag(ChunkingIgnoreType.Fighting, newValue: false);
			}
		}
		AllDynamicObjectsInZone.Clear();
	}

	private static bool IsChunkableObjectAlive(IChunkableObject chunkableObject)
	{
		if (chunkableObject != null)
		{
			if (chunkableObject is UnityEngine.Object @object)
			{
				return @object != null;
			}
			return true;
		}
		return false;
	}

	private bool WereWeLost()
	{
		return CurrentLevel.BaseCapturePoint.OwnedByTeam == LazyConsts.Fighting.TeamType.WildZombie;
	}

	private void ClearEnvironment()
	{
		isClearingFightEnvironment = true;
		try
		{
			PrepareFightBuildingsForTeardown();
			MainGame.Instance.GameSave.militaryBaseData.ReturnFightBuildingsToBase();
			LazySingleton<FightingGameController>.Instance.CurrentLevel.DeSpawnAllies();
			MainGame.Instance.GameSave.environmentData.EnvironmentEngine.IsPaused = false;
			Wgo wgo = MainGame.PlayerController.RemoveTheFlag();
			if ((bool)wgo)
			{
				wgo.Data.Position = MainGame.PlayerData.position.Value;
				AgentsGroupFlagController.SetInteractionLocked(wgo, isLocked: false);
			}
		}
		finally
		{
			isClearingFightEnvironment = false;
		}
	}

	private void PrepareFightBuildingsForTeardown()
	{
		foreach (MilitaryBaseData.MilitaryBaseFightBuilding fightBuilding in MainGame.Instance.GameSave.militaryBaseData.fightBuildings)
		{
			Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(fightBuilding.uniqueId);
			if (!wgoViewGlobal)
			{
				continue;
			}
			FlagPlacementPoint[] componentsInChildren = wgoViewGlobal.GetComponentsInChildren<FlagPlacementPoint>(includeInactive: true);
			foreach (FlagPlacementPoint obj in componentsInChildren)
			{
				Wgo flagWgo = obj.FlagWgo;
				obj.FlagWgo = null;
				obj.DeInit();
				if ((bool)flagWgo)
				{
					MainGame.WorldData.RemoveWgoDataFromGameScene(flagWgo.Data);
				}
			}
		}
	}

	private void SetPlayerDefault(bool hasCustomAfterFightPos = false, bool stopAsWon = false)
	{
		PlayerController playerController = MainGame.PlayerController;
		playerController.AttackComponent.UnequipWeapon();
		playerController.SetArmorView(isActive: false);
		if (!hasCustomAfterFightPos)
		{
			string text = (stopAsWon ? currentLevel.FightbackAfterWinGdPointId : currentLevel.FightbackGdPointId);
			if (string.IsNullOrEmpty(text))
			{
				text = "RT_fightback_player_spawn";
			}
			PlayerController.Teleport(new GDPointTeleportData(text, isTag: false, "outdoor", "", null, donNotFade: true));
		}
		if (!playerController.IsControlEnabledByType(TakenControlType.ByDeath))
		{
			playerController.SetControlTakenType(TakenControlType.ByDeath, isEnabled: true);
			playerController.View.PlayerAnimation.SetState(AnimationState.Idle);
		}
	}

	private void ClearTemporaryWgos()
	{
		foreach (WgoData destroyOnStopWgo in destroyOnStopWgos)
		{
			if (destroyOnStopWgo != null)
			{
				MainGame.WorldData.RemoveWgoDataFromGameScene(destroyOnStopWgo);
			}
		}
		destroyOnStopWgos.Clear();
	}

	private void DestroyFlagStandComponents()
	{
		foreach (FlagStandComponent flagStandComponent in FlagStandComponents)
		{
			flagStandComponent?.DestroyStandWgo();
		}
		FlagStandComponents.Clear();
	}

	private void ApplyLostFightStages()
	{
		CurrentLevel.ApplyStageId(2);
		RepairChainedFightsLeftAtWinAfterFollowUpReset();
	}

	public static void RepairChainedFightsLeftAtWinAfterFollowUpReset(WorldData worldData = null)
	{
		if (worldData == null)
		{
			worldData = MainGame.WorldData;
		}
		List<FightDef> list = GameBalance.Me?.fightDefinitions;
		if (worldData?.gameSceneDataList == null || list == null)
		{
			return;
		}
		for (int i = 0; i < list.Count; i++)
		{
			FightDef fightDef = list[i];
			if (fightDef != null && !string.IsNullOrEmpty(fightDef.onWinNextFightId) && TryGetFightingLevelData(worldData, fightDef.id, out var data) && TryGetFightingLevelData(worldData, fightDef.onWinNextFightId, out var data2) && (data.CurStageId == 5 || data.CurStageId == 6) && data2.CurStageId == 2)
			{
				ApplyFightingLevelStage(worldData, fightDef.id, 2);
			}
		}
	}

	private static bool TryGetFightingLevelData(WorldData worldData, string fightId, out FightingLevelData data)
	{
		data = null;
		if (worldData == null || string.IsNullOrEmpty(fightId))
		{
			return false;
		}
		if (MainGame.Instance != null && worldData.TryGetGameSceneDataForContent(fightId, out var sceneData, out var _) && sceneData?.fightingLevels != null)
		{
			data = sceneData.fightingLevels.Find((FightingLevelData level) => level.id == fightId);
			if (data != null)
			{
				return true;
			}
		}
		List<GameSceneData> gameSceneDataList = worldData.gameSceneDataList;
		for (int i = 0; i < gameSceneDataList.Count; i++)
		{
			GameSceneData gameSceneData = gameSceneDataList[i];
			if (gameSceneData?.fightingLevels != null)
			{
				data = gameSceneData.fightingLevels.Find((FightingLevelData level) => level.id == fightId);
				if (data != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void ApplyFightingLevelStage(WorldData worldData, string fightId, int stageId)
	{
		if (MainGame.Instance != null)
		{
			FightingLevel fightingLevel = MainGame.GetFightingLevel(fightId);
			if (fightingLevel != null)
			{
				fightingLevel.ApplyStageId(stageId);
				return;
			}
		}
		if (TryGetFightingLevelData(worldData, fightId, out var data))
		{
			data.CurStageId = stageId;
		}
	}

	private bool TryOpenChainedPrefightWindow(string nextFightId, int customStageid)
	{
		EnsureFightingLevelLoaded(nextFightId);
		FightingLevel nextLevel = MainGame.GetFightingLevel(nextFightId);
		if (nextLevel == null)
		{
			Debug.LogError("Can't get fighting level with id: " + nextFightId);
			return false;
		}
		if (!nextLevel.IsValid())
		{
			Debug.LogError("Fighting level [" + nextLevel.id + "] is not valid!");
			return false;
		}
		string levelGdPointId = nextLevel.LevelGdPointId;
		if (MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(levelGdPointId) == null)
		{
			Debug.LogError("Can't find GD point data with id [" + levelGdPointId + "]!");
			return false;
		}
		LazyUI.GetWindow<UIPrefightWindow>().Open(new UIPrefightWindowData(nextFightId, delegate
		{
			LazyUI.Get<UIFade>().Fade(1f, delegate
			{
				CurrentLevel.ApplyStageId(customStageid);
				Stop();
				GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FightWon, lastPlayedLevelId);
				GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(levelGdPointId);
				if (gDPointDataById != null)
				{
					if (MainGame.PlayerData.HasOverheadItem)
					{
						MainGame.PlayerData.DropOverheadItem();
					}
					PlayerController.Teleport(new GDPointTeleportData(gDPointDataById, nextLevel.EnvironmentPreset, "", null, donNotFade: true));
					StartPreFight(nextFightId);
				}
			}, delegate
			{
			});
		}, allowClose: false));
		return true;
	}
}
