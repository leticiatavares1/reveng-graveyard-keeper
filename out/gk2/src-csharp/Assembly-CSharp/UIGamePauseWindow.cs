using System;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGamePauseWindow : LazyWindow<LazyWidgetDataBase>
{
	[SerializeField]
	private LazyButton settingsBtn;

	[SerializeField]
	private LazyButton controlsBtn;

	[SerializeField]
	private LazyButton goToMenuBtn;

	[SerializeField]
	private LazyButton restartBtn;

	[SerializeField]
	private GameObject goToMenuSpacing;

	[SerializeField]
	private LazyButton tutorialListBtn;

	public override void Init()
	{
		base.Init();
		bool flag = false;
		controlsBtn.gameObject.SetActive(!flag);
		goToMenuBtn.gameObject.SetActive(!flag);
		if (goToMenuSpacing != null)
		{
			goToMenuSpacing.SetActive(goToMenuBtn.gameObject.activeSelf);
		}
		restartBtn.gameObject.SetActive(flag);
		if (tutorialListBtn == null)
		{
			tutorialListBtn = CreateRuntimeTutorialListButton(base.transform);
		}
		settingsBtn.onClick.AddListener(delegate
		{
			Close();
			UIGameSettingsWindow window2 = LazyUI.GetWindow<UIGameSettingsWindow>();
			window2.onClosed = delegate
			{
				Open(null);
			};
			window2.Open(null);
		});
		settingsBtn.SetCallbacksIntoGamepadNavigationItem();
		if (tutorialListBtn != null)
		{
			tutorialListBtn.onClick.AddListener(OpenTutorialList);
			tutorialListBtn.SetCallbacksIntoGamepadNavigationItem();
		}
		KnowledgeSystem.OnTutorialViewed = (Action<string>)Delegate.Remove(KnowledgeSystem.OnTutorialViewed, new Action<string>(HandleTutorialViewed));
		KnowledgeSystem.OnTutorialViewed = (Action<string>)Delegate.Combine(KnowledgeSystem.OnTutorialViewed, new Action<string>(HandleTutorialViewed));
		UpdateTutorialListButton();
		if (flag)
		{
			return;
		}
		controlsBtn.onClick.AddListener(delegate
		{
			Close();
			UIGameBindingSettingsWindow window = LazyUI.GetWindow<UIGameBindingSettingsWindow>();
			window.onClosed = delegate
			{
				Open(null);
			};
			window.Open(null);
		});
		goToMenuBtn.onClick.AddListener(OnPressedGoToMainMenu);
		goToMenuBtn.SetCallbacksIntoGamepadNavigationItem();
		controlsBtn.SetCallbacksIntoGamepadNavigationItem();
	}

	public override void Open(LazyWidgetDataBase data)
	{
		base.Open(data);
		UpdateTutorialListButton();
		((RectTransform)base.transform).RefreshContentFitter();
	}

	public override void Close()
	{
		base.Close();
	}

	private void OpenTutorialList()
	{
		Close();
		LazyUI.GetWindow<UITutorialListWindow>().Open(new UITutorialListWindowData(UITutorialListOpenSource.PauseWindow));
	}

	private void HandleTutorialViewed(string _)
	{
		UpdateTutorialListButton();
	}

	private void UpdateTutorialListButton()
	{
		if (!(tutorialListBtn == null) && MainGame.Instance?.GameSave?.knowledgeSystem != null)
		{
			tutorialListBtn.gameObject.SetActive(MainGame.Instance.GameSave.knowledgeSystem.HasViewedTutorials());
		}
	}

	private void OnDestroy()
	{
		KnowledgeSystem.OnTutorialViewed = (Action<string>)Delegate.Remove(KnowledgeSystem.OnTutorialViewed, new Action<string>(HandleTutorialViewed));
	}

	private static LazyButton CreateRuntimeTutorialListButton(Transform parent)
	{
		GameObject gameObject = new GameObject("TutorialListButton", typeof(RectTransform));
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		RectTransform obj = (RectTransform)gameObject.transform;
		obj.anchorMin = new Vector2(0.5f, 0.5f);
		obj.anchorMax = new Vector2(0.5f, 0.5f);
		obj.pivot = new Vector2(0.5f, 0.5f);
		obj.anchoredPosition = new Vector2(0f, -110f);
		obj.sizeDelta = new Vector2(220f, 42f);
		Image image = gameObject.AddComponent<Image>();
		image.color = new Color(0.2f, 0.1f, 0.08f, 0.95f);
		LazyButton lazyButton = gameObject.AddComponent<LazyButton>();
		lazyButton.targetGraphic = image;
		gameObject.AddComponent<GamepadNavigationItem>();
		GameObject obj2 = new GameObject("Label", typeof(RectTransform));
		obj2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		RectTransform obj3 = (RectTransform)obj2.transform;
		obj3.anchorMin = Vector2.zero;
		obj3.anchorMax = Vector2.one;
		obj3.offsetMin = Vector2.zero;
		obj3.offsetMax = Vector2.zero;
		TextMeshProUGUI textMeshProUGUI = obj2.AddComponent<TextMeshProUGUI>();
		textMeshProUGUI.text = "Tutorials";
		textMeshProUGUI.alignment = TextAlignmentOptions.Center;
		textMeshProUGUI.fontSize = 18f;
		textMeshProUGUI.color = Color.white;
		return lazyButton;
	}

	public void OnPressedGoToMainMenu()
	{
		LazyUI.GetWindow<UIDialogWindow>().Open(new UIDialogWindowData(LLBase.L("exit_menu_confirm"), LLBase.L("exit_menu_confirm_txt_new"), Yes, LazyUI.GetWindow<UIDialogWindow>().Close)
		{
			ShowCloseButton = true
		});
		void Yes()
		{
			LazyUI.GetWindow<UIDialogWindow>().Close();
			Close();
			MainGame.Instance.GoToMenu();
		}
	}

	protected override void TestDraw()
	{
	}
}
