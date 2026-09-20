using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LazyBearTechnology;
using UnityEngine;

public class MainGame : MonoBehaviour
{
	public enum GameState
	{
		MainMenu,
		InGame
	}

	public static Action OnGameStarted;

	public static Action OnGoToMainMenu;

	public static Action OnGamePaused;

	public static Action OnGameUnpaused;

	public const string MAIN_SCENE = "MainScene";

	public const string LOGO_SCENE = "Logos";

	[SerializeField]
	private GUIElements guiElements;

	[SerializeField]
	private GameSave gameSave;

	public PlayerUniqueCommandHolder PlayerUniqueCommandHolder = new PlayerUniqueCommandHolder();

	[SerializeField]
	private GameInfo gameInfo;

	[SerializeField]
	private UIMainMenuInfoPanel uiMainMenuInfoPanel;

	[SerializeField]
	private GameObject mainMenuViewObject;

	[SerializeField]
	private Vector3 mainMenuViewScaleX1 = Vector3.one;

	[SerializeField]
	private Vector3 mainMenuViewScaleX2 = Vector3.one * 2f;

	private PlayerController playerController;

	[SerializeField]
	private GraphHelper graphHelper;

	[SerializeField]
	private GameObject playerPrefab;

	[SerializeField]
	private UpdateManager updateManager;

	[SerializeField]
	private string conveyorPresetLoadOnNewGame;

	public MovementSystem movementSystem = new MovementSystem();

	public CraftSystem craftSystem = new CraftSystem();

	public DropSystem dropSystem = new DropSystem();

	public FightingLevelSystem fightingLevelSystem = new FightingLevelSystem();

	public PerkSystem perkSystem = new PerkSystem();

	public GameLogicsSystem gameLogicsSystem = new GameLogicsSystem();

	public QuestSystem questSystem = new QuestSystem();

	public ZombieSystem zombieSystem = new ZombieSystem();

	public ZombiePorterSystem zombiePorterSystem = new ZombiePorterSystem();

	public ConveyorSystem conveyorSystem = new ConveyorSystem();

	public RiverDropSystem riverDropSystem = new RiverDropSystem();

	public WgoCustomDeathSystem wgoCustomDeathSystem = new WgoCustomDeathSystem();

	public WgoDelayedEventsSystem wgoDelayedEventsSystem = new WgoDelayedEventsSystem();

	public WgoDelayedSpawnSystem wgoDelayedSpawnSystem = new WgoDelayedSpawnSystem();

	public NPCLifeSimulator npcLifeSimulator = new NPCLifeSimulator();

	private SaveSlotData saveSlotData;

	public List<GameSceneConfig> gameSceneConfigs = new List<GameSceneConfig>();

	private bool startingNewGame;

	private bool continueGame;

	private bool deferStartupMainMenu;

	private bool backgroundPreloadRegistered;

	private float gameSceneLoadProgress;

	private static string entrySceneToLoadCached;

	public GameState gameState { get; private set; }

	public static MainGame Instance { get; private set; }

	public static UpdateManager UpdateManager => Instance.updateManager;

	public static WorldData WorldData => Instance.GameSave.worldData;

	public static ZombieSystemData ZombieSystemData => Instance.GameSave.zombieSystemData;

	public static ConveyorSystemData ConveyorSystemData => Instance.GameSave.conveyorSystemData;

	public static PlayerController PlayerController => Instance.playerController;

	public static PlayerData PlayerData { get; private set; }

	public static bool IsGamePaused { get; private set; }

	public static string ConveyorPresetLoadOnNewGame => Instance.conveyorPresetLoadOnNewGame;

	public static GameSaveVersion GameSaveVersion => Instance.GameSave.SaveVersion;

	public static string EntrySceneToLoad => "RuinedTemple";

	public static HashSet<string> FirstQuestSceneIds => new HashSet<string> { "NorthRuinedTemple", "Prison" };

	public GameSave GameSave => gameSave;

	public GraphHelper GraphHelper => graphHelper;

	public SaveSlotData SaveSlotData => saveSlotData;

	public static FightingLevel GetFightingLevel(string id)
	{
		return Instance.fightingLevelSystem.GetFightingLevel(id);
	}

	private void Awake()
	{
		Debug.Log("Starting game, ver. " + LazySingletonSO<GameInfo>.Instance.Version + "." + LazySingletonSO<GameInfo>.Instance.gitCommitShortHash);
		Debug.Log($"#shutdown# MainGame.Awake (shutdownRequested:[{GameShutdown.IsRequested}] isQuitting:[{GameShutdown.IsQuitting}])");
		gameState = GameState.MainMenu;
		Instance = this;
		if (DevUtils.IsDemoBitsummitActive)
		{
			VoiceOverSettings.IsEnabled = false;
		}
		playerController = GetPlayerController();
		playerController.Initialize();
		PlayerController.SetDisabledStateType(DisabledStateType.ByMainMenu, isEnabled: false);
		ResolutionConfig.InitAvailableResolutions();
		GameSettings.OnResolutionChanged += OnResolutionChanged;
		LLBase.InitReplacementRuleSet(new LocaleReplacementRuleSet());
		ModsBootstrap.EnsureOnStartup();
		GameSettings.Instance.ApplySettings();
		VoiceOverSettings.IsEnabled = GameSettings.Instance.voiceOverMode == VoiceOverMode.VoiceOver;
		ApplyMainMenuViewScaleByCurrentResolution();
	}

	private void OnDestroy()
	{
		GameSettings.OnResolutionChanged -= OnResolutionChanged;
		InWorldSfxFilterController.Shutdown();
	}

	private void Start()
	{
		Debug.Log($"#shutdown# MainGame.Start begin (shutdownRequested:[{GameShutdown.IsRequested}] isQuitting:[{GameShutdown.IsQuitting}])");
		if (GameShutdown.IsQuitting)
		{
			return;
		}
		Debug.Log("#shutdown# MainGame.Start: WgoPartBakedDataCollection.LoadCache");
		LazySingletonSerializedSO<WgoPartBakedDataCollection>.Instance.LoadCache();
		if (gameState == GameState.MainMenu)
		{
			SetMainMenuEnabledState(isEnabled: true);
		}
		Debug.Log("#shutdown# MainGame.Start: GameBalance.LoadGameBalance");
		GameBalance.LoadGameBalance();
		Debug.Log("#shutdown# MainGame.Start: LazyInput.TryInit");
		LazyInput.TryInit();
		Debug.Log("#shutdown# MainGame.Start: LazyUI.Init");
		LazyUI.Init();
		Debug.Log("#shutdown# MainGame.Start: guiElements.Initialize");
		guiElements.Initialize();
		Debug.Log("#shutdown# MainGame.Start: GameSceneConfig.LoadAllConfigs");
		gameSceneConfigs = GameSceneConfig.LoadAllConfigs();
		Debug.Log("#shutdown# MainGame.Start: systems init");
		conveyorSystem.Init();
		updateManager.AddScheduledUpdate(new ScheduledUpdate(new List<ICustomUpdatable> { craftSystem, dropSystem, conveyorSystem }, 0.2f));
		updateManager.AddScheduledUpdate(new ScheduledUpdate(new List<ICustomUpdatable> { movementSystem, wgoDelayedEventsSystem, riverDropSystem }, 0f));
		updateManager.AddScheduledUpdate(new ScheduledUpdate(new List<ICustomUpdatable> { perkSystem, questSystem, wgoDelayedSpawnSystem }, 1f));
		updateManager.AddScheduledUpdate(new ScheduledUpdate(new List<ICustomUpdatable>
		{
			EnvironmentEngine.Instance,
			gameLogicsSystem,
			zombieSystem,
			wgoCustomDeathSystem,
			npcLifeSimulator
		}, 0.04f));
		updateManager.AddScheduledUpdate(new ScheduledUpdate(new List<ICustomUpdatable> { zombiePorterSystem }, 2f));
		OnGoToMainMenu = (Action)Delegate.Combine(OnGoToMainMenu, (Action)delegate
		{
			LazyAudio.StopAllPlaylistsImmediately();
			LazyAudio.StopAll();
			LazyAudio.PlayPlaylist("main_menu");
		});
		LazyWindowsStackController.OnWindowOpened += OnWindowOpened;
		LazyWindowsStackController.OnWindowClosed += OnWindowClosed;
		LazyWindowsStackController.OnAllWindowsClosed += OnAllWindowsClosed;
		InWorldSfxFilterController.Init();
		Debug.Log("#shutdown# MainGame.Start: EnvironmentEngine.PreloadTimeOfDayPresets");
		EnvironmentEngine.Instance.PreloadTimeOfDayPresets();
		LazyInput.OnInputChanged += delegate
		{
			CursorController.ChangeCursorVisibleState(!LazyInput.IsGamepadActive);
		};
		CursorController.ChangeCursorVisibleState(!LazyInput.IsGamepadActive);
		VoiceOverPlayer.MuteCheck = DialogDataContainer.IsVoiceOverMuted;
		VoiceOverModLoader.Refresh(GameSettings.Instance.language);
		Debug.Log("#shutdown# MainGame.Start: guiElements.PreloadWindows");
		guiElements.PreloadWindows();
		Debug.Log("#shutdown# MainGame.Start: InitNetwork");
		InitNetwork();
		if (LazyUITester.isTesting)
		{
			LazyUITester.OnGameStart();
			return;
		}
		Debug.Log("#shutdown# MainGame.Start: HUD + main menu music");
		LazyUI.Get<HUD>().SetDisableState(HudStateType.MainMenu, isEnabled: false);
		LazyAudio.StopAllPlaylistsImmediately();
		LazyAudio.PlayPlaylist("main_menu");
		SetMainMenuMixer(active: true);
		if (DevUtils.IsDemoBitsummitActive)
		{
			DevUtils.isMainSceneSkipped = false;
			StartNewGame();
		}
		else
		{
			TryOpenStartupMainMenu();
		}
		GameSettings.Instance.ApplySettings();
		if (!deferStartupMainMenu)
		{
			RegisterBackgroundPreloadTasks();
		}
		StartCoroutine(UnloadAstarCacheWhenAstarPathIsReady());
		Debug.Log($"#shutdown# MainGame.Start end (deferStartupMainMenu:[{deferStartupMainMenu}])");
	}

	private void TryOpenStartupMainMenu()
	{
		if (ConsolesManager.IsStartupPreloadOverlayActive)
		{
			deferStartupMainMenu = true;
		}
		else
		{
			OpenStartupMainMenu();
		}
	}

	public void CompleteStartupAfterPreloadOverlay()
	{
		if (deferStartupMainMenu && !GameShutdown.IsRequested)
		{
			deferStartupMainMenu = false;
			OpenStartupMainMenu(fromPreloader: true);
		}
	}

	private void OpenStartupMainMenu(bool fromPreloader = false)
	{
		UIMainMenuWindow window = LazyUI.GetWindow<UIMainMenuWindow>();
		if (fromPreloader)
		{
			window.OpenFromPreloader(LazySingleton<ConsolesManager>.Instance.PreloadOverlay, OnPreloaderIntroComplete);
			return;
		}
		window.Open(null);
		TryShowDLCPopUpOnStartUp();
	}

	private void OnPreloaderIntroComplete()
	{
		if (!GameShutdown.IsRequested)
		{
			RegisterBackgroundPreloadTasks();
			TryShowDLCPopUpOnStartUp();
		}
	}

	private void TryShowDLCPopUpOnStartUp()
	{
		DLCVersion dlcVersion = DLCVersion.Preorder;
		GameSettings instance = GameSettings.Instance;
		if (DLCEngine.IsDLCAvailable(dlcVersion) && !instance.IsDlcStartupPopUpShown(dlcVersion))
		{
			LazyUI.GetWindow<UIPreorderWindow>().Open(null);
			instance.MarkDlcStartupPopUpShown(dlcVersion);
		}
	}

	private IEnumerator UnloadAstarCacheWhenAstarPathIsReady()
	{
		while ((UnityEngine.Object)(object)AstarPath.active == null)
		{
			yield return null;
		}
		while (AstarPath.active.graphs == null || AstarPath.active.graphs.Length == 0)
		{
			yield return null;
		}
		_ = AstarPath.active.data.file_cachedStartup;
		AstarPath.active.data.file_cachedStartup = null;
		Debug.Log("Astar cache unloaded");
	}

	private void RegisterBackgroundPreloadTasks()
	{
		if (backgroundPreloadRegistered || GameShutdown.IsRequested)
		{
			return;
		}
		backgroundPreloadRegistered = true;
		BackgroundLoading.IsActive = true;
		LoadingPipeline instance = LoadingPipeline.Instance;
		LazyTerrainMeshPool terrainPool = LazySingleton<LazyTerrainMeshPool>.Instance;
		instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("LazyTerrainMeshPool.Instance.InitAsync()", terrainPool.InitAsync()), () => terrainPool.LocalProgress, 0.1f);
		ConstructorPartPool constructorPool = LazySingleton<ConstructorPartPool>.Instance;
		instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("ConstructorPartPool.Instance.InitAsync()", constructorPool.InitAsync()), () => constructorPool.LocalProgress, 0.2f);
		BakedChunkableObjectPool bakedPool = LazySingleton<BakedChunkableObjectPool>.Instance;
		instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("BakedChunkableObjectPool.Instance.InitAsync()", bakedPool.InitAsync()), () => bakedPool.LocalProgress, 0.3f);
		WgoPartPool wgoPool = LazySingleton<WgoPartPool>.Instance;
		instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("WgoPartPool.Instance.InitAsync()", wgoPool.InitAsync()), () => wgoPool.LocalProgress, 0.4f);
		WorldFXPool fxPool = LazySingleton<WorldFXPool>.Instance;
		instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("WorldFXPool.Instance.InitAsync()", fxPool.InitAsync()), () => fxPool.LocalProgress, 0.3f);
		instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("DropView.PreloadAsync()", DropView.PreloadAsync()), () => DropView.PreloadProgress, 0.05f);
		if (FlowScriptAssetLoadPolicy.UsesBackgroundPreload)
		{
			instance.RegisterTask(LoadingStage.BackgroundPreload, MeasureTask("FlowScriptAssetLoadPolicy.PreloadAsync()", FlowScriptAssetLoadPolicy.PreloadAsync()), () => FlowScriptAssetLoadPolicy.PreloadProgress, 0.2f);
		}
		static UniTask MeasureTask(string taskName, UniTask task)
		{
			return MeasureTaskInternal(taskName, task);
		}
		static async UniTask MeasureTaskInternal(string taskName, UniTask task)
		{
			await task;
		}
	}

	public void Update()
	{
		ModsBootstrap.Tick();
		SteamWorkshopCreatorConfig.Tick();
	}

	public void StartNewGame(bool skipMainScene = false)
	{
		StartNewGameWithSlotName(SaveSystem.GetNameForNewSlot(SaveSystem.SaveSlotDataList), skipMainScene);
	}

	public void StartNewGameInLimitedSaveSlot(int slotIndex, bool skipMainScene = false)
	{
		StartNewGameWithSlotName(SaveSystem.GetNameForLimitedSaveSlot(slotIndex), skipMainScene);
	}

	private void StartNewGameWithSlotName(string slotName, bool skipMainScene = false)
	{
		DLCEngine.ResetDLCStateCached();
		if (!skipMainScene)
		{
			LazySingleton<GlobalNavigationManager>.Instance.ClearAll();
		}
		saveSlotData = new SaveSlotData
		{
			slotName = slotName
		};
		CreateGameSaveAndStart(PlayerSkinHelper.playerStandardCustomizationData, !skipMainScene);
	}

	public void CompleteGame(bool shouldGoToMenuOnReturn, Action onCreditsWindowClosed = null)
	{
		Debug.Log("#DEV# CompleteGame, show credits");
		UICinematic uICinematic = LazyUI.Get<UICinematic>();
		if (uICinematic != null && uICinematic.gameObject.activeSelf)
		{
			uICinematic.DisableCinematic(null, instant: true);
		}
		SetMainMenuInfoPanelEnabled(isEnabled: false);
		LazyUI.GetWindow<UICreditsWindow>().OpenAfterGameComplete(shouldGoToMenuOnReturn, onCreditsWindowClosed);
	}

	public void CreateGameSaveAndStart(PlayerCustomizationData customizationData, bool startQuest = true)
	{
		entrySceneToLoadCached = EntrySceneToLoad;
		LoadingWindowData data = new LoadingWindowData(entrySceneToLoadCached, delegate
		{
			gameSave = new GameSave();
			GameSave.SetupNewGameSave(gameSave);
			PlayerData = gameSave.playerData;
			PlayerData.TryApplyStartState();
			PlayerData.ApplyCustomization(customizationData);
			startingNewGame = true;
			if (DLCEngine.IsDLCAvailable(DLCVersion.Preorder))
			{
				gameSave.GivePreorderReward();
			}
			else
			{
				Debug.Log("No preorder available GivePreorderReward failed");
			}
			StartGame(isNewGame: true);
			gameSave.questSystemData.AwaitQuest(ConstDef.Get("new_game_start_quest").StringValue);
		});
		LoadingPipeline.Instance.ClearStage(LoadingStage.GameplaySceneLoad);
		LazyUI.Get<UILoadingOverlay>().Draw(data);
	}

	public void ContinueGame(SaveSlotData saveSlotData, GameSave gameSave)
	{
		entrySceneToLoadCached = EntrySceneToLoad;
		LoadingPipeline.Instance.ClearStage(LoadingStage.GameplaySceneLoad);
		UILoadingOverlay uILoadingOverlay = LazyUI.Get<UILoadingOverlay>();
		if (uILoadingOverlay.IsShown)
		{
			OnOverlayReady();
		}
		else
		{
			uILoadingOverlay.Draw(new LoadingWindowData(entrySceneToLoadCached, OnOverlayReady));
		}
		void OnOverlayReady()
		{
			LazySingleton<GlobalNavigationManager>.Instance.ClearAll();
			Debug.Log("Continue Game: slot" + saveSlotData.slotName + " save version:[" + gameSave.GameSaveVer + "]");
			continueGame = true;
			this.gameSave = gameSave;
			this.saveSlotData = saveSlotData;
			PlayerData = gameSave.playerData;
			PlayerSkinHelper.ApplySkin(gameSave.playerData.customization, onlyForCustomizationCharacter: false);
			PlayerSkinHelper.ApplyPlayerColorsByData(gameSave.playerData.customization, onlyForCustomizationCharacter: false);
			if (EnvironmentEngine.Instance.IsPaused)
			{
				Debug.LogWarning("Try load game with EnvironmentEngine paused. Unpausing");
				EnvironmentEngine.Instance.IsPaused = false;
			}
			EnvironmentEngine.Instance.SetTimeOfDayPreset(gameSave.environmentData.timeOfDayPresetName);
			WeatherSystem.Instance.RestoreFSMStateFromData();
			StartGame();
		}
	}

	private async void StartGame(bool isNewGame = false)
	{
		if (GameShutdown.IsRequested)
		{
			return;
		}
		SetMainMenuEnabledState(isEnabled: false);
		SetMainMenuMixer(active: false);
		gameSave.PrepareForGame();
		if (isNewGame || SaveFixer.Apply(gameSave, gameSceneConfigs))
		{
			gameSave.GameSaveVer = LazySingletonSO<GameInfo>.Instance.Version;
		}
		playerController.PreparePlayerForGame(gameSave.playerData);
		if (!(await AwaitBackgroundPreloadAsync()) || GameShutdown.IsRequested)
		{
			return;
		}
		gameState = GameState.InGame;
		graphHelper.ScanGDPointGraph();
		TeleportPointGraph.Build(gameSave.worldData);
		SetSystemsPauseState(isPaused: false);
		gameSave.zombieSystemData.ResumeCrafterWorkAfterLoad();
		PlayerController.SetDisabledStateType(DisabledStateType.ByMainMenu, isEnabled: true);
		LazyUI.Get<HUD>().SetDisableState(HudStateType.MainMenu, isEnabled: true, new HUDData(gameSave));
		FlyingTechPoint.Clear();
		LazySingleton<ChunkManager>.Instance.IsActive = true;
		if (await AwaitGameplaySceneLoadAsync(isNewGame) && !GameShutdown.IsRequested)
		{
			if (!isNewGame && !DLCEngine.IsDLCAvailable(DLCVersion.Preorder))
			{
				gameSave.TryRemovePreorderReward();
			}
			OnGameStarted?.Invoke();
		}
	}

	private async UniTask<bool> AwaitBackgroundPreloadAsync()
	{
		while (true)
		{
			RegisterBackgroundPreloadTasks();
			BackgroundLoading.IsActive = false;
			if (backgroundPreloadRegistered)
			{
				try
				{
					await LoadingPipeline.Instance.AwaitStage(LoadingStage.BackgroundPreload);
					return true;
				}
				catch (OperationCanceledException)
				{
				}
			}
			if (!(await GameShutdown.WaitForResumeAsync()))
			{
				break;
			}
			backgroundPreloadRegistered = false;
			LoadingPipeline.Instance.ClearStage(LoadingStage.BackgroundPreload);
			await UniTask.Yield();
		}
		return false;
	}

	private async UniTask<bool> AwaitGameplaySceneLoadAsync(bool isNewGame)
	{
		LoadingPipeline lp = LoadingPipeline.Instance;
		while (true)
		{
			lp.ClearStage(LoadingStage.GameplaySceneLoad);
			lp.RegisterTask(LoadingStage.GameplaySceneLoad, LoadGameScene(isNewGame), () => gameSceneLoadProgress);
			try
			{
				await lp.AwaitStage(LoadingStage.GameplaySceneLoad);
				return true;
			}
			catch (OperationCanceledException)
			{
			}
			if (!(await GameShutdown.WaitForResumeAsync()))
			{
				break;
			}
			await UniTask.Yield();
		}
		return false;
	}

	public void PrepareGameForNetwork(GameSave gameSave, NetworkPlayer networkPlayer, bool isHost)
	{
		gameSave.PrepareForGame();
		gameSave.zombieSystemData.ResumeCrafterWorkAfterLoad();
		if (isHost)
		{
			gameSave.hostPlayer = networkPlayer;
		}
		PlayerUniqueCommandHolder.RegisterData(networkPlayer);
		playerController.PreparePlayerForGame(gameSave.playerData);
		if (!isHost)
		{
			SpawnPlayer(gameSave.hostPlayer);
		}
	}

	public async UniTask LoadGameScene(bool isNewGame = false)
	{
		GameShutdown.ThrowIfRequested();
		gameSceneLoadProgress = 0f;
		PlayerController.SetDisabledStateType(DisabledStateType.BySceneLoading, isEnabled: false);
		Debug.Log("LoadGameScene");
		Progress<float> progress = new Progress<float>(delegate(float p)
		{
			gameSceneLoadProgress = p * 0.8f;
		});
		await LazySingleton<GameSceneManager>.Instance.LoadSceneAsync(entrySceneToLoadCached, progress);
		GameShutdown.ThrowIfRequested();
		gameSceneLoadProgress = 0.8f;
		if (isNewGame)
		{
			int questSceneIndex = 0;
			int questSceneCount = FirstQuestSceneIds.Count - 1;
			foreach (string firstQuestSceneId in FirstQuestSceneIds)
			{
				progress = new Progress<float>(delegate
				{
					gameSceneLoadProgress = 0.8f + (float)questSceneIndex / (float)questSceneCount * 0.1f;
				});
				await LazySingleton<GameSceneManager>.Instance.LoadSceneAsync(firstQuestSceneId, progress);
				GameShutdown.ThrowIfRequested();
				questSceneIndex++;
			}
		}
		gameSceneLoadProgress = 0.9f;
		LazyUI.Get<UILoadingOverlay>()?.NotifyAfterSceneWorkStarted();
		await AfterSceneHasLoaded();
		gameSceneLoadProgress = 1f;
	}

	public void SetGameSave(GameSave gameSave)
	{
		this.gameSave = gameSave;
	}

	public void SpawnPlayer(NetworkPlayer player)
	{
		PlayerPhysicalBody component = UnityEngine.Object.Instantiate(playerPrefab).GetComponent<PlayerPhysicalBody>();
		component.InitNetworkPlayer(player);
		player.SubscribeToPlayerDataChanges(component);
		component.gameObject.SetActive(value: true);
		Debug.Log("Spawned player for network player");
	}

	public void GoToMenu(Action onFadeInComplete = null, bool skipFadeIn = false, FadeFlag fadeOutFlag = FadeFlag.Common)
	{
		UIFade fadeElement = LazyUI.Get<UIFade>();
		if (skipFadeIn)
		{
			GoToMenuAfterFadeIn(fadeElement, onFadeInComplete, fadeOutFlag);
			return;
		}
		fadeElement.FadeIn(0.5f, delegate
		{
			GoToMenuAfterFadeIn(fadeElement, onFadeInComplete, fadeOutFlag);
		});
	}

	private async void GoToMenuAfterFadeIn(UIFade fadeElement, Action onFadeInComplete, FadeFlag fadeOutFlag)
	{
		onFadeInComplete?.Invoke();
		gameState = GameState.MainMenu;
		FightingGameController instance = LazySingleton<FightingGameController>.Instance;
		if (instance.CurrentFightState == FightState.InPreFight)
		{
			instance.CancelPreFight();
			instance.TryCloseFightEndWindows();
		}
		if (instance.CurrentFightState == FightState.ActiveFight)
		{
			instance.Stop();
			instance.TryCloseFightEndWindows();
		}
		FlyingTechPoint.Clear();
		LazySingleton<GlobalNavigationManager>.Instance.ClearAll();
		SetMainMenuEnabledState(isEnabled: true);
		guiElements.Clear();
		OnGoToMainMenu?.Invoke();
		SetSystemsPauseState(isPaused: true);
		gameSave.UnPrepareFromGame();
		playerController.UnPreparePlayerFromGame();
		Debug.Log("MainGame.GoToMenu");
		LazySingleton<ChunkManager>.Instance.IsActive = false;
		playerController.ClearCurrentGameScene();
		LazySingleton<GameSceneManager>.Instance.UnloadAllScenes();
		PlayerController.SetDisabledStateType(DisabledStateType.ByMainMenu, isEnabled: false);
		LazyUI.Get<HUD>().SetDisableState(HudStateType.MainMenu, isEnabled: false);
		playerController.WispController.ChangeActiveState(enabled: false);
		WeatherSystem.Instance.ClearWeather();
		SetMainMenuMixer(active: true);
		GUIElements.Instance.NpcWidget.Hide();
		ResetAllGameData();
		if (DevUtils.IsDemoBitsummitActive)
		{
			DevUtils.isMainSceneSkipped = false;
			StartNewGame();
		}
		else
		{
			LazyUI.GetWindow<UIMainMenuWindow>().Open(null);
		}
		await Awaitable.NextFrameAsync();
		fadeElement.FadeOut(0.2f, null, fadeOutFlag);
	}

	private void PauseGame()
	{
		SetSystemsPauseState(isPaused: true);
		LazySingleton<FightingGameController>.Instance.PauseAgentsMovement();
		Object3DMesh.SetDestructionTweenPauseState(isPaused: true);
		IsGamePaused = true;
		OnGamePaused?.Invoke();
	}

	private void UnpauseGame()
	{
		SetSystemsPauseState(isPaused: false);
		LazySingleton<FightingGameController>.Instance.UnpauseAgentsMovement();
		Object3DMesh.SetDestructionTweenPauseState(isPaused: false);
		IsGamePaused = false;
		OnGameUnpaused?.Invoke();
	}

	private void ResetAllGameData()
	{
		GameScene.ClearGlobalCaches();
		GlobalScriptsManager.TerminateAllRunningScripts();
		WgoDataScriptsManager.DestroyAll();
		UIMultiAnswer.ForceDisableAll();
		UISpeechBubble.ForceRemoveAll();
		Object3DOptimizedPart.ClearAtlasTextureCache();
		LazySingleton<ChunkManager>.Instance.ClearAll();
		Debug.Log("ResetAllGameData");
	}

	private async UniTask AfterSceneHasLoaded()
	{
		PlayerController.SetDisabledStateType(DisabledStateType.BySceneLoading, isEnabled: true);
		UIFade uIFade = LazyUI.Get<UIFade>();
		playerController.WispController.ChangeActiveState(GameSave.playerData.isWispEnabled);
		TrySetPlayerPosition();
		if (continueGame)
		{
			if (PlayerData.HasOverheadItem)
			{
				PlayerController.SetOverheadItems(PlayerData.OverheadItems);
			}
			if (PlayerData.tutorialArrowWgoId != null && !string.IsNullOrEmpty(PlayerData.tutorialArrowWgoId.Id))
			{
				LazySingleton<UITutorialArrow>.Instance.Attach(GameSave.worldData.GetWgoData(PlayerData.tutorialArrowWgoId));
			}
		}
		if (startingNewGame)
		{
			startingNewGame = false;
			uIFade.FadeIn(0f, null, FadeFlag.FlowScript);
			await Awaitable.NextFrameAsync();
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.StartNewGame);
		}
		CPDirectShadowsBlur.Initialize();
		LazyAudio.StopAllPlaylistsImmediately();
		LazyAudio.PlayPlaylist("gameplay");
		if (continueGame)
		{
			EnvironmentEngine.Instance.SetTimeOfDayPreset("indoor");
			await Awaitable.NextFrameAsync();
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.AfterSleep);
			continueGame = false;
		}
		await Resources.UnloadUnusedAssets();
		GC.Collect();
		await Awaitable.NextFrameAsync();
		await Awaitable.NextFrameAsync();
		LazyUI.Get<UILoadingOverlay>().Hide();
	}

	private void TrySetPlayerPosition()
	{
		if (startingNewGame && !continueGame)
		{
			PlayerSpawnPoint playerSpawnPoint = UnityEngine.Object.FindObjectOfType<PlayerSpawnPoint>(includeInactive: true);
			if (!(playerSpawnPoint == null))
			{
				playerController.SetPosition(playerSpawnPoint.transform.position, updateWispWgoData: false);
				EnvironmentEngine.Instance.SetTimeOfDayPreset(playerSpawnPoint.environmentPreset);
			}
		}
	}

	private PlayerController GetPlayerController()
	{
		PlayerController obj = UnityEngine.Object.FindObjectOfType<PlayerController>(includeInactive: true);
		if (obj == null)
		{
			throw new Exception("Can't find [PlayerController]");
		}
		return obj;
	}

	private void OnWindowOpened(LazyWidgetBase window)
	{
		if (LazyWindowsStackController.HasAnyModalWindowOpened && gameState == GameState.InGame)
		{
			playerController.SetControlTakenType(TakenControlType.ByUI, isEnabled: false);
			if (!(window is UIFishingWindow) && !(window is UICreditsWindow))
			{
				PauseGame();
			}
		}
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.OpenUIWindow, window.GetType().Name);
	}

	private void OnWindowClosed(LazyWidgetBase window)
	{
		if (!LazyWindowsStackController.HasAnyModalWindowOpened && gameState == GameState.InGame)
		{
			playerController.SetControlTakenType(TakenControlType.ByUI, isEnabled: true);
			if (!(window is UIFishingWindow) && !(window is UICreditsWindow))
			{
				UnpauseGame();
			}
		}
		if (window is UICraftWindow && !gameSave.playerData.openedCraftWindowOnce)
		{
			gameSave.playerData.openedCraftWindowOnce = true;
			GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.FirstCloseCraftWindow);
		}
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.CloseUIWindow, window.GetType().Name);
	}

	private void OnAllWindowsClosed()
	{
		if (gameState == GameState.InGame)
		{
			playerController.SetControlTakenType(TakenControlType.ByUI, isEnabled: true);
		}
	}

	private void InitNetwork()
	{
	}

	private void SetSystemsPauseState(bool isPaused)
	{
		Debug.Log($"Set systems pause state to {isPaused}");
		updateManager.IsActive = !isPaused;
	}

	public void SetMainMenuInfoPanelEnabled(bool isEnabled)
	{
		if (isEnabled)
		{
			uiMainMenuInfoPanel.ShowFullPanel();
		}
		else
		{
			uiMainMenuInfoPanel.Hide();
		}
	}

	private void SetMainMenuEnabledState(bool isEnabled)
	{
		if (isEnabled)
		{
			uiMainMenuInfoPanel.ShowFullPanel();
		}
		else
		{
			uiMainMenuInfoPanel.ShowVersionInGameIfNeeded();
		}
		mainMenuViewObject.SetActive(isEnabled);
		ApplyMainMenuViewScaleByCurrentResolution();
		if (isEnabled)
		{
			CameraSystem.Instance.ActiveCameraController.SetTargetInstant(mainMenuViewObject.transform);
		}
		else
		{
			CameraSystem.Instance.ActiveCameraController.SetTargetInstant(playerController.PhysicalBody.PlayerView.transform);
		}
		CameraSystem.Instance.ActiveCameraController.UpdateTargetPosInstant();
	}

	private static void SetMainMenuMixer(bool active)
	{
		if (!(WeatherSystem.Instance == null))
		{
			WeatherSystem.Instance.AudioMixerStateController.SetLayerActive(AudioMixerSnapshotLayer.MainMenu, active);
		}
	}

	private void OnResolutionChanged(IntVector2 resolution)
	{
		ApplyMainMenuViewScaleByCurrentResolution();
	}

	private void ApplyMainMenuViewScaleByCurrentResolution()
	{
		if (!(mainMenuViewObject == null))
		{
			bool flag = ResolutionConfig.currentResolution != null && ResolutionConfig.currentResolution.UseMainMenuScaleX2;
			mainMenuViewObject.transform.localScale = (flag ? mainMenuViewScaleX2 : mainMenuViewScaleX1);
		}
	}
}
