using System;
using System.Collections;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using Rewired.Integration.UnityUI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GUIElements : MonoBehaviour
{
	private readonly string[] windowsWithSmallVersion = new string[2] { "CharacterWindow", "UIMapWindow" };

	public const int OBJECT_BUBBLE_DEFAULT_SORTING_ORDER_VALUE = 40;

	public const int SLEEP_BLACKOUT_DEFAULT_SORTING_ORDER_VALUE = 49;

	public const int HUD_DEFAULT_SORTING_ORDER_VALUE = 50;

	public const int TUTORIAL_DEFAULT_ARROW_SORTING = 51;

	public const int CINEMATIC_DEFAULT_SORTING_ORDER_VALUE = 300;

	public const int SPEECH_DEFAULT_SORTING_ORDER_VALUE = 350;

	public const int TOOLTIP_DEFAULT_SORTING_ORDER_VALUE = 700;

	public const int BLACKOUT_DEFAULT_SORTING_ORDER_VALUE = 800;

	public const int OVER_BLACKOUT_SORTING_ORDER_VALUE = 900;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private CanvasScaler canvasScaler;

	[SerializeField]
	private UIFitter uiFitter;

	[SerializeField]
	private GamepadDynamicSelector dynamicSelector;

	[SerializeField]
	public RewiredStandaloneInputModule standaloneInputModule;

	[SerializeField]
	private Transform worldMin;

	[SerializeField]
	private Transform worldMax;

	[Space]
	[SerializeField]
	private WorldZoneWidget worldZoneWidget;

	[Space]
	[SerializeField]
	private UINpcWidget npcWidget;

	private bool isVisuallyHidden;

	private Dictionary<ILazyGUIElement, bool> storedGroupStates = new Dictionary<ILazyGUIElement, bool>();

	private static GUIElements instance;

	private Dictionary<string, LazyWidgetBase> smallSizeWindows = new Dictionary<string, LazyWidgetBase>();

	private Dictionary<string, LazyWidgetBase> bigSizeWindows = new Dictionary<string, LazyWidgetBase>();

	private RectTransform root;

	private UIWindowSizeType uiWindowSizeType;

	private Coroutine delayedResolutionApply;

	public Transform WorldMin => worldMin;

	public Transform WorldMax => worldMax;

	public static GUIElements Instance
	{
		get
		{
			if (instance == null)
			{
				instance = UnityEngine.Object.FindObjectOfType<GUIElements>();
			}
			return instance;
		}
	}

	public RectTransform Root
	{
		get
		{
			if (root == null)
			{
				root = GetComponent<RectTransform>();
			}
			return root;
		}
	}

	public WorldZoneWidget WorldZoneWidget => worldZoneWidget;

	public UINpcWidget NpcWidget => npcWidget;

	public UIWindowSizeType UIWindowSizeType => uiWindowSizeType;

	public static event Action<UIWindowSizeType> OnWindowSizeTypeChanged;

	public void Initialize()
	{
		OnResolutionChanged(GameSettings.Instance.GetResolutionIntVector2());
		GameSettings.OnResolutionChanged += OnResolutionChanged;
		LazySingleton<LazyWidgetPrefabContainer>.Instance.Init();
		LazyGameKeyTip.InitSelectAndBackLocales("tip_select", "tip_back");
		dynamicSelector.Init();
		dynamicSelector.SetOnUpdateCheckActivity(OnUpdateDynamicSelectorActivity);
		LazyInput.OnInputChanged += OnInputChanged;
		LazyInput.ClearAllKeysDown();
		LazyInput.OnInputChanged += UpdateRewiredInputType;
		standaloneInputModule.ForceInitEventSystem();
		worldZoneWidget.gameObject.SetActive(value: false);
		MainGame.OnGoToMainMenu = (Action)Delegate.Combine(MainGame.OnGoToMainMenu, (Action)delegate
		{
			worldZoneWidget.gameObject.SetActive(value: false);
		});
		UpdateRewiredInputType();
		LazyUI.SetWindowLoadAction(LoadWindowFrom);
	}

	public void Clear()
	{
		UIObjectBubbleManager.Instance.Clear();
	}

	public void OnResolutionChanged(IntVector2 res)
	{
		ApplyUiForResolution();
		if (base.isActiveAndEnabled)
		{
			if (delayedResolutionApply != null)
			{
				StopCoroutine(delayedResolutionApply);
			}
			delayedResolutionApply = StartCoroutine(ApplyUiForResolutionNextFrame());
		}
	}

	private IEnumerator ApplyUiForResolutionNextFrame()
	{
		yield return null;
		ApplyUiForResolution();
		delayedResolutionApply = null;
	}

	private void ApplyUiForResolution()
	{
		float uiScaleFactor = ResolutionConfig.GetUiScaleFactor();
		canvasScaler.scaleFactor = uiScaleFactor;
		if (LazyUI.IsInitialized)
		{
			LazyUI.SetCanvasScaleFactor(uiScaleFactor);
			LazyUI.SetSafeZones(Screen.safeArea);
		}
		if (ResolutionConfig.currentResolution != null)
		{
			SetUIMode(ResolutionConfig.currentResolution.WindowSizeType);
		}
		Canvas.ForceUpdateCanvases();
		if (LazyWindowsStackController.ActiveWindow != null)
		{
			RectTransform rectTransform = (RectTransform)LazyWindowsStackController.ActiveWindow.transform;
			if (LazyWindowsStackController.ActiveWindow is UIMainMenuWindow)
			{
				rectTransform.RefreshContentFitterAndDisable();
			}
			else
			{
				rectTransform.RefreshContentFitter();
			}
		}
		if (worldZoneWidget != null && worldZoneWidget.gameObject.activeInHierarchy)
		{
			((RectTransform)worldZoneWidget.transform).RefreshContentFitter();
		}
		if (LazyUI.IsInitialized)
		{
			HUD hUD = LazyUI.Get<HUD>();
			if (hUD != null)
			{
				((RectTransform)hUD.transform).RefreshContentFitter();
			}
		}
	}

	public void SetUIMode(UIWindowSizeType uiWindowSizeType)
	{
		Debug.Log("SetUIMode: " + uiWindowSizeType);
		if (this.uiWindowSizeType != uiWindowSizeType)
		{
			if (this.uiWindowSizeType == UIWindowSizeType.Big)
			{
				LazyUI.ClearWindowsFromCache(bigSizeWindows.Keys.ToList());
				bigSizeWindows.Clear();
			}
			else
			{
				LazyUI.ClearWindowsFromCache(smallSizeWindows.Keys.ToList());
				smallSizeWindows.Clear();
			}
			this.uiWindowSizeType = uiWindowSizeType;
			GUIElements.OnWindowSizeTypeChanged?.Invoke(uiWindowSizeType);
		}
	}

	public void SetVisibilityState(bool isVisible)
	{
		if (isVisible == !isVisuallyHidden)
		{
			return;
		}
		foreach (ILazyGUIElement item in new List<ILazyGUIElement>
		{
			LazyUI.Get<Bubble>(),
			LazyUI.Get<UICinematic>(),
			LazyUI.Get<UIFade>(),
			LazyUI.Get<UISleepFade>()
		})
		{
			if (item is MonoBehaviour monoBehaviour && monoBehaviour.TryGetComponent<CanvasGroup>(out var component))
			{
				if (!isVisible)
				{
					storedGroupStates.Add(item, component.ignoreParentGroups);
					component.ignoreParentGroups = true;
				}
				else
				{
					component.ignoreParentGroups = storedGroupStates[item];
					storedGroupStates.Remove(item);
				}
			}
		}
		if (!isVisible)
		{
			CinematicsTextWidget[] componentsInChildren = Root.GetComponentsInChildren<CinematicsTextWidget>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].TryGetComponent<CanvasGroup>(out var component2))
				{
					component2.ignoreParentGroups = true;
				}
			}
		}
		canvasGroup.alpha = (isVisible ? 1f : 0f);
		isVisuallyHidden = !isVisible;
	}

	public LazyWidgetBase LoadWindowFrom(string windowTypeName)
	{
		Dictionary<string, LazyWidgetBase> dictionary;
		string key;
		if (uiWindowSizeType == UIWindowSizeType.Small && windowsWithSmallVersion.Contains(windowTypeName))
		{
			dictionary = smallSizeWindows;
			key = "Assets/AddressableAssets/UIElements/WindowsSmall/" + windowTypeName + "_Small.prefab";
		}
		else
		{
			dictionary = bigSizeWindows;
			key = "Assets/AddressableAssets/UIElements/WindowsBig/" + windowTypeName + "_Big.prefab";
		}
		if (dictionary.TryGetValue(windowTypeName, out var value))
		{
			return value;
		}
		Debug.Log($"#shutdown# GUIElements: window [{windowTypeName}] WaitForCompletion begin (shutdownRequested:[{GameShutdown.IsRequested}])");
		if (GameShutdown.IsQuitting)
		{
			return null;
		}
		GameObject original = Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion();
		Debug.Log("#shutdown# GUIElements: window [" + windowTypeName + "] WaitForCompletion done");
		value = UnityEngine.Object.Instantiate(original, uiFitter.transform).GetComponent<LazyWidgetBase>();
		ILazyGUIElement[] componentsInChildren = value.GetComponentsInChildren<ILazyGUIElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		value.gameObject.SetActive(value: false);
		dictionary.Add(windowTypeName, value);
		return value;
	}

	public void UpdateLocalizedLabels()
	{
		LocalizedLabel[] componentsInChildren = GetComponentsInChildren<LocalizedLabel>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Localize();
		}
		LocalizedAssetReferenceImage[] componentsInChildren2 = GetComponentsInChildren<LocalizedAssetReferenceImage>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].Localize();
		}
		LocalizedTextMargins[] componentsInChildren3 = GetComponentsInChildren<LocalizedTextMargins>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].ApplyMargins();
		}
		LocalizedVerticalOffset[] componentsInChildren4 = GetComponentsInChildren<LocalizedVerticalOffset>(includeInactive: true);
		for (int i = 0; i < componentsInChildren4.Length; i++)
		{
			componentsInChildren4[i].Apply();
		}
		LocalizedSize[] componentsInChildren5 = GetComponentsInChildren<LocalizedSize>(includeInactive: true);
		for (int i = 0; i < componentsInChildren5.Length; i++)
		{
			componentsInChildren5[i].Apply();
		}
	}

	public void PreloadWindows()
	{
		LazyUI.GetWindow<CharacterWindow>();
		LazyUI.GetWindow<UIMainMenuWindow>();
	}

	private bool OnUpdateDynamicSelectorActivity()
	{
		return true;
	}

	private void OnInputChanged()
	{
	}

	private void UpdateRewiredInputType()
	{
		standaloneInputModule.isGamepadActive = LazyInput.IsGamepadActive;
		if (LazyInput.IsGamepadActive)
		{
			standaloneInputModule.ClearMouseSelection();
		}
	}
}
