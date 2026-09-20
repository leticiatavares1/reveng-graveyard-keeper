using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LazyBearTechnology.Preloader;
using Rewired;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LazyBearTechnology;

public class ConsolesManager : LazySingleton<ConsolesManager>
{
	private const string PS5_ACTIVITY_ID = "continue";

	private static bool showNoControllerWarning;

	private static float showNoControllerWarningDelay;

	private static float frozenTimeScale;

	private static bool unfreezeGame;

	private static bool isFrozen;

	private bool isUserInitializationFinished;

	private bool triggerUserAdded;

	[SerializeField]
	private UIPreloadOverlay preloadOverlay;

	[SerializeField]
	private CanvasScaler preloadOverlayCanvasScaler;

	[SerializeField]
	private UIUnsupportedResolutionWindow unsupportedResolutionWindow;

	[SerializeField]
	private LazyPreloader lazyPreloader;

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private bool isPreloadWindowInitialized;

	[SerializeField]
	private AssetReferenceT<LazyApplicationSettings> lazyApplicationSettingsRef;

	[SerializeField]
	private AssetReferenceT<ZombieCustomizationConfig> zombieCustomizationConfigRef;

	[SerializeField]
	private AssetReferenceT<WgoPartPoolInitialSizesConfig> wgoPartPoolInitialSizesConfigRef;

	[SerializeField]
	private AssetReferenceT<WgoPartBakedDataCollection> wgoPartBakedDataCollectionRef;

	[SerializeField]
	private AssetReferenceT<VoiceOverSettings> voiceOverSettingsRef;

	[SerializeField]
	private AssetReferenceT<TMPShaderSetup> tmpShaderSetupRef;

	[SerializeField]
	private AssetReferenceT<SurfaceStepSoundSettings> stepSoundSettingsRef;

	[SerializeField]
	private AssetReferenceT<NPCLifeSimulatorConfiguration> npcLifeSimulatorConfigurationRef;

	[SerializeField]
	private AssetReferenceT<LazyTerrainMeshCollection> lazyTerrainMeshCollectionRef;

	[SerializeField]
	private AssetReferenceT<GlobalResources> globalResourcesRef;

	[SerializeField]
	private AssetReferenceT<GameResDisplayConfig> gameResDisplayConfigRef;

	[SerializeField]
	private AssetReferenceT<GamepadTypeData> gamepadTypeDataRef;

	[SerializeField]
	private AssetReferenceT<GameInfo> gameInfoRef;

	[SerializeField]
	private AssetReferenceT<GameBindings> gameBindingsRef;

	[SerializeField]
	private AssetReferenceT<EasySpritesCollection> easySpritesCollectionRef;

	[SerializeField]
	private AssetReferenceT<DeformingGrassSettings> deformingGrassSettingsRef;

	[SerializeField]
	private AssetReferenceT<ConstructorPartPoolInitialSizesConfig> constructorPartPoolInitialSizesConfigRef;

	[SerializeField]
	private AssetReferenceT<ConstructorPartBoundsConfig> constructorPartBoundsConfigRef;

	[SerializeField]
	private AssetReferenceT<BakedChunkableObjectPoolInitialSizesConfig> bakedChunkableObjectPoolInitialSizesConfigRef;

	[SerializeField]
	private AssetReferenceT<AudioConfig> audioConfigRef;

	private readonly List<AsyncOperationHandle> configHandles = new List<AsyncOperationHandle>();

	private AsyncOperationHandle startupDownloadHandle;

	private AsyncOperationHandle startupSceneHandle;

	private bool isLazyPreloaderFinished;

	private bool isLoadingActionCalled;

	private bool startupLoadCompleted;

	private int startupAttemptId;

	private bool isFinishingStartupOverlay;

	public static bool IsUserInitializationFinished => LazySingleton<ConsolesManager>.Instance.isUserInitializationFinished;

	public static bool IsStartupPreloadOverlayActive { get; private set; }

	public UIPreloadOverlay PreloadOverlay => preloadOverlay;

	protected override void Awake()
	{
		lazyPreloader.Run(OnFinishedLazyPreloaderCoroutine());
		LazySingletonSO<LazyApplicationSettings>.SetReference(LoadConfig(lazyApplicationSettingsRef));
		LazySingletonSO<TMPShaderSetup>.SetReference(LoadConfig(tmpShaderSetupRef));
		LazySingletonSO<GlobalResources>.SetReference(LoadConfig(globalResourcesRef));
		LazySingletonSO<GameResDisplayConfig>.SetReference(LoadConfig(gameResDisplayConfigRef));
		LazySingletonSO<GamepadTypeData>.SetReference(LoadConfig(gamepadTypeDataRef));
		LazySingletonSO<GameInfo>.SetReference(LoadConfig(gameInfoRef));
		LazySingletonSO<GameBindings>.SetReference(LoadConfig(gameBindingsRef));
		LazySingletonSO<EasySpritesCollection>.SetReference(LoadConfig(easySpritesCollectionRef));
		LazySingletonSO<AudioConfig>.SetReference(LoadConfig(audioConfigRef));
		Init();
	}

	private T LoadConfig<T>(AssetReferenceT<T> assetReference) where T : ScriptableObject
	{
		AsyncOperationHandle<T> handle = default(AsyncOperationHandle<T>);
		T result = AddressableUtils.LoadAssetReferenceSync(assetReference, ref handle);
		if (handle.IsValid())
		{
			configHandles.Add(handle);
		}
		return result;
	}

	private UniTask<T> LoadConfigAsync<T>(AssetReferenceT<T> assetReference) where T : ScriptableObject
	{
		return LoadConfigAsyncInternal(assetReference);
	}

	private async UniTask<T> LoadConfigAsyncInternal<T>(AssetReferenceT<T> assetReference) where T : ScriptableObject
	{
		if (assetReference == null || !assetReference.RuntimeKeyIsValid())
		{
			return null;
		}
		AsyncOperationHandle<T> handle = assetReference.LoadAssetAsync<T>();
		try
		{
			T result = await handle.ToUniTask(null, PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true, autoReleaseWhenCanceled: true);
			if (handle.IsValid())
			{
				configHandles.Add(handle);
			}
			return result;
		}
		catch (OperationCanceledException)
		{
			throw;
		}
		catch (Exception ex2)
		{
			Debug.LogError("Failed to asynchronously load asset with GUID '" + assetReference.AssetGUID + "': " + ex2.Message);
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			return null;
		}
	}

	private async UniTask LoadConfigAndSetReference<T>(AssetReferenceT<T> assetReference, Action<T> setReference) where T : ScriptableObject
	{
		T obj = await LoadConfigAsync(assetReference);
		GameShutdown.ThrowIfRequested();
		setReference(obj);
	}

	private async UniTask LoadLazyTerrainMeshCollectionAsync()
	{
		LazyTerrainMeshCollection obj = await LoadConfigAsync(lazyTerrainMeshCollectionRef);
		GameShutdown.ThrowIfRequested();
		LazySingletonSO<LazyTerrainMeshCollection>.SetReference(obj);
		obj?.Runtime_StripAllMeshCpuData();
	}

	private UniTask LoadDeferredConfigsAsync()
	{
		return UniTask.WhenAll(LoadConfigAndSetReference(zombieCustomizationConfigRef, LazySingletonSO<ZombieCustomizationConfig>.SetReference), LoadConfigAndSetReference(wgoPartPoolInitialSizesConfigRef, LazySingletonSO<WgoPartPoolInitialSizesConfig>.SetReference), LoadConfigAndSetReference(wgoPartBakedDataCollectionRef, LazySingletonSerializedSO<WgoPartBakedDataCollection>.SetReference), LoadConfigAndSetReference(voiceOverSettingsRef, LazySingletonSO<VoiceOverSettings>.SetReference), LoadConfigAndSetReference(stepSoundSettingsRef, LazySingletonSO<SurfaceStepSoundSettings>.SetReference), LoadConfigAndSetReference(npcLifeSimulatorConfigurationRef, LazySingletonSO<NPCLifeSimulatorConfiguration>.SetReference), LoadLazyTerrainMeshCollectionAsync(), LoadConfigAndSetReference(deformingGrassSettingsRef, LazySingletonSO<DeformingGrassSettings>.SetReference), LoadConfigAndSetReference(constructorPartPoolInitialSizesConfigRef, LazySingletonSO<ConstructorPartPoolInitialSizesConfig>.SetReference), LoadConfigAndSetReference(constructorPartBoundsConfigRef, LazySingletonSO<ConstructorPartBoundsConfig>.SetReference), LoadConfigAndSetReference(bakedChunkableObjectPoolInitialSizesConfigRef, LazySingletonSO<BakedChunkableObjectPoolInitialSizesConfig>.SetReference));
	}

	private void OnDestroy()
	{
		Debug.Log($"#shutdown# ConsolesManager.OnDestroy (isQuitting:[{GameShutdown.IsQuitting}])");
		IsStartupPreloadOverlayActive = false;
		GameShutdown.Resumed -= OnGameShutdownResumed;
		GameShutdown.SetQuitBlocker(null);
		if (!GameShutdown.IsQuitting)
		{
			ReleaseStartupHandles();
			ReleaseConfigHandles();
		}
		Debug.Log("#shutdown# ConsolesManager.OnDestroy finished");
	}

	private bool IsStartupSceneLoadInFlight()
	{
		if (startupSceneHandle.IsValid())
		{
			return !startupSceneHandle.IsDone;
		}
		return false;
	}

	private void ReleaseStartupHandles()
	{
		if (startupDownloadHandle.IsValid())
		{
			Debug.Log($"#shutdown# ReleaseStartupHandles: releasing download handle (isDone:[{startupDownloadHandle.IsDone}] status:[{startupDownloadHandle.Status}] percent:[{startupDownloadHandle.PercentComplete}])");
			Addressables.Release(startupDownloadHandle);
			startupDownloadHandle = default(AsyncOperationHandle);
			Debug.Log("#shutdown# ReleaseStartupHandles: download handle released");
		}
		if (startupSceneHandle.IsValid())
		{
			Debug.Log($"#shutdown# ReleaseStartupHandles: releasing scene handle (isDone:[{startupSceneHandle.IsDone}] status:[{startupSceneHandle.Status}] percent:[{startupSceneHandle.PercentComplete}])");
			Addressables.Release(startupSceneHandle);
			startupSceneHandle = default(AsyncOperationHandle);
			Debug.Log("#shutdown# ReleaseStartupHandles: scene handle released");
		}
	}

	private void ReleaseConfigHandles()
	{
		for (int num = configHandles.Count - 1; num >= 0; num--)
		{
			AsyncOperationHandle handle = configHandles[num];
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
		configHandles.Clear();
	}

	private void Init()
	{
		base.Awake();
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		LazyAPI.Platform.Init();
		GameShutdown.Resumed += OnGameShutdownResumed;
		GameShutdown.SetQuitBlocker(IsStartupSceneLoadInFlight);
		ResolutionConfig.InitAvailableResolutions();
		LLBase.InitReplacementRuleSet(new LocaleReplacementRuleSet());
		GameSettings.Instance.ApplySettings();
		LazySingleton<CursorController>.Instance.Init();
		isUserInitializationFinished = true;
		OnUserAdded();
		LazyAPI.Platform.AddUser();
		LazyAPI.Platform.OnAllControllersDisabled += ShowNoControllersWarning;
	}

	private void OnUserAdded()
	{
		if (!triggerUserAdded)
		{
			GameSettings.Instance.ApplyAudioSettings();
			VoiceOverSettings.IsEnabled = GameSettings.Instance.voiceOverMode == VoiceOverMode.VoiceOver;
			triggerUserAdded = true;
			if (isLazyPreloaderFinished)
			{
				LoadingAction();
			}
		}
	}

	private async void LoadingAction()
	{
		if (isLoadingActionCalled)
		{
			return;
		}
		isLoadingActionCalled = true;
		int attemptId = ++startupAttemptId;
		lazyPreloader.gameObject.SetActive(value: false);
		BackgroundLoading.IsActive = false;
		IsStartupPreloadOverlayActive = true;
		LoadingPipeline.Instance.ClearStage(LoadingStage.PreMainMenu);
		try
		{
			GameShutdown.ThrowIfRequested();
			mainCamera.gameObject.SetActive(value: true);
			ApplyPreloadOverlayCanvasScale();
			await preloadOverlay.Open().AttachExternalCancellation(GameShutdown.Token);
			await preloadOverlay.ShowInitialProgressAndWaitFrame().AttachExternalCancellation(GameShutdown.Token);
			await LoadDeferredConfigsAsync();
			LoadingPipeline lp = LoadingPipeline.Instance;
			startupDownloadHandle = Addressables.DownloadDependenciesAsync("Preload");
			float downloadProgress = 0f;
			IProgress<float> progress = Progress.Create(delegate(float p)
			{
				downloadProgress = p;
			});
			lp.RegisterTask(LoadingStage.PreMainMenu, MeasureTask("Addressables.DownloadDependenciesAsync", startupDownloadHandle.ToUniTask(progress, PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true)), () => downloadProgress, 0.3f);
			startupSceneHandle = Addressables.LoadSceneAsync("MainScene", LoadSceneMode.Additive);
			float sceneLoadProgress = 0f;
			IProgress<float> progress2 = Progress.Create(delegate(float p)
			{
				sceneLoadProgress = p;
			});
			lp.RegisterTask(LoadingStage.PreMainMenu, MeasureTask("Addressables.LoadSceneAsync MAIN_SCENE", startupSceneHandle.ToUniTask(progress2, PlayerLoopTiming.Update, GameShutdown.Token, cancelImmediately: true)), () => sceneLoadProgress, 0.75f);
			await UniTask.DelayFrame(1, PlayerLoopTiming.Update, GameShutdown.Token);
			preloadOverlay.JumpToDownloadDependenciesLoaded();
			await lp.AwaitStage(LoadingStage.PreMainMenu);
			GameShutdown.ThrowIfRequested();
			startupLoadCompleted = true;
			preloadOverlay.JumpToMainSceneLoaded();
			if (startupDownloadHandle.IsValid())
			{
				Addressables.Release(startupDownloadHandle);
				startupDownloadHandle = default(AsyncOperationHandle);
			}
			mainCamera.gameObject.SetActive(value: false);
			await UniTask.NextFrame(GameShutdown.Token);
			await UniTask.NextFrame(GameShutdown.Token);
			await preloadOverlay.CompleteProgressBar().AttachExternalCancellation(GameShutdown.Token);
			IsStartupPreloadOverlayActive = false;
			if (!(MainGame.Instance == null))
			{
				MainGame.Instance.CompleteStartupAfterPreloadOverlay();
				await preloadOverlay.FadeOut().AttachExternalCancellation(GameShutdown.Token);
			}
		}
		catch (OperationCanceledException)
		{
			Debug.Log($"#shutdown# LoadingAction cancelled (attempt:[{attemptId}] current:[{startupAttemptId}] startupLoadCompleted:[{startupLoadCompleted}] isQuitting:[{GameShutdown.IsQuitting}])");
			if (attemptId == startupAttemptId)
			{
				if (GameShutdown.IsQuitting)
				{
					IsStartupPreloadOverlayActive = false;
					return;
				}
				AbortStartup();
				TryContinueStartupAfterResume();
				Debug.Log("#shutdown# LoadingAction cancellation handled");
			}
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

	private void OnGameShutdownResumed()
	{
		TryContinueStartupAfterResume();
	}

	private void AbortStartup()
	{
		Debug.Log($"#shutdown# AbortStartup (startupLoadCompleted:[{startupLoadCompleted}] isQuitting:[{GameShutdown.IsQuitting}])");
		IsStartupPreloadOverlayActive = false;
		if (!(this == null) && !startupLoadCompleted)
		{
			ReleaseStartupHandles();
			LoadingPipeline.Instance.ClearStage(LoadingStage.PreMainMenu);
			isLoadingActionCalled = false;
		}
	}

	private void TryContinueStartupAfterResume()
	{
		if (GameShutdown.IsRequested || this == null)
		{
			return;
		}
		if (!startupLoadCompleted)
		{
			if (!isLoadingActionCalled && isLazyPreloaderFinished)
			{
				LoadingAction();
			}
		}
		else if (!(MainGame.Instance == null))
		{
			FinishStartupOverlayAfterResume();
		}
	}

	private async void FinishStartupOverlayAfterResume()
	{
		if (isFinishingStartupOverlay)
		{
			return;
		}
		isFinishingStartupOverlay = true;
		try
		{
			if (mainCamera != null)
			{
				mainCamera.gameObject.SetActive(value: false);
			}
			bool hasOverlay = preloadOverlay != null && preloadOverlay.gameObject.activeSelf;
			if (hasOverlay)
			{
				await preloadOverlay.CompleteProgressBar().AttachExternalCancellation(GameShutdown.Token);
			}
			IsStartupPreloadOverlayActive = false;
			if (MainGame.Instance != null)
			{
				MainGame.Instance.CompleteStartupAfterPreloadOverlay();
			}
			if (hasOverlay)
			{
				await preloadOverlay.FadeOut().AttachExternalCancellation(GameShutdown.Token);
			}
		}
		catch (OperationCanceledException)
		{
			if (preloadOverlay != null)
			{
				preloadOverlay.gameObject.SetActive(value: false);
			}
		}
		finally
		{
			isFinishingStartupOverlay = false;
		}
	}

	private void ApplyPreloadOverlayCanvasScale()
	{
		if (!(preloadOverlayCanvasScaler == null))
		{
			preloadOverlayCanvasScaler.scaleFactor = ResolutionConfig.GetUiScaleFactor();
		}
	}

	private void ShowUnsupportedResolutionWindow()
	{
		isLoadingActionCalled = true;
		if (lazyPreloader != null)
		{
			lazyPreloader.gameObject.SetActive(value: false);
		}
		if (mainCamera != null)
		{
			mainCamera.gameObject.SetActive(value: true);
		}
		ApplyPreloadOverlayCanvasScale();
		if (preloadOverlay != null)
		{
			preloadOverlay.gameObject.SetActive(value: false);
		}
		if (unsupportedResolutionWindow == null)
		{
			Debug.LogError("UIUnsupportedResolutionWindow is not assigned on ConsolesManager.");
		}
		else
		{
			unsupportedResolutionWindow.Open();
		}
	}

	private void Update()
	{
		if (showNoControllerWarning)
		{
			showNoControllerWarningDelay -= LazyTime.GetUnscaledDeltaTime;
			if (showNoControllerWarningDelay <= 0f)
			{
				showNoControllerWarning = false;
				if (ReInput.controllers.joystickCount == 0)
				{
					ShowNoControllersWarning();
				}
			}
		}
		LazyAPI.Platform.Update();
		if (unfreezeGame)
		{
			unfreezeGame = false;
			UnfreezeGame();
		}
	}

	private bool IsGameFrozen()
	{
		return isFrozen;
	}

	private void FreezeGame()
	{
		isFrozen = true;
		LazyTime.OverrideUnscaledDeltaTime(0f);
		DOTween.PauseAll();
		frozenTimeScale = Time.timeScale;
		Time.timeScale = 0f;
	}

	private void UnfreezeGame()
	{
		isFrozen = false;
		LazyTime.CancelOverrideUnscaledDeltaTime();
		DOTween.PlayAll();
		Time.timeScale = frozenTimeScale;
	}

	private IEnumerator OnFinishedLazyPreloaderCoroutine()
	{
		isLazyPreloaderFinished = true;
		if (triggerUserAdded)
		{
			LoadingAction();
		}
		yield break;
	}

	public void ShowNoControllersWarning()
	{
	}
}
