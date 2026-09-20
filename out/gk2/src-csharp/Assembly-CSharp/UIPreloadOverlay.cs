using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPreloadOverlay : MonoBehaviour
{
	public enum StartupProgressPhase
	{
		Inactive,
		FadeIn,
		InitialVisible,
		PreMainMenuCrawl,
		DownloadLoaded,
		MainSceneLoaded,
		Closing
	}

	[SerializeField]
	private Slider progressSlider;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private float fadeInTime = 0.3f;

	[SerializeField]
	private float fadeOutTime = 0.5f;

	[SerializeField]
	private float progressSliderAnimationTime = 0.2f;

	[SerializeField]
	private float finalProgressSliderAnimationTime = 0.2f;

	[SerializeField]
	private GameObject releaseLogo;

	[SerializeField]
	private GameObject demoLogo;

	private const float INITIAL_VISIBLE_PROGRESS = 0.15f;

	private const float PRE_MAIN_MENU_PROGRESS_STEP = 0.002f;

	private const float PRE_MAIN_MENU_PROGRESS_CAP = 0.6f;

	public const float DOWNLOAD_DEPENDENCIES_WEIGHT = 0.3f;

	public const float MAIN_SCENE_LOAD_WEIGHT = 0.75f;

	private float progressSliderTargetValue = -1f;

	private LoadingStage loadingStage;

	private bool isUpdateEnabled;

	private StartupProgressPhase phase;

	public float FadeOutTime => fadeOutTime;

	private void Awake()
	{
		UpdateDemoDependentStuff();
	}

	public async UniTask Open()
	{
		base.gameObject.SetActive(value: true);
		SetPhase(StartupProgressPhase.FadeIn);
		SetProgressImmediate(0f);
		canvasGroup.DOKill();
		canvasGroup.alpha = 0f;
		UniTaskCompletionSource fadeCompletion = new UniTaskCompletionSource();
		canvasGroup.DOFade(1f, fadeInTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			fadeCompletion.TrySetResult();
		})
			.OnKill(delegate
			{
				fadeCompletion.TrySetResult();
			});
		await fadeCompletion.Task;
		isUpdateEnabled = true;
	}

	public async UniTask ShowInitialProgressAndWaitFrame()
	{
		SetPhase(StartupProgressPhase.InitialVisible);
		SetProgressImmediate(0.15f);
		await UniTask.NextFrame();
	}

	public void JumpToDownloadDependenciesLoaded()
	{
		SetPhase(StartupProgressPhase.DownloadLoaded);
		SetProgressImmediate(Mathf.Max(progressSliderTargetValue, 0.3f));
	}

	public void JumpToMainSceneLoaded()
	{
		SetPhase(StartupProgressPhase.MainSceneLoaded);
		SetProgressImmediate(0.75f);
	}

	protected void Update()
	{
		TickPhase();
	}

	private void TickPhase()
	{
		if (!isUpdateEnabled)
		{
			return;
		}
		LoadingPipeline instance = LoadingPipeline.Instance;
		bool flag = instance?.HasStage(loadingStage) ?? false;
		bool flag2 = instance?.IsStageComplete(loadingStage) ?? false;
		switch (phase)
		{
		case StartupProgressPhase.InitialVisible:
			if (flag)
			{
				SetPhase(StartupProgressPhase.PreMainMenuCrawl);
			}
			break;
		case StartupProgressPhase.PreMainMenuCrawl:
		case StartupProgressPhase.DownloadLoaded:
			if (flag2)
			{
				JumpToMainSceneLoaded();
			}
			else
			{
				SetProgressImmediate(Mathf.Min(progressSliderTargetValue + 0.002f, 0.6f));
			}
			break;
		case StartupProgressPhase.MainSceneLoaded:
			if (flag2)
			{
				SetProgressImmediate(0.75f);
			}
			break;
		}
	}

	private void SetPhase(StartupProgressPhase next)
	{
		if (next != phase && (next == StartupProgressPhase.FadeIn || next == StartupProgressPhase.Inactive || next > phase))
		{
			phase = next;
		}
	}

	private void UpdateDemoDependentStuff()
	{
		demoLogo.SetActive(value: true);
		releaseLogo.SetActive(value: false);
	}

	private void SetProgressImmediate(float normalized01)
	{
		normalized01 = Mathf.Clamp01(normalized01);
		progressSlider.DOKill();
		progressSliderTargetValue = normalized01;
		progressSlider.value = normalized01;
		if (progressSlider.fillRect != null)
		{
			progressSlider.fillRect.gameObject.SetActive(normalized01 > 0f);
		}
	}

	public RectTransform GetActiveLogoRect()
	{
		GameObject activeLogoRoot = GetActiveLogoRoot();
		if (activeLogoRoot == null)
		{
			return null;
		}
		Transform transform = activeLogoRoot.transform.Find("Logo");
		if (!(transform != null))
		{
			return (RectTransform)activeLogoRoot.transform;
		}
		return (RectTransform)transform;
	}

	public void HideActiveLogo()
	{
		GameObject activeLogoRoot = GetActiveLogoRoot();
		if (activeLogoRoot != null)
		{
			activeLogoRoot.SetActive(value: false);
		}
	}

	public async UniTask CompleteProgressBar()
	{
		isUpdateEnabled = false;
		SetPhase(StartupProgressPhase.Closing);
		UniTaskCompletionSource sliderCompletion = new UniTaskCompletionSource();
		progressSlider.DOKill();
		progressSlider.DOValue(1f, finalProgressSliderAnimationTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			sliderCompletion.TrySetResult();
		})
			.OnKill(delegate
			{
				sliderCompletion.TrySetResult();
			});
		await sliderCompletion.Task;
	}

	public async UniTask FadeOut()
	{
		UniTaskCompletionSource fadeCompletion = new UniTaskCompletionSource();
		canvasGroup.DOKill();
		canvasGroup.DOFade(0f, fadeOutTime).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
			fadeCompletion.TrySetResult();
		})
			.OnKill(delegate
			{
				fadeCompletion.TrySetResult();
			});
		await fadeCompletion.Task;
		SetPhase(StartupProgressPhase.Inactive);
	}

	public async UniTask Close()
	{
		await CompleteProgressBar();
		await FadeOut();
	}

	private GameObject GetActiveLogoRoot()
	{
		return demoLogo;
	}
}
