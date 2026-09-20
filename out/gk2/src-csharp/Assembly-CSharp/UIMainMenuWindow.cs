using System;
using System.Collections.Generic;
using DG.Tweening;
using LazyBearTechnology;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class UIMainMenuWindow : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private LazyButton startNewGameButton;

	[SerializeField]
	private LazyButton continueGameButton;

	[SerializeField]
	private LazyButton loadGameButton;

	[SerializeField]
	private LazyButton gameSettingsButton;

	[SerializeField]
	private LazyButton creditsButton;

	[SerializeField]
	private LazyButton exitGameButton;

	[SerializeField]
	private LazyButton consolesGameButton;

	[Header("Steam Demo")]
	[SerializeField]
	private LazyButton preorderButton;

	[SerializeField]
	private LazyButton leaveReviewButton;

	[SerializeField]
	private LazyButton discordButton;

	[SerializeField]
	private LazyButton twitterButton;

	[SerializeField]
	private LazyButton bilibiliButton;

	[SerializeField]
	private GameObject releaseLogo;

	[SerializeField]
	private GameObject demoLogo;

	[SerializeField]
	private float logoMoveDuration = 0.5f;

	[SerializeField]
	private Ease logoMoveEase = Ease.OutCubic;

	[SerializeField]
	private float buttonsMoveDuration = 0.35f;

	[SerializeField]
	private float buttonsMoveOffset = 18f;

	[SerializeField]
	private Ease buttonsMoveEase = Ease.OutCubic;

	[SerializeField]
	private float buttonsStartAtLogoProgress = 0.8f;

	private UIPreloadOverlay pendingPreloaderOverlay;

	private Action onPreloaderIntroComplete;

	private Sequence introSequence;

	private RectTransform introLogo;

	private Vector2 introLogoRestAnchored;

	private CanvasGroup introButtonsGroup;

	private CanvasGroup introLogoGroup;

	private readonly List<(RectTransform rect, Vector2 rest)> introButtonRests = new List<(RectTransform, Vector2)>();

	public override void Init()
	{
		UpdateDemoDependentStuff();
		startNewGameButton.onClick.AddListener(OnStartNewGameButtonClicked);
		gameSettingsButton.onClick.AddListener(OnGameSettingsButtonClicked);
		exitGameButton.onClick.AddListener(OnExitGameButtonClicked);
		continueGameButton.onClick.AddListener(OnContinueButtonClicked);
		loadGameButton.onClick.AddListener(OnLoadButtonClicked);
		creditsButton.onClick.AddListener(OnCreditsButtonClicked);
		if (consolesGameButton != null)
		{
			consolesGameButton.onClick.AddListener(OnConsolesGameButtonClicked);
		}
		startNewGameButton.SetCallbacksIntoGamepadNavigationItem();
		gameSettingsButton.SetCallbacksIntoGamepadNavigationItem();
		exitGameButton.SetCallbacksIntoGamepadNavigationItem();
		continueGameButton.SetCallbacksIntoGamepadNavigationItem();
		loadGameButton.SetCallbacksIntoGamepadNavigationItem();
		creditsButton.SetCallbacksIntoGamepadNavigationItem();
		if (consolesGameButton != null)
		{
			consolesGameButton.SetCallbacksIntoGamepadNavigationItem();
		}
		SetupSteamDemoButtons();
		exitGameButton.gameObject.SetActive(value: true);
		base.Init();
	}

	private void UpdateDemoDependentStuff()
	{
		demoLogo.SetActive(value: true);
		releaseLogo.SetActive(value: false);
	}

	private void SetupSteamDemoButtons()
	{
		UpdateSteamDemoButtonsVisibility();
		SetupNavigableSteamDemoButton(preorderButton, OnPreorderButtonClicked);
		SetupNavigableSteamDemoButton(leaveReviewButton, OnLeaveReviewButtonClicked);
		SetupSocialSteamDemoButton(discordButton, OnDiscordButtonClicked);
		SetupSocialSteamDemoButton(twitterButton, OnTwitterButtonClicked);
		SetupSocialSteamDemoButton(bilibiliButton, OnBilibiliButtonClicked);
	}

	private void UpdateSteamDemoButtonsVisibility()
	{
		SetButtonActive(preorderButton, active: true);
		SetButtonActive(leaveReviewButton, active: true);
		SetButtonActive(discordButton, active: true);
		SetButtonActive(twitterButton, active: true);
		SetButtonActive(bilibiliButton, IsChineseLanguageSelected());
	}

	private static void SetButtonActive(LazyButton button, bool active)
	{
		if (button != null)
		{
			button.gameObject.SetActive(active);
		}
	}

	private static void SetupNavigableSteamDemoButton(LazyButton button, Action onClick)
	{
		if (!(button == null))
		{
			button.onClick.AddListener(delegate
			{
				onClick();
			});
			button.SetCallbacksIntoGamepadNavigationItem();
		}
	}

	private static void SetupSocialSteamDemoButton(LazyButton button, Action onClick)
	{
		if (!(button == null))
		{
			button.onClick.AddListener(delegate
			{
				onClick();
			});
			GamepadNavigationItem[] componentsInChildren = button.GetComponentsInChildren<GamepadNavigationItem>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Active = false;
			}
		}
	}

	private void OnPreorderButtonClicked()
	{
		UIDemoEndWindow.OpenFullGameInStore();
	}

	private void OnLeaveReviewButtonClicked()
	{
		SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/recommended/recommendgame/5075680");
	}

	private void OnDiscordButtonClicked()
	{
		Application.OpenURL("https://discord.gg/lazybeargames");
	}

	private void OnTwitterButtonClicked()
	{
		Application.OpenURL("https://x.com/lazybeargames");
	}

	private void OnBilibiliButtonClicked()
	{
		Application.OpenURL("https://space.bilibili.com/3707062611609883");
	}

	private static bool IsChineseLanguageSelected()
	{
		string currentLang = LLBase.CurrentLang;
		if (!(currentLang == "zh_cn"))
		{
			return currentLang == "zh_cht";
		}
		return true;
	}

	public override void Open(LazyWidgetDataBase data)
	{
		DLCEngine.ResetDLCStateCached();
		base.Open(data);
		continueGameButton.SetKeepPressed(keepPressed: false);
		continueGameButton.interactable = true;
		SaveSlotData lastSaveSlot = SaveSystem.GetLastSaveSlot();
		bool active = false;
		if (lastSaveSlot != null && !lastSaveSlot.repValue)
		{
			active = true;
		}
		continueGameButton.gameObject.SetActive(active);
		loadGameButton.gameObject.SetActive(lastSaveSlot != null);
		bool isLimitedSaveSlotsEnabled = SaveSystem.IsLimitedSaveSlotsEnabled;
		startNewGameButton.gameObject.SetActive(!isLimitedSaveSlotsEnabled);
		loadGameButton.gameObject.SetActive(!isLimitedSaveSlotsEnabled && loadGameButton.gameObject.activeSelf);
		if (consolesGameButton != null)
		{
			consolesGameButton.gameObject.SetActive(isLimitedSaveSlotsEnabled);
		}
		UpdateSteamDemoButtonsVisibility();
		if (LazyInput.IsGamepadActive)
		{
			base.GamepadNavigationController.ReinitItems(focusOnFirstActive: true);
		}
		((RectTransform)base.transform).RefreshContentFitterAndDisable();
		if (pendingPreloaderOverlay != null)
		{
			UIPreloadOverlay overlay = pendingPreloaderOverlay;
			pendingPreloaderOverlay = null;
			PlayPreloaderIntro(overlay);
		}
	}

	public void OpenFromPreloader(UIPreloadOverlay overlay, Action onComplete)
	{
		pendingPreloaderOverlay = overlay;
		onPreloaderIntroComplete = onComplete;
		Open(null);
	}

	protected override void HideWindow()
	{
		CompleteIntroImmediate();
		base.HideWindow();
	}

	public bool IsContinueButtonWillBeActive()
	{
		return SaveSystem.GetLastSaveSlot() != null;
	}

	public void OnStartNewGameButtonClicked()
	{
		Debug.Log("OnStartNewGameButtonClicked");
		MainGame.Instance.StartNewGame();
		Close();
	}

	public void OnContinueButtonClicked()
	{
		SaveSlotData saveSlotData = SaveSystem.GetActiveSaveData();
		Debug.Log("OnContinueButtonClicked saveSlotData:[" + saveSlotData.slotName + "]");
		continueGameButton.SetKeepPressed(keepPressed: true);
		continueGameButton.interactable = false;
		UILoadingOverlay overlay = LazyUI.Get<UILoadingOverlay>();
		overlay.Draw(new LoadingWindowData(MainGame.EntrySceneToLoad, delegate
		{
			SaveSystem.Load(saveSlotData, delegate(GameSave s)
			{
				if (s != null)
				{
					MainGame.Instance.ContinueGame(saveSlotData, s);
					Close();
				}
				else
				{
					overlay.Hide();
					continueGameButton.SetKeepPressed(keepPressed: false);
					continueGameButton.interactable = true;
				}
			});
		}));
	}

	public void OnLoadButtonClicked()
	{
		Close();
		LazyUI.GetWindow<UISaveSlotsWindow>().Open(null);
	}

	public void OnConsolesGameButtonClicked()
	{
		Close();
		LazyUI.GetWindow<UISaveSlotsWindowLimited>().Open(null);
	}

	private void OnStartHostButtonClicked()
	{
		Close();
		LazyUI.GetWindow<UIStartHostGameWindow>().Open(null);
	}

	private void OnConnectToHostButtonClicked()
	{
		Close();
		LazyUI.GetWindow<UIConnectToHostGameWindow>().Open(null);
	}

	private void OnGameSettingsButtonClicked()
	{
		Close();
		UIGameSettingsWindow window = LazyUI.GetWindow<UIGameSettingsWindow>();
		window.onClosed = delegate
		{
			Open(null);
		};
		window.Open(null);
	}

	private void OnCreditsButtonClicked()
	{
		MainGame.Instance.SetMainMenuInfoPanelEnabled(isEnabled: false);
		Close();
		LazyUI.GetWindow<UICreditsWindow>().Open(null);
	}

	private void OnExitGameButtonClicked()
	{
		GameShutdown.RequestQuit();
		Application.Quit();
	}

	private void PlayPreloaderIntro(UIPreloadOverlay overlay)
	{
		RectTransform activeLogoRect = GetActiveLogoRect();
		RectTransform rectTransform = ((activeLogoRect != null) ? (activeLogoRect.parent as RectTransform) : null);
		RectTransform rectTransform2 = ((rectTransform != null) ? (rectTransform.parent as RectTransform) : null);
		if (activeLogoRect == null || rectTransform == null || rectTransform2 == null)
		{
			CompleteIntroImmediate();
			return;
		}
		ResetIntroVisuals();
		introLogo = activeLogoRect;
		introLogoRestAnchored = activeLogoRect.anchoredPosition;
		introLogoGroup = GetOrAddCanvasGroup(rectTransform.gameObject);
		introLogoGroup.ignoreParentGroups = true;
		introLogoGroup.alpha = 1f;
		introButtonsGroup = GetOrAddCanvasGroup(rectTransform2.gameObject);
		introButtonsGroup.alpha = 0f;
		introButtonsGroup.blocksRaycasts = false;
		introButtonRests.Clear();
		for (int i = 0; i < rectTransform2.childCount; i++)
		{
			RectTransform rectTransform3 = rectTransform2.GetChild(i) as RectTransform;
			if (!(rectTransform3 == null) && !(rectTransform3 == rectTransform) && rectTransform3.gameObject.activeSelf)
			{
				introButtonRests.Add((rectTransform3, rectTransform3.anchoredPosition));
				rectTransform3.anchoredPosition -= new Vector2(0f, buttonsMoveOffset);
			}
		}
		Canvas.ForceUpdateCanvases();
		RectTransform rectTransform4 = ((overlay != null) ? overlay.GetActiveLogoRect() : null);
		if (rectTransform4 != null)
		{
			MatchPivotToScreenOf(activeLogoRect, rectTransform4);
		}
		overlay?.HideActiveLogo();
		float num = logoMoveDuration;
		float atPosition = num * Mathf.Clamp01(buttonsStartAtLogoProgress);
		introSequence = DOTween.Sequence().SetUpdate(isIndependentUpdate: true).SetLink(base.gameObject, LinkBehaviour.KillOnDisable);
		introSequence.Join(activeLogoRect.DOAnchorPos(introLogoRestAnchored, num).SetEase(logoMoveEase));
		introSequence.Insert(atPosition, introButtonsGroup.DOFade(1f, buttonsMoveDuration).SetEase(Ease.OutQuad));
		for (int j = 0; j < introButtonRests.Count; j++)
		{
			var (target, endValue) = introButtonRests[j];
			introSequence.Insert(atPosition, target.DOAnchorPos(endValue, buttonsMoveDuration).SetEase(buttonsMoveEase));
		}
		introSequence.OnComplete(delegate
		{
			introSequence = null;
			introLogo = null;
			introButtonRests.Clear();
			if (introButtonsGroup != null)
			{
				introButtonsGroup.blocksRaycasts = true;
			}
			NotifyIntroCompleted();
		});
		introSequence.OnKill(delegate
		{
			introSequence = null;
			NotifyIntroCompleted();
		});
	}

	private void CompleteIntroImmediate()
	{
		ResetIntroVisuals();
		NotifyIntroCompleted();
	}

	private void ResetIntroVisuals()
	{
		introSequence?.Kill();
		introSequence = null;
		if (introLogo != null)
		{
			introLogo.anchoredPosition = introLogoRestAnchored;
		}
		for (int i = 0; i < introButtonRests.Count; i++)
		{
			var (rectTransform, anchoredPosition) = introButtonRests[i];
			if (rectTransform != null)
			{
				rectTransform.anchoredPosition = anchoredPosition;
			}
		}
		introButtonRests.Clear();
		if (introButtonsGroup != null)
		{
			introButtonsGroup.alpha = 1f;
			introButtonsGroup.blocksRaycasts = true;
		}
		if (introLogoGroup != null)
		{
			introLogoGroup.alpha = 1f;
		}
		introLogo = null;
	}

	private void NotifyIntroCompleted()
	{
		Action action = onPreloaderIntroComplete;
		onPreloaderIntroComplete = null;
		action?.Invoke();
	}

	private RectTransform GetActiveLogoRect()
	{
		GameObject gameObject = (releaseLogo.activeSelf ? releaseLogo : demoLogo);
		if (!(gameObject != null))
		{
			return null;
		}
		return (RectTransform)gameObject.transform;
	}

	private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
	{
		if (!target.TryGetComponent<CanvasGroup>(out var component))
		{
			return target.AddComponent<CanvasGroup>();
		}
		return component;
	}

	private static void MatchPivotToScreenOf(RectTransform target, RectTransform source)
	{
		Camera canvasCamera = GetCanvasCamera(source);
		Camera canvasCamera2 = GetCanvasCamera(target);
		Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvasCamera, source.position);
		RectTransform rectTransform = target.parent as RectTransform;
		if (!(rectTransform == null) && RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPoint, canvasCamera2, out var worldPoint))
		{
			target.position = worldPoint;
		}
	}

	private static Camera GetCanvasCamera(RectTransform rect)
	{
		Canvas componentInParent = rect.GetComponentInParent<Canvas>();
		if (componentInParent == null)
		{
			return null;
		}
		componentInParent = componentInParent.rootCanvas;
		if (componentInParent.renderMode != 0)
		{
			return componentInParent.worldCamera;
		}
		return null;
	}

	protected override void TestDraw()
	{
	}
}
