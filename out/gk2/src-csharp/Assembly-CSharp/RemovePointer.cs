using System.Collections.Generic;
using System.Linq;
using LazyBearTechnology;
using UnityEngine;
using UnityEngine.UI;

public class RemovePointer : BuildPointerObject, ICursorChanger
{
	private static readonly Dictionary<IBuildRemovable, Rect> markedForRemovingObjectRects = new Dictionary<IBuildRemovable, Rect>();

	private static Rect currentSelectedObjectRect = Rect.zero;

	private static IBuildRemovable currentRemovingSelection = null;

	private float selectionTintAmount = 0.3f;

	private Color selectionTintColor = Color.red;

	private Color unDestroyableSelectionTintColor = Color.gray;

	private string selectionTintColorHex = "#ca2b00";

	private string unDestroyableSelectionTintColorHex = "#444e50";

	private readonly HashSet<IBuildRemovable> highlightedRemovables = new HashSet<IBuildRemovable>();

	private readonly HashSet<IBuildRemovable> unDestroyableRemovables = new HashSet<IBuildRemovable>();

	private Vector3 position;

	private bool doFixedUpdate;

	private int previousRemovableFoundState = -1;

	private BuildingDef currentSelectedBuildingDef;

	private Canvas gamepadCursorCanvas;

	private RectTransform gamepadCursorRect;

	private Image gamepadCursorImage;

	private Sprite defaultCursorSprite;

	private Sprite destroyCursorSprite;

	private Vector2 defaultCursorPivot;

	private Vector2 destroyCursorPivot;

	private bool gamepadCursorReady;

	public override void Rotate()
	{
	}

	public override bool TryDoBuildAction()
	{
		if (currentRemovingSelection != null && currentRemovingSelection.IsBuildRemovable())
		{
			bool flag = currentRemovingSelection.DoBuildRemove();
			if (currentSelectedBuildingDef != null && currentRemovingSelection is Wgo wgo)
			{
				foreach (LazyExpression item in currentSelectedBuildingDef.expressionAfterBuilding)
				{
					item.EvaluateBool(wgo.Data);
				}
			}
			if (!flag)
			{
				if (markedForRemovingObjectRects.TryGetValue(currentRemovingSelection, out var _))
				{
					markedForRemovingObjectRects.Remove(currentRemovingSelection);
				}
				else
				{
					markedForRemovingObjectRects.Add(currentRemovingSelection, currentSelectedObjectRect);
				}
			}
			else
			{
				UpdateUnDestroyableRemovables();
				currentRemovingSelection = null;
				currentSelectedObjectRect = Rect.zero;
			}
			UpdateSelectionRect();
			return flag;
		}
		return false;
	}

	public override void SetupSelectionCells(BuildSelectionCell[] prefabCells, Transform parent)
	{
		LazyInput.OnInputChanged += UpdateGamepadCursorState;
		ColorUtility.TryParseHtmlString(selectionTintColorHex, out selectionTintColor);
		ColorUtility.TryParseHtmlString(unDestroyableSelectionTintColorHex, out unDestroyableSelectionTintColor);
		UpdateUnDestroyableRemovables();
		UpdateSelectionRect();
		UpdateUnDestroyableSelectionTint();
		SetupGamepadCursor();
		UpdateGamepadCursorState();
	}

	public void UpdateUnDestroyableRemovables()
	{
		foreach (IBuildRemovable unDestroyableRemovable in unDestroyableRemovables)
		{
			ApplySelectionTint(unDestroyableRemovable, 0f, Color.white);
		}
		unDestroyableRemovables.Clear();
		foreach (Wgo wgo in LazySingleton<BuildManager>.Instance.WorldZone.Wgos)
		{
			if (wgo.IsBuildRemovable() && wgo.Data.CraftComponent.IsDestroyingCraftActive)
			{
				markedForRemovingObjectRects[wgo] = GetRectForWgo(wgo);
			}
			if (!wgo.IsBuildRemovable())
			{
				unDestroyableRemovables.Add(wgo);
			}
			GameBalance.Me.removableWgos.TryGetValue(wgo.Data.id, out var value);
			if (value != null && value.chooseCustomBuildAreaType == BuildingDef.BuildAreaChoosingType.Strict && HasBuiltWgoOnAnyBuildArea(wgo))
			{
				unDestroyableRemovables.Add(wgo);
			}
		}
	}

	public override void UpdateSelectionCellsState()
	{
		base.UpdateSelectionCellsState();
	}

	public override Vector3 GetCellsCenterLocal()
	{
		return new Vector3(BuildConsts.CELL_SIZE.x, 0f, 0f - BuildConsts.CELL_SIZE.y) / 2f;
	}

	public override void UpdatePosition(Vector3 position)
	{
		this.position = position;
		doFixedUpdate = true;
	}

	private void LateUpdate()
	{
		UpdateGamepadCursorVisual();
	}

	private void FixedUpdate()
	{
		if (doFixedUpdate)
		{
			doFixedUpdate = false;
			UpdateHoverFromOverlapCenter(GetSnappedOverlapCenter());
		}
	}

	private Vector3 GetSnappedOverlapCenter()
	{
		return position + new Vector3(BuildConsts.CELL_SIZE.x, 0f, 0f - BuildConsts.CELL_SIZE.y) / 2f + Vector3.up * 0.01f;
	}

	private void UpdateHoverFromOverlapCenter(Vector3 overlapCenter)
	{
		int num = 0;
		IBuildRemovable buildRemovable = null;
		Collider[] array = new Collider[20];
		int num2 = Physics.OverlapBoxNonAlloc(overlapCenter, Vector3.Scale(BuildConsts.CASTING_BOX_HALF_EXTENTS, new Vector3(0.1f, 1f, 0.1f)), array, Quaternion.identity, 655616);
		Bounds bounds = default(Bounds);
		bool flag = false;
		for (int i = 0; i < num2; i++)
		{
			Collider collider = array[i];
			if (collider == null)
			{
				continue;
			}
			int layer = collider.gameObject.layer;
			if ((layer != 8 && layer != 19) || (collider.TryGetComponent<BuildArea>(out var component) && (component.foprceShowAsBuffAreaForPointerPlacement || component.ignoreForPointerPlacement)) || PlacementBlockingArea.TryGet(collider, out var _))
			{
				continue;
			}
			IBuildRemovable componentInParent = collider.GetComponentInParent<IBuildRemovable>();
			if (componentInParent != null && !IsFromCurrentWorldZone(componentInParent))
			{
				continue;
			}
			if (num < 2)
			{
				num = ((componentInParent != null) ? 1 : 0);
			}
			if (componentInParent != null && componentInParent.IsBuildRemovable() && !unDestroyableRemovables.Contains(componentInParent))
			{
				buildRemovable = componentInParent;
				num = 2;
				if (!flag)
				{
					bounds = collider.bounds;
					flag = true;
				}
				else
				{
					bounds.Encapsulate(collider.bounds);
				}
			}
		}
		Rect rect = (flag ? new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z) : Rect.zero);
		IBuildRemovable obj = ((num == 2) ? buildRemovable : null);
		bool flag2 = obj != currentRemovingSelection || currentSelectedObjectRect != rect || num != previousRemovableFoundState;
		currentSelectedObjectRect = rect;
		currentRemovingSelection = obj;
		currentSelectedBuildingDef = null;
		if (currentRemovingSelection is Wgo wgo)
		{
			GameBalance.Me.removableWgos.TryGetValue(wgo.Data.id, out currentSelectedBuildingDef);
		}
		if (flag2)
		{
			UpdateSelectionRect();
		}
		if (num != previousRemovableFoundState)
		{
			ApplyCursorHoverState(num);
			previousRemovableFoundState = num;
		}
	}

	private void ApplyCursorHoverState(int removableFoundState)
	{
		if (!LazyInput.IsGamepadActive)
		{
			CursorController.RemoveCursorState(this);
			if (removableFoundState == 2)
			{
				CursorController.AddCursorState(CursorType.BuildingModeDestroy, this);
			}
		}
		else
		{
			CursorController.RemoveCursorState(this);
		}
		RefreshGamepadCursorSprite(removableFoundState == 2);
	}

	private void UpdateSelectionRect()
	{
		List<Rect> list = markedForRemovingObjectRects.Values.ToList();
		List<IBuildRemovable> removables = markedForRemovingObjectRects.Keys.ToList();
		if (currentRemovingSelection != null && currentRemovingSelection.IsBuildRemovable() && !markedForRemovingObjectRects.ContainsKey(currentRemovingSelection))
		{
			list.Add(currentSelectedObjectRect);
		}
		LazySingleton<BuildManager>.Instance.BuildController.BuildLayout.UpdateSelection(list);
		UpdateSelectionTint(removables);
	}

	private void UpdateUnDestroyableSelectionTint()
	{
		foreach (IBuildRemovable unDestroyableRemovable in unDestroyableRemovables)
		{
			ApplySelectionTint(unDestroyableRemovable, 0.4f, unDestroyableSelectionTintColor);
		}
	}

	public override void OnPointerDisable()
	{
		base.OnPointerDisable();
		CursorController.RemoveCursorState(this);
		ClearSelectionTint();
		markedForRemovingObjectRects.Clear();
		currentSelectedBuildingDef = null;
		LazyInput.OnInputChanged -= UpdateGamepadCursorState;
		DestroyGamepadCursor();
	}

	private void OnDestroy()
	{
		LazyInput.OnInputChanged -= UpdateGamepadCursorState;
		DestroyGamepadCursor();
	}

	private void UpdateSelectionTint(List<IBuildRemovable> removables)
	{
		foreach (IBuildRemovable highlightedRemovable in highlightedRemovables)
		{
			ApplySelectionTint(highlightedRemovable, 0f, selectionTintColor);
		}
		highlightedRemovables.Clear();
		foreach (IBuildRemovable removable in removables)
		{
			if (IsAliveRemovable(removable))
			{
				ApplySelectionTint(removable, selectionTintAmount, selectionTintColor);
				highlightedRemovables.Add(removable);
			}
		}
	}

	private void ClearSelectionTint()
	{
		foreach (IBuildRemovable highlightedRemovable in highlightedRemovables)
		{
			ApplySelectionTint(highlightedRemovable, 0f, Color.white);
		}
		foreach (IBuildRemovable unDestroyableRemovable in unDestroyableRemovables)
		{
			ApplySelectionTint(unDestroyableRemovable, 0f, Color.white);
		}
		highlightedRemovables.Clear();
		unDestroyableRemovables.Clear();
	}

	private static bool IsAliveRemovable(IBuildRemovable removable)
	{
		if (removable == null)
		{
			return false;
		}
		if (removable is Object @object && @object == null)
		{
			return false;
		}
		return true;
	}

	private void ApplySelectionTint(IBuildRemovable removable, float amount, Color color)
	{
		if (IsAliveRemovable(removable) && TryGetWgo(removable, out var wgo))
		{
			wgo.SetSelectionTint(color, amount);
		}
	}

	private static bool TryGetWgo(IBuildRemovable removable, out Wgo wgo)
	{
		if (removable is Wgo wgo2)
		{
			wgo = wgo2;
			return true;
		}
		if (removable is Component component)
		{
			wgo = component.GetComponentInParent<Wgo>();
			return wgo != null;
		}
		wgo = null;
		return false;
	}

	private Rect GetRectForWgo(Wgo wgo)
	{
		bool flag = false;
		Bounds bounds = default(Bounds);
		Collider[] componentsInChildren = wgo.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider.gameObject.layer == 19)
			{
				if (!flag)
				{
					flag = true;
					bounds = collider.bounds;
				}
				else
				{
					bounds.Encapsulate(collider.bounds);
				}
			}
		}
		if (!flag)
		{
			return Rect.zero;
		}
		return new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
	}

	private static bool HasBuiltWgoOnAnyBuildArea(Wgo currentWgo)
	{
		if (currentWgo?.Data == null)
		{
			return false;
		}
		WorldZoneData worldZoneData = currentWgo.Data.WorldZoneData ?? LazySingleton<BuildManager>.Instance.WorldZone?.Data;
		if (worldZoneData == null)
		{
			return false;
		}
		HashSet<WgoData> hashSet = new HashSet<WgoData>();
		Collider[] componentsInChildren = currentWgo.GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren)
		{
			if (collider == null || collider.gameObject.layer != 19 || PlacementBlockingArea.TryGet(collider, out var _))
			{
				continue;
			}
			Bounds bounds = collider.bounds;
			Rect rect = new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
			foreach (WgoData item in worldZoneData.GetWgoDataByRect(rect))
			{
				hashSet.Add(item);
			}
		}
		GameBalance.Me.buildableWgos.TryGetValue(currentWgo.Data.id, out var value);
		Debug.Log($"WGOs in rect: {currentWgo.Data.id} {hashSet.Count}");
		foreach (WgoData item2 in hashSet)
		{
			if (item2 != null && !(item2.UniqueId == currentWgo.Data.UniqueId) && (value == null || !value.ShouldIgnoreWgoGroupAsObstacle(item2.Definition.wgoGroup)) && (GameBalance.Me.buildableWgos.ContainsKey(item2.id) || GameBalance.Me.removableWgos.ContainsKey(item2.id)))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsFromCurrentWorldZone(IBuildRemovable removable)
	{
		if (!TryGetWgo(removable, out var wgo))
		{
			return true;
		}
		WorldZoneData worldZoneData = LazySingleton<BuildManager>.Instance.WorldZone?.Data;
		WorldZoneData worldZoneData2 = wgo.Data?.WorldZoneData;
		if (worldZoneData == null || worldZoneData2 == null)
		{
			return true;
		}
		if (worldZoneData != worldZoneData2)
		{
			return worldZoneData.id == worldZoneData2.id;
		}
		return true;
	}

	private void SetupGamepadCursor()
	{
		if (!TryCreateCursorSprite(CursorType.Default, out defaultCursorSprite, out defaultCursorPivot))
		{
			Debug.LogError("RemovePointer: default cursor texture is missing on CursorController. Assign it on Assets/Prefabs/Systems/CursorController.prefab.");
			return;
		}
		if (!TryCreateCursorSprite(CursorType.BuildingModeDestroy, out destroyCursorSprite, out destroyCursorPivot))
		{
			destroyCursorSprite = defaultCursorSprite;
		}
		GameObject gameObject = new GameObject("RemovePointerGamepadCursor");
		gamepadCursorCanvas = gameObject.AddComponent<Canvas>();
		gamepadCursorCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		gamepadCursorCanvas.pixelPerfect = true;
		gamepadCursorCanvas.sortingOrder = 701;
		gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		GameObject gameObject2 = new GameObject("Cursor");
		gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: false);
		gamepadCursorImage = gameObject2.AddComponent<Image>();
		gamepadCursorImage.raycastTarget = false;
		gamepadCursorImage.preserveAspect = true;
		gamepadCursorRect = gamepadCursorImage.rectTransform;
		gamepadCursorRect.anchorMin = Vector2.zero;
		gamepadCursorRect.anchorMax = Vector2.zero;
		gamepadCursorReady = true;
		RefreshGamepadCursorSprite(hoverRemovable: false);
	}

	private static bool TryCreateCursorSprite(CursorType type, out Sprite sprite, out Vector2 pivot)
	{
		sprite = null;
		pivot = new Vector2(0f, 1f);
		if (!CursorController.TryGetCursorConfiguration(type, out var configuration))
		{
			return false;
		}
		Texture2D sprite2 = configuration.sprite;
		if (sprite2 == null)
		{
			return false;
		}
		if (sprite2.width > 0 && sprite2.height > 0)
		{
			pivot = new Vector2(configuration.hotSpot.x / (float)sprite2.width, 1f - configuration.hotSpot.y / (float)sprite2.height);
		}
		sprite = Sprite.Create(sprite2, new Rect(0f, 0f, sprite2.width, sprite2.height), pivot, 100f, 0u, SpriteMeshType.FullRect);
		sprite.name = sprite2.name + "_RemovePointer";
		return true;
	}

	private void RefreshGamepadCursorSprite(bool hoverRemovable)
	{
		if (gamepadCursorReady && !(gamepadCursorImage == null))
		{
			Sprite sprite = ((hoverRemovable && destroyCursorSprite != null) ? destroyCursorSprite : defaultCursorSprite);
			Vector2 pivot = (hoverRemovable ? destroyCursorPivot : defaultCursorPivot);
			if (!(sprite == null))
			{
				gamepadCursorImage.sprite = sprite;
				gamepadCursorRect.pivot = pivot;
				gamepadCursorImage.SetNativeSize();
				float softwareCursorScale = CursorController.GetSoftwareCursorScale();
				gamepadCursorRect.localScale = Vector3.one * softwareCursorScale;
			}
		}
	}

	private void UpdateGamepadCursorVisual()
	{
		if (gamepadCursorReady && !(gamepadCursorRect == null))
		{
			bool isGamepadActive = LazyInput.IsGamepadActive;
			if (gamepadCursorCanvas != null && gamepadCursorCanvas.gameObject.activeSelf != isGamepadActive)
			{
				gamepadCursorCanvas.gameObject.SetActive(isGamepadActive);
			}
			if (isGamepadActive)
			{
				Vector3 cursorScreenPosition = GetCursorScreenPosition();
				cursorScreenPosition.z = 0f;
				gamepadCursorRect.position = cursorScreenPosition;
			}
		}
	}

	private void UpdateGamepadCursorState()
	{
		if (LazyInput.IsGamepadActive)
		{
			CursorController.RemoveCursorState(this);
			RefreshGamepadCursorSprite(previousRemovableFoundState == 2);
		}
		else
		{
			ApplyCursorHoverState(previousRemovableFoundState);
		}
		UpdateGamepadCursorVisual();
	}

	private static Vector3 GetCursorScreenPosition()
	{
		if (BuildController.Instance != null)
		{
			return BuildController.Instance.LastCursorScreenPosition;
		}
		return Input.mousePosition;
	}

	private void DestroyGamepadCursor()
	{
		if (gamepadCursorCanvas != null)
		{
			Object.Destroy(gamepadCursorCanvas.gameObject);
		}
		gamepadCursorCanvas = null;
		gamepadCursorRect = null;
		gamepadCursorImage = null;
		gamepadCursorReady = false;
		if (defaultCursorSprite != null)
		{
			Object.Destroy(defaultCursorSprite);
		}
		if (destroyCursorSprite != null && destroyCursorSprite != defaultCursorSprite)
		{
			Object.Destroy(destroyCursorSprite);
		}
		defaultCursorSprite = null;
		destroyCursorSprite = null;
	}
}
