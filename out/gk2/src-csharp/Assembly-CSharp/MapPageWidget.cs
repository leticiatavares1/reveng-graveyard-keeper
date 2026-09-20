using System;
using System.Collections.Generic;
using LazyBearTechnology;
using LinqTools;
using UnityEngine;
using UnityEngine.UI;

public class MapPageWidget : LazyWidget<MapPageWidgetData>
{
	[Serializable]
	private class WorldZonePoint
	{
		public Vector2 mapPosition;

		public string worldZoneId;
	}

	private static readonly Vector3[] viewportCornersBuffer = new Vector3[4];

	[SerializeField]
	private RectTransform playerIcon;

	[SerializeField]
	private RectTransform mapRect;

	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private float cursorBorderMargin = 10f;

	[SerializeField]
	private float keyboardPanSpeed = 350f;

	[SerializeField]
	private MapVirtualCursor mapVirtualCursor;

	[SerializeField]
	private List<WorldZonePoint> worldZonePoints;

	[SerializeField]
	private List<UIMapMilestoneData> milestonesData;

	[SerializeField]
	private List<GameObject> mapFightIcons;

	[SerializeField]
	private GameObject onAllUnlockedObject;

	[SerializeField]
	private UIHierarchySorter hierarchySorter;

	private List<UISortComponent> sortComponentsOtherMap;

	private Dictionary<string, UIMapMilestone> createdMilestones;

	private UIMapMilestone currentSelected;

	private Dictionary<string, UIMapZone> zones;

	private bool isInitialized;

	public UIMapMilestone CurrentSelected => currentSelected;

	public override void Init()
	{
		if (isInitialized)
		{
			return;
		}
		isInitialized = true;
		base.Init();
		createdMilestones = new Dictionary<string, UIMapMilestone>();
		zones = new Dictionary<string, UIMapZone>();
		sortComponentsOtherMap = onAllUnlockedObject.GetComponentsInChildren<UISortComponent>(includeInactive: true).ToList();
		UIMapZone[] componentsInChildren = GetComponentsInChildren<UIMapZone>(includeInactive: true);
		foreach (UIMapZone uIMapZone in componentsInChildren)
		{
			uIMapZone.Init();
			zones.Add(uIMapZone.Id, uIMapZone);
			foreach (UISortComponent sortComponent in uIMapZone.SortComponents)
			{
				sortComponent.transform.SetParent(hierarchySorter.HierarchyTarget);
			}
		}
		foreach (UISortComponent item in sortComponentsOtherMap)
		{
			item.transform.SetParent(hierarchySorter.HierarchyTarget);
		}
		onAllUnlockedObject.gameObject.SetActive(value: false);
		hierarchySorter.ReinitChildrenAndSortComponents();
	}

	public void UpdateGamepadDependentStuff()
	{
		if (mapVirtualCursor != null)
		{
			mapVirtualCursor.gameObject.SetActive(LazyInput.IsGamepadActive);
		}
	}

	public override void Redraw()
	{
		base.Redraw();
		currentSelected = null;
		UpdateMilestones();
		UpdatePlayerPos();
		UpdateZones();
		UpdateFightIcons();
		if (mapVirtualCursor != null && scrollRect != null && scrollRect.viewport != null)
		{
			mapVirtualCursor.SetBoundsRectTransform(scrollRect.viewport);
			CenterMapVirtualCursor();
		}
		UpdateGamepadDependentStuff();
	}

	private void Update()
	{
		if (scrollRect == null || scrollRect.viewport == null || scrollRect.content == null)
		{
			return;
		}
		if (LazyInput.IsGamepadActive)
		{
			if (!(mapVirtualCursor == null))
			{
				Vector2 cursorPos = mapVirtualCursor.transform.position;
				Vector2 desiredMovementDelta = mapVirtualCursor.DesiredMovementDelta;
				if (!(desiredMovementDelta.sqrMagnitude <= 0.0001f))
				{
					PanMapByCursorBorder(cursorPos, desiredMovementDelta);
				}
			}
		}
		else
		{
			PanMapByKeyboard();
		}
	}

	private void PanMapByKeyboard()
	{
		Vector2 direction = LazyInput.GetDirection();
		if (!(direction.sqrMagnitude <= 0.0001f))
		{
			scrollRect.StopMovement();
			Vector2 worldDelta = direction * keyboardPanSpeed * LazyUI.ScaleFactor * Time.deltaTime;
			Vector2 targetAnchoredPos = scrollRect.content.anchoredPosition - GetViewportLocalDelta(worldDelta);
			scrollRect.content.anchoredPosition = ClampContentAnchoredPosition(targetAnchoredPos);
		}
	}

	private void PanMapByCursorBorder(Vector2 cursorPos, Vector2 cursorDelta)
	{
		scrollRect.viewport.GetWorldCorners(viewportCornersBuffer);
		Vector2 vector = viewportCornersBuffer[0];
		Vector2 vector2 = viewportCornersBuffer[2];
		Vector2 viewportWorldDelta = GetViewportWorldDelta(new Vector2(cursorBorderMargin, cursorBorderMargin));
		viewportWorldDelta = new Vector2(Mathf.Abs(viewportWorldDelta.x), Mathf.Abs(viewportWorldDelta.y));
		Vector2 vector3 = vector + viewportWorldDelta;
		Vector2 vector4 = vector2 - viewportWorldDelta;
		bool flag = (cursorDelta.x < 0f && cursorPos.x < vector3.x) || (cursorDelta.x > 0f && cursorPos.x > vector4.x);
		bool flag2 = (cursorDelta.y < 0f && cursorPos.y < vector3.y) || (cursorDelta.y > 0f && cursorPos.y > vector4.y);
		if (flag || flag2)
		{
			Vector2 worldDelta = new Vector2(flag ? cursorDelta.x : 0f, flag2 ? cursorDelta.y : 0f);
			Vector2 targetAnchoredPos = scrollRect.content.anchoredPosition - GetViewportLocalDelta(worldDelta);
			scrollRect.content.anchoredPosition = ClampContentAnchoredPosition(targetAnchoredPos);
		}
	}

	private Vector2 GetViewportLocalDelta(Vector2 worldDelta)
	{
		return scrollRect.viewport.InverseTransformVector(worldDelta);
	}

	private Vector2 GetViewportWorldDelta(Vector2 localDelta)
	{
		return scrollRect.viewport.TransformVector(localDelta);
	}

	private Vector2 ClampContentAnchoredPosition(Vector2 targetAnchoredPos)
	{
		Vector2 sizeDelta = scrollRect.content.sizeDelta;
		Vector2 size = scrollRect.viewport.rect.size;
		float min = (0f - (sizeDelta.x - size.x)) * 0.5f;
		float max = (sizeDelta.x - size.x) * 0.5f;
		float min2 = (0f - (sizeDelta.y - size.y)) * 0.5f;
		float max2 = (sizeDelta.y - size.y) * 0.5f;
		if (sizeDelta.x <= size.x)
		{
			targetAnchoredPos.x = 0f;
		}
		else
		{
			targetAnchoredPos.x = Mathf.Clamp(targetAnchoredPos.x, min, max);
		}
		if (sizeDelta.y <= size.y)
		{
			targetAnchoredPos.y = 0f;
		}
		else
		{
			targetAnchoredPos.y = Mathf.Clamp(targetAnchoredPos.y, min2, max2);
		}
		return targetAnchoredPos;
	}

	private void CenterMapVirtualCursor()
	{
		if (!(mapVirtualCursor == null) && !(scrollRect == null) && !(scrollRect.viewport == null))
		{
			scrollRect.viewport.GetWorldCorners(viewportCornersBuffer);
			Vector3 position = (viewportCornersBuffer[0] + viewportCornersBuffer[2]) * 0.5f;
			position.z = mapVirtualCursor.transform.position.z;
			mapVirtualCursor.SetPosition(position);
		}
	}

	public void UpdatePlayerPos()
	{
		playerIcon.gameObject.SetActive(value: true);
		mapRect.gameObject.SetActive(value: true);
		scrollRect.content.sizeDelta = mapRect.sizeDelta;
		playerIcon.SetParent(mapRect);
		Vector3 value = data.PlayerData.position.Value;
		float num = Mathf.Min(GUIElements.Instance.WorldMin.position.x, GUIElements.Instance.WorldMax.position.x);
		float num2 = Mathf.Max(GUIElements.Instance.WorldMin.position.x, GUIElements.Instance.WorldMax.position.x);
		float num3 = Mathf.Min(GUIElements.Instance.WorldMin.position.z, GUIElements.Instance.WorldMax.position.z);
		float num4 = Mathf.Max(GUIElements.Instance.WorldMin.position.z, GUIElements.Instance.WorldMax.position.z);
		Rect rect = new Rect(num, num3, num2 - num, num4 - num3);
		float f = MathF.PI / 180f * GUIElements.Instance.WorldMin.rotation.x;
		float num5 = value.z + value.y * Mathf.Tan(f);
		if (rect.Contains(new Vector2(value.x, num5)))
		{
			Debug.Log("#Map# UpdatePlayerPos In World");
			float num6 = Mathf.InverseLerp(GUIElements.Instance.WorldMin.position.x, GUIElements.Instance.WorldMax.position.x, value.x);
			float num7 = Mathf.InverseLerp(GUIElements.Instance.WorldMin.position.z, GUIElements.Instance.WorldMax.position.z, num5);
			Vector2 sizeDelta = mapRect.sizeDelta;
			Vector2 anchoredPosition = new Vector2((num6 - 0.5f) * sizeDelta.x, (num7 - 0.5f) * sizeDelta.y);
			playerIcon.anchoredPosition = anchoredPosition;
			FocusOnPlayer();
		}
		else if (MainGame.PlayerData.CurrentWorldZoneData != null)
		{
			for (int i = 0; i < worldZonePoints.Count; i++)
			{
				WorldZonePoint worldZonePoint = worldZonePoints[i];
				if (worldZonePoint.worldZoneId == MainGame.PlayerData.CurrentWorldZoneData.id)
				{
					playerIcon.anchoredPosition = worldZonePoint.mapPosition;
					Debug.Log("#Map# UpdatePlayerPos In WorldZone:[" + worldZonePoint.worldZoneId + "]");
					FocusOnPlayer();
					return;
				}
			}
			playerIcon.gameObject.SetActive(value: false);
			Debug.Log("#Map# player not in World, not in WorldZone:[" + MainGame.PlayerData.CurrentWorldZoneData.id + "]. But it is not added into MapPage Config");
		}
		else
		{
			scrollRect.content.anchoredPosition = Vector2.zero;
			playerIcon.gameObject.SetActive(value: false);
			Debug.Log($"#Map# player not in World, not in WorldZone. Pos:[{value}]");
		}
		playerIcon.SetAsLastSibling();
		hierarchySorter.HierarchyTarget.SetAsLastSibling();
	}

	public void OnEnterMapMilestone(UIMapMilestone mapMilestone)
	{
		if (currentSelected != null)
		{
			currentSelected.OnEnter();
		}
		currentSelected = mapMilestone;
		currentSelected.OnEnter();
	}

	public void OnExitMapMilestone(UIMapMilestone mapMilestone)
	{
		currentSelected.OnExit();
		if (currentSelected == mapMilestone)
		{
			currentSelected = null;
		}
	}

	public void OnPressMapMilestone(UIMapMilestone mapMilestone)
	{
		UIMapWindow window = LazyUI.GetWindow<UIMapWindow>();
		if (window.IsShown)
		{
			window.Close();
		}
		PlayerController.Teleport((!mapMilestone.UIMapMilestoneData.isNeedApplyPreset) ? new GDPointTeleportData(mapMilestone.WgoData.GetGDPointData("milestone_teleport_point")) : new GDPointTeleportData(mapMilestone.WgoData.GetGDPointData("milestone_teleport_point"), mapMilestone.UIMapMilestoneData.presetNameToApply));
		if (!string.IsNullOrEmpty(mapMilestone.UIMapMilestoneData.lazyExpressionOnTeleporting))
		{
			new LazyExpression(mapMilestone.UIMapMilestoneData.lazyExpressionOnTeleporting).Evaluate();
		}
	}

	public static List<string> GetALlMapZonesForUnlock()
	{
		MapPageWidget mapPageWidget = LazyUI.GetWindow<UIMapWindow>().MapPageWidget;
		if (!mapPageWidget.isInitialized)
		{
			mapPageWidget.Init();
		}
		return mapPageWidget.zones.Keys.ToList();
	}

	private void UpdateZones()
	{
		bool flag = true;
		foreach (KeyValuePair<string, UIMapZone> zone in zones)
		{
			UIMapZone value = zone.Value;
			bool flag2 = MainGame.Instance.GameSave.knowledgeSystem.knownMapZones.Contains(value.Id);
			value.Draw(!flag2);
			if (!flag2)
			{
				flag = false;
			}
		}
		foreach (UISortComponent item in sortComponentsOtherMap)
		{
			item.gameObject.SetActive(!flag);
		}
	}

	private void UpdateMilestones()
	{
		Transform worldMin = GUIElements.Instance.WorldMin;
		Transform worldMax = GUIElements.Instance.WorldMax;
		float f = MathF.PI / 180f * GUIElements.Instance.WorldMin.rotation.x;
		Vector2 sizeDelta = mapRect.sizeDelta;
		foreach (UIMapMilestoneData milestonesDatum in milestonesData)
		{
			WgoData wgoData = MainGame.WorldData.GetWgoData(milestonesDatum.wgoId);
			UIMapMilestone value;
			bool flag = createdMilestones.TryGetValue(milestonesDatum.wgoId, out value);
			if (wgoData == null || wgoData.IsHidden)
			{
				if (flag)
				{
					value.gameObject.SetActive(value: false);
				}
				continue;
			}
			if (!flag)
			{
				value = UIPrefabsPooler.Instance.GetElementFromPool<UIMapMilestone>(mapRect);
				createdMilestones.Add(milestonesDatum.wgoId, value);
			}
			if (wgoData.GetGameRes("activated_milestone") > 0f)
			{
				value.DrawAsActivated(milestonesDatum, wgoData, data.MilestonesInteractable && data.CurrentMilestone != milestonesDatum.wgoId, OnPressMapMilestone);
			}
			else
			{
				value.DrawAsNotActivated(milestonesDatum, wgoData);
			}
			if (milestonesDatum.hasCustomMapPos)
			{
				value.RectTransform.anchoredPosition = milestonesDatum.customMapPos;
				continue;
			}
			Vector3 position = wgoData.Position;
			float value2 = position.z + position.y * Mathf.Tan(f);
			value.gameObject.SetActive(value: true);
			float num = Mathf.InverseLerp(worldMin.position.x, worldMax.position.x, position.x);
			float num2 = Mathf.InverseLerp(worldMin.position.z, worldMax.position.z, value2);
			Vector2 anchoredPosition = new Vector2((num - 0.5f) * sizeDelta.x, (num2 - 0.5f) * sizeDelta.y);
			value.RectTransform.anchoredPosition = anchoredPosition;
		}
	}

	private void UpdateFightIcons()
	{
		foreach (GameObject mapFightIcon in mapFightIcons)
		{
			mapFightIcon.SetActive(MainGame.Instance.GameSave.knowledgeSystem.activeMapFightIcons.Contains(mapFightIcon.name));
		}
	}

	private void FocusOnPlayer()
	{
		Vector2 anchoredPosition = -playerIcon.anchoredPosition;
		Vector2 sizeDelta = scrollRect.content.sizeDelta;
		Vector2 size = scrollRect.viewport.rect.size;
		float min = (0f - (sizeDelta.x - size.x)) * 0.5f;
		float max = (sizeDelta.x - size.x) * 0.5f;
		float min2 = (0f - (sizeDelta.y - size.y)) * 0.5f;
		float max2 = (sizeDelta.y - size.y) * 0.5f;
		anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, min, max);
		anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, min2, max2);
		scrollRect.content.anchoredPosition = anchoredPosition;
	}

	protected override void TestDraw()
	{
	}
}
