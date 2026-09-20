using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using LazyBearTechnology;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class HUD : LazyWidget<HUDData>
{
	[SerializeField]
	private GameObject leftUpGroup;

	[SerializeField]
	private GameObject rightUpGroup;

	[SerializeField]
	private UIEnergySanityBar energySanityBar;

	[SerializeField]
	private UIHUDWheel wheel;

	[SerializeField]
	private UIGameResNotificator gameResNotificatior;

	[SerializeField]
	private UIBuffsDisplay buffsDisplay;

	[SerializeField]
	private UIHotBarWidget hotBarWidget;

	[SerializeField]
	private LazyButton tutorialListBtn;

	[SerializeField]
	private Transform hotBarKeyboardPos;

	[SerializeField]
	private Transform hotBarGamepadPos;

	[SerializeField]
	private UIFightingTimelineRendererWidget fightingTimelineWidget;

	[SerializeField]
	private UIFightingSquadHudGroupWidget uiFightingSquadHudGroupWidget;

	[SerializeField]
	private UIFightingControlsWidget fightingControlsWidget;

	[SerializeField]
	private Transform controlsWidgetMouse;

	[SerializeField]
	private Transform controlsWidgetGamepad;

	[SerializeField]
	private TextMeshProUGUI happinessLabel;

	[SerializeField]
	[Space]
	[Header("Tech Points")]
	private TextMeshProUGUI redSpheresLabel;

	[SerializeField]
	private TextMeshProUGUI greenSpheresLabel;

	[SerializeField]
	private TextMeshProUGUI blueSpheresLabel;

	[SerializeField]
	private RectTransform techPointsPanel;

	[SerializeField]
	private RectTransform techPointsLeftAnchor;

	[SerializeField]
	private RectTransform techPointsRightAnchor;

	[SerializeField]
	private RectTransform techPointsSmallLeftAnchor;

	[SerializeField]
	private RectTransform techPointsSmallRightAnchor;

	[SerializeField]
	private float techPointsPanelMoveTime = 0.25f;

	[SerializeField]
	private float techPointsPanelShowTime = 5f;

	[SerializeField]
	private UIMagnifyingGlassWidget magnifyingGlassWidgetPrefab;

	[SerializeField]
	private float clampWidgetOffsetFromScreenBorder = 20f;

	private Tween currentTechPointsPanelTween;

	private bool isTechPointsPanelVisible;

	private bool isTechPointsPanelOnRightPlace;

	private float hideTechPointsPanelTimer;

	private static readonly Vector2 techPointsPanelBigPivot = new Vector2(0f, 0.5f);

	private static readonly Vector2 techPointsPanelSmallPivot = new Vector2(0.5f, 0.5f);

	private Canvas canvas;

	private bool subscribedGameEvents;

	private bool subscribedMagnifyingGlassTracking;

	private PlayerController subscribedPlayerController;

	private MultiFlagAND<HudStateType> disableStateType;

	private Pool magnifyingGlassWidgetPool;

	private readonly Dictionary<SGuid, WgoData> trackedWorkEventTargets = new Dictionary<SGuid, WgoData>();

	private readonly Dictionary<SGuid, UIMagnifyingGlassWidgetData> drawingMagnifyingGlassData = new Dictionary<SGuid, UIMagnifyingGlassWidgetData>();

	private readonly Dictionary<SGuid, UIMagnifyingGlassWidget> drawingMagnifyingGlasses = new Dictionary<SGuid, UIMagnifyingGlassWidget>();

	private readonly Dictionary<SGuid, WgoData> magnifyingGlassDoorOverride = new Dictionary<SGuid, WgoData>();

	private readonly List<SGuid> magnifyingGlassUntrackBuffer = new List<SGuid>();

	private HUDMode mode;

	public HUDMode Mode => mode;

	public event Action onTechPointsPanelShown;

	public override void Init()
	{
		canvas = GetComponent<Canvas>();
		canvas.overrideSorting = true;
		canvas.sortingOrder = 50;
		disableStateType = new MultiFlagAND<HudStateType>();
		disableStateType.Init(null, initialFlag: true);
		UpdateTechPointsPanelPivot();
		techPointsPanel.anchoredPosition = GetTechPointsLeftAnchor().anchoredPosition;
		if (tutorialListBtn != null)
		{
			tutorialListBtn.onClick.AddListener(OpenTutorialList);
		}
		GUIElements.OnWindowSizeTypeChanged += OnWindowSizeTypeChanged;
		UIGameBindingSettingsWindow.OnBtnUpdated += hotBarWidget.UpdateButtonsText;
		UIMouseTooltip.Attach(happinessLabel.gameObject, "tt_town_happiness", null, addRaycastTarget: true);
		energySanityBar.AttachHudIconTooltips();
		if (GetComponent<GraphicRaycaster>() == null)
		{
			base.gameObject.AddComponent<GraphicRaycaster>();
		}
		if (canvas.renderMode != 0 && canvas.worldCamera == null && CameraSystem.Instance != null)
		{
			canvas.worldCamera = CameraSystem.Instance.WorldCamera;
		}
		if (magnifyingGlassWidgetPrefab != null)
		{
			magnifyingGlassWidgetPool = new Pool(magnifyingGlassWidgetPrefab, base.transform, 1);
			magnifyingGlassWidgetPrefab.gameObject.SetActive(value: false);
		}
	}

	protected override void SetData(HUDData data)
	{
		base.SetData(data);
		hotBarWidget.SetDataOutside(data.HotBarWidgetData);
	}

	public override void Draw()
	{
		base.Draw();
		if (data != null)
		{
			Redraw();
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		energySanityBar.Draw(data.EnergySanityBarData);
		gameResNotificatior.Draw(data.GameResNotificatiorData);
		buffsDisplay.Draw(data.BuffsDisplayData);
		hotBarWidget.Draw(data.HotBarWidgetData);
		wheel.forceTimeDependentChange = true;
		wheel.OnNewDayStarted(EnvironmentEngine.Instance.Data.Day, animate: false);
		wheel.OnTimeOfDayChanged(EnvironmentEngine.Instance.timeOfDay);
		UpdateGamepadDependentStuff();
		UpdateTechPointsInstant();
		TrySubscribeGameEvents();
		TrySubscribeMagnifyingGlassTracking();
		UpdateHotBarEnabledState();
	}

	public override void Hide()
	{
		base.Hide();
		energySanityBar.Hide();
		buffsDisplay.Hide();
		TryUnsubscribeGameEvents();
		TryUnsubscribeMagnifyingGlassTracking();
	}

	public void DrawFightingTimeline(UIFightingTimelineRendererData widgetData)
	{
		fightingTimelineWidget.Draw(widgetData);
		uiFightingSquadHudGroupWidget.Draw(new UIFightingSquadHudGroupWidgetData(widgetData.CurrentLevel));
	}

	public void HideFightingTimeline()
	{
		fightingTimelineWidget.Hide();
		uiFightingSquadHudGroupWidget.Hide();
	}

	public void SetMode(HUDMode mode)
	{
		this.mode = mode;
		RectTransform rectTransform = buffsDisplay.RectTransform;
		RectTransform content = buffsDisplay.Content;
		switch (mode)
		{
		case HUDMode.Common:
		{
			leftUpGroup.SetActive(value: true);
			rightUpGroup.SetActive(value: true);
			fightingControlsWidget.gameObject.SetActive(value: false);
			fightingTimelineWidget.gameObject.SetActive(value: false);
			uiFightingSquadHudGroupWidget.gameObject.SetActive(value: false);
			Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0.5f, 1f));
			rectTransform.anchorMin = anchorMin;
			rectTransform.anchoredPosition = Vector2.zero;
			content.pivot = new Vector2(0.5f, 1f);
			content.anchoredPosition = new Vector2(0f, -9.5f);
			break;
		}
		case HUDMode.Fight:
		{
			leftUpGroup.SetActive(value: false);
			rightUpGroup.SetActive(value: false);
			fightingControlsWidget.gameObject.SetActive(value: true);
			fightingControlsWidget.Draw(new UIFightingControlsWidgetData());
			fightingTimelineWidget.gameObject.SetActive(value: true);
			uiFightingSquadHudGroupWidget.gameObject.SetActive(value: true);
			Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0f, 1f));
			rectTransform.anchorMin = anchorMin;
			rectTransform.anchoredPosition = Vector2.zero;
			content.pivot = new Vector2(0f, 1f);
			content.anchoredPosition = new Vector2(8f, -9.5f);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("mode", mode, null);
		}
	}

	private void Update()
	{
		if (isTechPointsPanelVisible)
		{
			hideTechPointsPanelTimer -= Time.deltaTime;
			if (hideTechPointsPanelTimer <= 0f)
			{
				TurnOffTechPointsPanel();
			}
		}
		if (mode == HUDMode.Fight)
		{
			fightingControlsWidget.CustomUpdate();
		}
	}

	public void SetDisableState(HudStateType type, bool isEnabled, HUDData dataToDraw = null)
	{
		disableStateType.UpdateFlag(type, isEnabled);
		Debug.Log($"HUD: SetDisableState:[{type}] isDisabled:[{isEnabled}] disableStateType.ResultFlag:[{disableStateType.ResultFlag}]");
		if (disableStateType.ResultFlag)
		{
			if (dataToDraw != null)
			{
				Draw(dataToDraw);
			}
			else
			{
				Draw(data);
			}
			return;
		}
		if (dataToDraw != null)
		{
			SetData(dataToDraw);
		}
		Hide();
	}

	private void TrySubscribeGameEvents()
	{
		if (!subscribedGameEvents)
		{
			subscribedGameEvents = true;
			GK2GameResSystem system = GK2GameResSystem.GetSystem("happiness");
			system.onValueChanged = (Action<float>)Delegate.Combine(system.onValueChanged, new Action<float>(UpdateHappinessInstant));
			GK2GameResSystem system2 = GK2GameResSystem.GetSystem("tech_red");
			system2.onValueChanged = (Action<float>)Delegate.Combine(system2.onValueChanged, new Action<float>(UpdateRedTechInstant));
			GK2GameResSystem system3 = GK2GameResSystem.GetSystem("tech_green");
			system3.onValueChanged = (Action<float>)Delegate.Combine(system3.onValueChanged, new Action<float>(UpdateGreenTechInstant));
			GK2GameResSystem system4 = GK2GameResSystem.GetSystem("tech_blue");
			system4.onValueChanged = (Action<float>)Delegate.Combine(system4.onValueChanged, new Action<float>(UpdateBlueTechInstant));
			TownSystem.OnQualityChanged += UpdateHappinessInstant;
			KnowledgeSystem.OnTutorialViewed = (Action<string>)Delegate.Combine(KnowledgeSystem.OnTutorialViewed, new Action<string>(HandleTutorialViewed));
			LazyInput.OnInputChanged += UpdateGamepadDependentStuff;
			subscribedPlayerController = MainGame.PlayerController;
			subscribedPlayerController.OnControlStateChanged += HandlePlayerControlStateChanged;
			LazyWindowsStackController.OnWindowOpened += UpdateHotBarEnabledState;
			LazyWindowsStackController.OnWindowClosed += UpdateHotBarEnabledState;
			LazyWindowsStackController.OnAllWindowsClosed += UpdateHotBarEnabledState;
			CharacterWindow.OnTabChanged += UpdateHotBarEnabledState;
		}
	}

	private void TryUnsubscribeGameEvents()
	{
		if (subscribedGameEvents)
		{
			subscribedGameEvents = false;
			GK2GameResSystem system = GK2GameResSystem.GetSystem("happiness");
			system.onValueChanged = (Action<float>)Delegate.Remove(system.onValueChanged, new Action<float>(UpdateHappinessInstant));
			GK2GameResSystem system2 = GK2GameResSystem.GetSystem("tech_red");
			system2.onValueChanged = (Action<float>)Delegate.Remove(system2.onValueChanged, new Action<float>(UpdateRedTechInstant));
			GK2GameResSystem system3 = GK2GameResSystem.GetSystem("tech_green");
			system3.onValueChanged = (Action<float>)Delegate.Remove(system3.onValueChanged, new Action<float>(UpdateGreenTechInstant));
			GK2GameResSystem system4 = GK2GameResSystem.GetSystem("tech_blue");
			system4.onValueChanged = (Action<float>)Delegate.Remove(system4.onValueChanged, new Action<float>(UpdateBlueTechInstant));
			TownSystem.OnQualityChanged -= UpdateHappinessInstant;
			KnowledgeSystem.OnTutorialViewed = (Action<string>)Delegate.Remove(KnowledgeSystem.OnTutorialViewed, new Action<string>(HandleTutorialViewed));
			LazyInput.OnInputChanged -= UpdateGamepadDependentStuff;
			UnsubscribePlayerControlStateChanged();
			LazyWindowsStackController.OnWindowOpened -= UpdateHotBarEnabledState;
			LazyWindowsStackController.OnWindowClosed -= UpdateHotBarEnabledState;
			LazyWindowsStackController.OnAllWindowsClosed -= UpdateHotBarEnabledState;
			CharacterWindow.OnTabChanged -= UpdateHotBarEnabledState;
		}
	}

	private void TrySubscribeMagnifyingGlassTracking()
	{
		if (!subscribedMagnifyingGlassTracking && magnifyingGlassWidgetPool != null)
		{
			subscribedMagnifyingGlassTracking = true;
			WgoData.OnAnyInteractionEventChanged += HandleInteractionEventChangedAndRedraw;
			PlayerController.OnPlayerTeleported += HandlePlayerTeleportedMagnifyingGlasses;
			CinemachineCore.CameraUpdatedEvent.AddListener(UpdateMagnifyingGlasses);
			ScanExistingWorkEvents();
			UpdateMagnifyingGlasses(null);
		}
	}

	private void TryUnsubscribeMagnifyingGlassTracking()
	{
		if (subscribedMagnifyingGlassTracking)
		{
			subscribedMagnifyingGlassTracking = false;
			WgoData.OnAnyInteractionEventChanged -= HandleInteractionEventChangedAndRedraw;
			PlayerController.OnPlayerTeleported -= HandlePlayerTeleportedMagnifyingGlasses;
			CinemachineCore.CameraUpdatedEvent.RemoveListener(UpdateMagnifyingGlasses);
			UntrackAllMagnifyingGlasses();
		}
	}

	private void ScanExistingWorkEvents()
	{
		if (MainGame.WorldData == null || !MainGame.WorldData.HasCache)
		{
			return;
		}
		foreach (WgoData value in MainGame.WorldData.Cache.wgoDataByUidCache.Values)
		{
			HandleInteractionEventChanged(value);
		}
	}

	private void HandleInteractionEventChangedAndRedraw(WgoData wgoData)
	{
		HandleInteractionEventChanged(wgoData);
		UpdateMagnifyingGlasses(null);
	}

	private void HandleInteractionEventChanged(WgoData wgoData)
	{
		if (wgoData != null)
		{
			if (HasWorkInteractionEvent(wgoData))
			{
				TrackWorkEventTarget(wgoData);
			}
			else
			{
				UntrackWorkEventTarget(wgoData.UniqueId);
			}
		}
	}

	private static bool HasWorkInteractionEvent(WgoData wgoData)
	{
		InteractionEvent interactionEvent = wgoData.PeekFirstAddedEvent();
		if (interactionEvent != null)
		{
			return interactionEvent.type == InteractionEvent.Type.Work;
		}
		return false;
	}

	private void TrackWorkEventTarget(WgoData wgoData)
	{
		SGuid uniqueId = wgoData.UniqueId;
		if (trackedWorkEventTargets.ContainsKey(uniqueId))
		{
			trackedWorkEventTargets[uniqueId] = wgoData;
			RefreshMagnifyingGlassWorldOverride(uniqueId, wgoData);
		}
		else
		{
			trackedWorkEventTargets.Add(uniqueId, wgoData);
			drawingMagnifyingGlassData.Add(uniqueId, new UIMagnifyingGlassWidgetData(uniqueId, Vector2.zero, Vector2.zero));
			RefreshMagnifyingGlassWorldOverride(uniqueId, wgoData);
		}
	}

	private void UntrackWorkEventTarget(SGuid uniqueId)
	{
		if (!SGuid.IsNullOrEmpty(uniqueId) && trackedWorkEventTargets.ContainsKey(uniqueId))
		{
			ReleaseMagnifyingGlassWidget(uniqueId);
			drawingMagnifyingGlassData.Remove(uniqueId);
			magnifyingGlassDoorOverride.Remove(uniqueId);
			trackedWorkEventTargets.Remove(uniqueId);
		}
	}

	private void UntrackAllMagnifyingGlasses()
	{
		magnifyingGlassUntrackBuffer.Clear();
		foreach (SGuid key in trackedWorkEventTargets.Keys)
		{
			magnifyingGlassUntrackBuffer.Add(key);
		}
		foreach (SGuid item in magnifyingGlassUntrackBuffer)
		{
			UntrackWorkEventTarget(item);
		}
	}

	private void UpdateMagnifyingGlasses(CinemachineBrain brain)
	{
		if (magnifyingGlassWidgetPool == null || trackedWorkEventTargets.Count == 0 || CameraSystem.Instance == null)
		{
			return;
		}
		Bounds screenBounds = LazyUI.GetScreenBounds();
		Vector2 from = new Vector2(screenBounds.center.x, screenBounds.center.y);
		Rect rect = new Rect(screenBounds.min.x + clampWidgetOffsetFromScreenBorder, screenBounds.min.y + clampWidgetOffsetFromScreenBorder, screenBounds.size.x - clampWidgetOffsetFromScreenBorder * 2f, screenBounds.size.y - clampWidgetOffsetFromScreenBorder * 2f);
		magnifyingGlassUntrackBuffer.Clear();
		foreach (KeyValuePair<SGuid, WgoData> trackedWorkEventTarget in trackedWorkEventTargets)
		{
			SGuid key = trackedWorkEventTarget.Key;
			WgoData value = trackedWorkEventTarget.Value;
			if (value == null || MainGame.WorldData?.GetWgoData(key) == null || !HasWorkInteractionEvent(value))
			{
				magnifyingGlassUntrackBuffer.Add(key);
				continue;
			}
			RefreshMagnifyingGlassWorldOverride(key, value);
			Vector2 vector = CameraSystem.WorldToScreenPoint(GetMagnifyingGlassTrackedWorldPosition(key, value));
			bool flag = !rect.Contains(vector);
			bool flag2 = magnifyingGlassDoorOverride.ContainsKey(key);
			if ((!flag2 && UIObjectBubbleManager.Instance != null && UIObjectBubbleManager.Instance.TryGetDisplayedBubble(key, out var bubble) && !bubble.IsOutOfScreen) || (!flag && !flag2))
			{
				ReleaseMagnifyingGlassWidget(key);
				continue;
			}
			UIMagnifyingGlassWidgetData uIMagnifyingGlassWidgetData = drawingMagnifyingGlassData[key];
			uIMagnifyingGlassWidgetData.IsOutOfScreen = flag;
			if (flag)
			{
				uIMagnifyingGlassWidgetData.ScreenPosition = GetRectEdgeIntersection(from, vector, rect);
				uIMagnifyingGlassWidgetData.DirectionToTarget = GetPointerDirectionFromEdgePosition(uIMagnifyingGlassWidgetData.ScreenPosition, rect);
			}
			else
			{
				uIMagnifyingGlassWidgetData.ScreenPosition = vector;
				uIMagnifyingGlassWidgetData.DirectionToTarget = Vector2.zero;
			}
			if (!drawingMagnifyingGlasses.TryGetValue(key, out var value2))
			{
				value2 = magnifyingGlassWidgetPool.GetOrCreateObject<UIMagnifyingGlassWidget>();
				value2.transform.SetParent(base.transform);
				value2.transform.SetAsLastSibling();
				drawingMagnifyingGlasses.Add(key, value2);
			}
			value2.Draw(uIMagnifyingGlassWidgetData);
		}
		foreach (SGuid item in magnifyingGlassUntrackBuffer)
		{
			UntrackWorkEventTarget(item);
		}
	}

	private void HandlePlayerTeleportedMagnifyingGlasses()
	{
		foreach (KeyValuePair<SGuid, WgoData> trackedWorkEventTarget in trackedWorkEventTargets)
		{
			RefreshMagnifyingGlassWorldOverride(trackedWorkEventTarget.Key, trackedWorkEventTarget.Value);
		}
		UpdateMagnifyingGlasses(null);
	}

	private void RefreshMagnifyingGlassWorldOverride(SGuid uniqueId, WgoData wgoData)
	{
		Vector3 magnifyingGlassObjectWorldPosition = GetMagnifyingGlassObjectWorldPosition(uniqueId, wgoData);
		Vector3 playerPos = ((MainGame.PlayerController != null) ? MainGame.PlayerController.transform.position : magnifyingGlassObjectWorldPosition);
		if (TeleportPointGraph.Instance != null && TeleportPointGraph.Instance.TryResolveDoor(playerPos, magnifyingGlassObjectWorldPosition, wgoData, out var doorWgo))
		{
			magnifyingGlassDoorOverride[uniqueId] = doorWgo;
		}
		else
		{
			magnifyingGlassDoorOverride.Remove(uniqueId);
		}
	}

	private Vector3 GetMagnifyingGlassTrackedWorldPosition(SGuid uniqueId, WgoData wgoData)
	{
		if (magnifyingGlassDoorOverride.TryGetValue(uniqueId, out var value) && value != null)
		{
			return GetMagnifyingGlassObjectWorldPosition(value.UniqueId, value);
		}
		return GetMagnifyingGlassObjectWorldPosition(uniqueId, wgoData);
	}

	private static Vector3 GetMagnifyingGlassObjectWorldPosition(SGuid uniqueId, WgoData wgoData)
	{
		Wgo wgoViewGlobal = GameScene.GetWgoViewGlobal(uniqueId);
		if (wgoViewGlobal != null)
		{
			return wgoViewGlobal.BubbleDrawablePosition;
		}
		return wgoData.BubblePos;
	}

	private void ReleaseMagnifyingGlassWidget(SGuid uniqueId)
	{
		if (drawingMagnifyingGlasses.TryGetValue(uniqueId, out var value))
		{
			magnifyingGlassWidgetPool.ReleaseObject(value);
			drawingMagnifyingGlasses.Remove(uniqueId);
		}
	}

	private Vector2 GetRectEdgeIntersection(Vector2 from, Vector2 to, Rect rect)
	{
		Vector2 vector = to - from;
		float a = 0f;
		float num = 1f;
		if (Mathf.Abs(vector.x) > 0.0001f)
		{
			float num2 = (rect.xMin - from.x) / vector.x;
			float num3 = (rect.xMax - from.x) / vector.x;
			if (vector.x < 0f)
			{
				float num4 = num3;
				float num5 = num2;
				num2 = num4;
				num3 = num5;
			}
			a = Mathf.Max(a, num2);
			num = Mathf.Min(num, num3);
		}
		if (Mathf.Abs(vector.y) > 0.0001f)
		{
			float num6 = (rect.yMin - from.y) / vector.y;
			float num7 = (rect.yMax - from.y) / vector.y;
			if (vector.y < 0f)
			{
				float num8 = num7;
				float num5 = num6;
				num6 = num8;
				num7 = num5;
			}
			a = Mathf.Max(a, num6);
			num = Mathf.Min(num, num7);
		}
		float num9 = Mathf.Clamp(num, 0f, 1f);
		return from + vector * num9;
	}

	private static Vector2 GetPointerDirectionFromEdgePosition(Vector2 widgetPosition, Rect clampRect)
	{
		float num = Mathf.Abs(widgetPosition.x - clampRect.xMin);
		float num2 = Mathf.Abs(widgetPosition.x - clampRect.xMax);
		float num3 = Mathf.Abs(widgetPosition.y - clampRect.yMin);
		float num4 = Mathf.Abs(widgetPosition.y - clampRect.yMax);
		float num5 = Mathf.Min(num, num2);
		float num6;
		if (Mathf.Min(num3, num4) <= num5)
		{
			float t = ((clampRect.width > 0.0001f) ? Mathf.InverseLerp(clampRect.xMin, clampRect.xMax, widgetPosition.x) : 0.5f);
			num6 = ((num4 <= num3) ? Mathf.Lerp(-30f, 30f, t) : (180f + Mathf.Lerp(30f, -30f, t)));
		}
		else
		{
			float t2 = ((clampRect.height > 0.0001f) ? Mathf.InverseLerp(clampRect.yMin, clampRect.yMax, widgetPosition.y) : 0.5f);
			num6 = ((num <= num2) ? (270f + Mathf.Lerp(-30f, 30f, t2)) : (90f + Mathf.Lerp(30f, -30f, t2)));
		}
		float f = (90f - num6) * (MathF.PI / 180f);
		return new Vector2(Mathf.Cos(f), Mathf.Sin(f));
	}

	private void OnDestroy()
	{
		GUIElements.OnWindowSizeTypeChanged -= OnWindowSizeTypeChanged;
		UnsubscribePlayerControlStateChanged();
		TryUnsubscribeMagnifyingGlassTracking();
	}

	private void UpdateGamepadDependentStuff()
	{
		if (LazyInput.IsGamepadActive)
		{
			hotBarWidget.transform.SetParent(hotBarGamepadPos);
			hotBarGamepadPos.gameObject.SetActive(value: true);
			hotBarKeyboardPos.gameObject.SetActive(value: false);
			fightingControlsWidget.transform.SetParent(controlsWidgetGamepad);
			fightingControlsWidget.DrawGamepadTips();
		}
		else
		{
			hotBarWidget.transform.SetParent(hotBarKeyboardPos);
			hotBarGamepadPos.gameObject.SetActive(value: false);
			hotBarKeyboardPos.gameObject.SetActive(value: true);
			fightingControlsWidget.transform.SetParent(controlsWidgetMouse);
		}
		LazyPlatformDependentElement[] componentsInChildren = GetComponentsInChildren<LazyPlatformDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Init();
		}
		LazyGamepadDependentElement[] componentsInChildren2 = GetComponentsInChildren<LazyGamepadDependentElement>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].UpdateState();
		}
		UpdateTutorialListButton();
		((RectTransform)hotBarWidget.transform).anchoredPosition = Vector3.zero;
		((RectTransform)fightingControlsWidget.transform).anchoredPosition = Vector3.zero;
	}

	private void UpdateHotBarEnabledState(CharacterWindowData.CharPage page)
	{
		UpdateHotBarEnabledState();
	}

	private void UpdateHotBarEnabledState(LazyWidgetBase widgetBase)
	{
		UpdateHotBarEnabledState();
	}

	private void UpdateHotBarEnabledState()
	{
		hotBarWidget.gameObject.SetActive(LazyWindowsStackController.ActiveWindow == null);
	}

	private void OpenTutorialList()
	{
		LazyUI.GetWindow<UITutorialListWindow>().Open(new UITutorialListWindowData(UITutorialListOpenSource.HUD));
	}

	private void HandleTutorialViewed(string _)
	{
		UpdateTutorialListButton();
	}

	private void HandlePlayerControlStateChanged()
	{
		UpdateTutorialListButton();
	}

	private void UnsubscribePlayerControlStateChanged()
	{
		if (!(subscribedPlayerController == null))
		{
			subscribedPlayerController.OnControlStateChanged -= HandlePlayerControlStateChanged;
			subscribedPlayerController = null;
		}
	}

	private void UpdateTutorialListButton()
	{
		if (!(tutorialListBtn == null) && MainGame.Instance?.GameSave?.knowledgeSystem != null)
		{
			PlayerController playerController = MainGame.PlayerController;
			tutorialListBtn.gameObject.SetActive(MainGame.Instance.GameSave.knowledgeSystem.HasViewedTutorials() && !LazyInput.IsGamepadActive && playerController != null && playerController.IsControlsEnabled);
		}
	}

	public void UpdateHappinessInstant(float value)
	{
		if (MainGame.Instance.GameSave.townSystem.Quality > 0)
		{
			happinessLabel.text = string.Format("{0}{1}/{2}", "happiness".FontIcon(), value, MainGame.Instance.GameSave.townSystem.Quality);
		}
		else
		{
			happinessLabel.text = string.Format("{0}{1}", "happiness".FontIcon(), value);
		}
	}

	private void UpdateHappinessInstant()
	{
		UpdateHappinessInstant(MainGame.PlayerData.GetRes("happiness"));
	}

	public void UpdateTechPointsInstant()
	{
		UpdateRedTechInstant(MainGame.PlayerData.GetRes("tech_red"));
		UpdateGreenTechInstant(MainGame.PlayerData.GetRes("tech_green"));
		UpdateBlueTechInstant(MainGame.PlayerData.GetRes("tech_blue"));
		UpdateHappinessInstant(MainGame.PlayerData.GetRes("happiness"));
	}

	public bool TryTurnOnTechPointsPanel()
	{
		if (isTechPointsPanelVisible)
		{
			hideTechPointsPanelTimer = techPointsPanelShowTime;
			return isTechPointsPanelOnRightPlace;
		}
		isTechPointsPanelVisible = true;
		hideTechPointsPanelTimer = techPointsPanelShowTime;
		currentTechPointsPanelTween?.Kill();
		UpdateTechPointsPanelPivot();
		techPointsPanel.anchoredPosition = GetTechPointsLeftAnchor().anchoredPosition;
		currentTechPointsPanelTween = techPointsPanel.DOAnchorPos(GetTechPointsRightAnchor().anchoredPosition, techPointsPanelMoveTime).SetEase(Ease.OutCubic).OnComplete(delegate
		{
			isTechPointsPanelOnRightPlace = true;
			this.onTechPointsPanelShown?.Invoke();
		});
		return false;
	}

	private void TurnOffTechPointsPanel()
	{
		isTechPointsPanelOnRightPlace = false;
		isTechPointsPanelVisible = false;
		currentTechPointsPanelTween?.Kill();
		UpdateTechPointsPanelPivot();
		currentTechPointsPanelTween = techPointsPanel.DOAnchorPos(GetTechPointsLeftAnchor().anchoredPosition, techPointsPanelMoveTime).SetEase(Ease.InCubic);
	}

	private void OnWindowSizeTypeChanged(UIWindowSizeType _)
	{
		UpdateTechPointsPanelPivot();
		RectTransform rectTransform = (isTechPointsPanelVisible ? GetTechPointsRightAnchor() : GetTechPointsLeftAnchor());
		currentTechPointsPanelTween?.Kill();
		isTechPointsPanelOnRightPlace = false;
		currentTechPointsPanelTween = techPointsPanel.DOAnchorPos(rectTransform.anchoredPosition, techPointsPanelMoveTime).SetEase(isTechPointsPanelVisible ? Ease.OutCubic : Ease.InCubic).OnComplete(delegate
		{
			isTechPointsPanelOnRightPlace = isTechPointsPanelVisible;
		});
	}

	private void UpdateTechPointsPanelPivot()
	{
		techPointsPanel.pivot = ((GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Small) ? techPointsPanelSmallPivot : techPointsPanelBigPivot);
	}

	private RectTransform GetTechPointsLeftAnchor()
	{
		if (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Small && techPointsSmallLeftAnchor != null)
		{
			return techPointsSmallLeftAnchor;
		}
		return techPointsLeftAnchor;
	}

	private RectTransform GetTechPointsRightAnchor()
	{
		if (GUIElements.Instance.UIWindowSizeType == UIWindowSizeType.Small && techPointsSmallRightAnchor != null)
		{
			return techPointsSmallRightAnchor;
		}
		return techPointsRightAnchor;
	}

	private void UpdateRedTechInstant(float value)
	{
		redSpheresLabel.text = string.Format("{0}{1}", "tech_red".FontIcon(), value);
	}

	private void UpdateGreenTechInstant(float value)
	{
		greenSpheresLabel.text = string.Format("{0}{1}", "tech_green".FontIcon(), value);
	}

	private void UpdateBlueTechInstant(float value)
	{
		blueSpheresLabel.text = string.Format("{0}{1}", "tech_blue".FontIcon(), value);
	}

	public TextMeshProUGUI GetHudLabel(string type)
	{
		return type switch
		{
			"tech_red" => redSpheresLabel, 
			"tech_green" => greenSpheresLabel, 
			"tech_blue" => blueSpheresLabel, 
			"happiness" => happinessLabel, 
			_ => null, 
		};
	}

	[LazyUITest]
	protected override void TestDraw()
	{
		Draw(new HUDData(MainGame.Instance.GameSave));
	}
}
