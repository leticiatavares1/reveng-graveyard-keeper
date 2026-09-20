using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class UILoadingOverlay : LazyWidget<LoadingWindowData>
{
	public enum SaveLoadProgressPhase
	{
		Inactive,
		FadeIn,
		AwaitingScene,
		SceneAsync,
		StallAtHalf,
		PostStallJump,
		PostStallCrawl,
		AfterSceneLoaded,
		Complete
	}

	private const float BACKGROUND_PRELOAD_WEIGHT = 0.3f;

	private const float SCENE_STALL_PROGRESS = 0.38f;

	private const float SCENE_STALL_CAP = 0.65f;

	private const float SCENE_POST_STALL_JUMP = 0.75f;

	private const float SCENE_POST_STALL_CAP = 0.85f;

	private const float SCENE_POST_STALL_STEP = 0.005f;

	private const float SCENE_ASYNC_PROGRESS_END = 0.8f;

	private const float AFTER_SCENE_PROGRESS = 0.9f;

	private const float HITCH_JUMP_UDT_THRESHOLD = 1f;

	private const float PROGRESS_SLIDER_ANIMATION_TIME = 0.2f;

	[SerializeField]
	private RectTransform rootRect;

	[SerializeField]
	private GameObject[] notShowDuringCrossScene;

	[SerializeField]
	private float windowFadeTime = 0.4f;

	[SerializeField]
	private GameObject releaseLogo;

	[SerializeField]
	private GameObject demoLogo;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private Slider progressSlider;

	private Action onAnimationComplete;

	private float progressSliderTargetValue = -1f;

	private bool isShown;

	private bool canUpdateProgress;

	private SaveLoadProgressPhase phase;

	private float phaseProgress;

	private bool hadNormalFramesAtStall;

	private bool stallHitchPending;

	public bool IsShown => isShown;

	protected override void SetData(LoadingWindowData data)
	{
		base.SetData(data);
		onAnimationComplete = data.OnAnimationComplete;
		SetNotShowDuringCrossSceneActive(!data.IsCrossSceneLoading);
	}

	private void Awake()
	{
		UpdateDemoDependentStuff();
	}

	private void UpdateDemoDependentStuff()
	{
		demoLogo.SetActive(value: true);
		releaseLogo.SetActive(value: false);
	}

	public override void Draw(LoadingWindowData data)
	{
		canUpdateProgress = false;
		hadNormalFramesAtStall = false;
		stallHitchPending = false;
		SetPhase(SaveLoadProgressPhase.FadeIn);
		base.Draw(data);
		ApplyProgress(0f);
		isShown = true;
		SetNotDirectlyInGame(active: true);
	}

	public override void Redraw()
	{
		base.Redraw();
		canvasGroup.DOKill();
		canvasGroup.alpha = 0f;
		ApplyProgress(0f);
		canvasGroup.DOFade(1f, windowFadeTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			CompleteShowAsync().Forget();
		});
	}

	protected void Update()
	{
		TickPhase();
		float normalized = EvaluateProgress();
		if (isShown && canUpdateProgress && LoadingPipeline.Instance != null)
		{
			ApplyProgress(normalized);
		}
	}

	public void NotifyAfterSceneWorkStarted()
	{
		SetPhase(SaveLoadProgressPhase.AfterSceneLoaded);
	}

	public override void Hide()
	{
		if (!isShown)
		{
			base.Hide();
			return;
		}
		isShown = false;
		canUpdateProgress = false;
		onAnimationComplete = null;
		progressSlider?.DOKill();
		canvasGroup.DOKill();
		SetNotDirectlyInGame(active: false);
		SetPhase(SaveLoadProgressPhase.Complete);
		canvasGroup.DOFade(0f, windowFadeTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			base.Hide();
		});
	}

	private async UniTaskVoid CompleteShowAsync()
	{
		canUpdateProgress = true;
		await UniTask.NextFrame();
		onAnimationComplete?.Invoke();
	}

	private void TickPhase()
	{
		if (!isShown)
		{
			return;
		}
		switch (phase)
		{
		case SaveLoadProgressPhase.FadeIn:
			if (canUpdateProgress)
			{
				SetPhase(HasGameplaySceneLoad() ? SaveLoadProgressPhase.SceneAsync : SaveLoadProgressPhase.AwaitingScene);
			}
			break;
		case SaveLoadProgressPhase.AwaitingScene:
			if (IsGameplaySceneLoadComplete())
			{
				SetPhase(SaveLoadProgressPhase.Complete);
			}
			else if (HasGameplaySceneLoad())
			{
				SetPhase(SaveLoadProgressPhase.SceneAsync);
			}
			break;
		case SaveLoadProgressPhase.SceneAsync:
			if (IsGameplaySceneLoadComplete())
			{
				SetPhase(SaveLoadProgressPhase.Complete);
			}
			else if (GetSceneAsyncMappedProgress() >= 0.35999998f)
			{
				SetPhase(SaveLoadProgressPhase.StallAtHalf);
			}
			break;
		case SaveLoadProgressPhase.StallAtHalf:
			if (IsGameplaySceneLoadComplete())
			{
				SetPhase(SaveLoadProgressPhase.Complete);
				break;
			}
			if (Time.unscaledDeltaTime >= 1f)
			{
				if (hadNormalFramesAtStall)
				{
					stallHitchPending = true;
				}
				break;
			}
			hadNormalFramesAtStall = true;
			phaseProgress = Mathf.Min(phaseProgress + 0.005f, 0.65f);
			if (stallHitchPending)
			{
				SetPhase(SaveLoadProgressPhase.PostStallJump);
			}
			break;
		case SaveLoadProgressPhase.PostStallJump:
			SetPhase(SaveLoadProgressPhase.PostStallCrawl);
			break;
		case SaveLoadProgressPhase.PostStallCrawl:
			phaseProgress = Mathf.Min(phaseProgress + 0.005f, 0.85f);
			if (IsGameplaySceneLoadComplete())
			{
				SetPhase(SaveLoadProgressPhase.Complete);
			}
			break;
		case SaveLoadProgressPhase.AfterSceneLoaded:
			if (IsGameplaySceneLoadComplete())
			{
				SetPhase(SaveLoadProgressPhase.Complete);
			}
			break;
		}
	}

	private float EvaluateProgress()
	{
		switch (phase)
		{
		case SaveLoadProgressPhase.FadeIn:
		case SaveLoadProgressPhase.AwaitingScene:
			return GetBackgroundHoldProgress();
		case SaveLoadProgressPhase.SceneAsync:
			return GetSceneAsyncMappedProgress();
		case SaveLoadProgressPhase.StallAtHalf:
			return phaseProgress;
		case SaveLoadProgressPhase.PostStallJump:
			return 0.75f;
		case SaveLoadProgressPhase.PostStallCrawl:
			return phaseProgress;
		case SaveLoadProgressPhase.AfterSceneLoaded:
			return 0.9f;
		case SaveLoadProgressPhase.Complete:
			return 1f;
		default:
			return 0f;
		}
	}

	private void SetPhase(SaveLoadProgressPhase next)
	{
		if (next == SaveLoadProgressPhase.FadeIn || next > phase)
		{
			phase = next;
			switch (next)
			{
			case SaveLoadProgressPhase.FadeIn:
				hadNormalFramesAtStall = false;
				stallHitchPending = false;
				phaseProgress = 0f;
				break;
			case SaveLoadProgressPhase.StallAtHalf:
				phaseProgress = 0.38f;
				break;
			case SaveLoadProgressPhase.PostStallJump:
				phaseProgress = 0.75f;
				break;
			case SaveLoadProgressPhase.AfterSceneLoaded:
				phaseProgress = 0.9f;
				break;
			case SaveLoadProgressPhase.Complete:
				phaseProgress = 1f;
				break;
			case SaveLoadProgressPhase.AwaitingScene:
			case SaveLoadProgressPhase.SceneAsync:
			case SaveLoadProgressPhase.PostStallCrawl:
				break;
			}
		}
	}

	private void ApplyProgress(float normalized01)
	{
		if (progressSlider == null)
		{
			return;
		}
		normalized01 = Mathf.Clamp01(normalized01);
		if (!Mathf.Approximately(progressSliderTargetValue, normalized01))
		{
			progressSliderTargetValue = normalized01;
			progressSlider.DOKill();
			SaveLoadProgressPhase saveLoadProgressPhase = phase;
			if (saveLoadProgressPhase == SaveLoadProgressPhase.StallAtHalf || saveLoadProgressPhase == SaveLoadProgressPhase.PostStallJump || saveLoadProgressPhase == SaveLoadProgressPhase.PostStallCrawl || Mathf.Approximately(normalized01, 0f))
			{
				progressSlider.value = normalized01;
			}
			else
			{
				progressSlider.DOValue(normalized01, 0.2f).SetUpdate(isIndependentUpdate: true);
			}
		}
	}

	private static float GetBackgroundHoldProgress()
	{
		LoadingPipeline instance = LoadingPipeline.Instance;
		if (instance == null)
		{
			return 0.3f;
		}
		if (!instance.IsStageComplete(LoadingStage.BackgroundPreload))
		{
			return instance.GetStageProgress(LoadingStage.BackgroundPreload) * 0.3f;
		}
		return 0.3f;
	}

	private static float GetSceneAsyncMappedProgress()
	{
		LoadingPipeline instance = LoadingPipeline.Instance;
		if (instance == null || !instance.HasStage(LoadingStage.GameplaySceneLoad))
		{
			return 0.3f;
		}
		float stageProgress = instance.GetStageProgress(LoadingStage.GameplaySceneLoad);
		if (stageProgress >= 0.8f)
		{
			return 0.38f;
		}
		return Mathf.Lerp(0.3f, 0.38f, stageProgress / 0.8f);
	}

	private static bool HasGameplaySceneLoad()
	{
		if (LoadingPipeline.Instance != null)
		{
			return LoadingPipeline.Instance.HasStage(LoadingStage.GameplaySceneLoad);
		}
		return false;
	}

	private static bool IsGameplaySceneLoadComplete()
	{
		if (LoadingPipeline.Instance != null)
		{
			return LoadingPipeline.Instance.IsStageComplete(LoadingStage.GameplaySceneLoad);
		}
		return false;
	}

	private void SetNotShowDuringCrossSceneActive(bool isActive)
	{
		if (notShowDuringCrossScene == null)
		{
			return;
		}
		GameObject[] array = notShowDuringCrossScene;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(isActive);
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

	protected override void TestDraw()
	{
	}
}
