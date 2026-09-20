using System.Collections.Generic;
using UnityEngine;

public class FloatingWorldGameObject : MonoBehaviour
{
	private WorldGameObject _wo;

	private static FloatingWorldGameObject _cur_floating = null;

	private const int MASK_SIZE = 51;

	private bool[,] _mask = new bool[51, 51];

	public Vector2 center_offsest = Vector2.zero;

	private bool _need_obj_mask_update = true;

	public static bool can_be_built = false;

	private static List<FlowGridCell> _cells = new List<FlowGridCell>();

	private static List<GameObject> _docks = new List<GameObject>();

	public WorldGameObject wobj => _wo;

	public static FloatingWorldGameObject cur_floating => _cur_floating;

	public static Vector3 cur_floating_pos
	{
		get
		{
			if (!(_cur_floating != null))
			{
				return Vector3.zero;
			}
			return _cur_floating.transform.position;
		}
	}

	public static FloatingWorldGameObject CreateFloatingObject(WorldGameObject prefab)
	{
		WorldGameObject worldGameObject = prefab.Copy(LazyEngine.world_root);
		worldGameObject.GetComponent<RoundAndSortComponent>().grid_divider = 3;
		FloatingWorldGameObject floatingWorldGameObject = worldGameObject.gameObject.AddComponent<FloatingWorldGameObject>();
		floatingWorldGameObject._wo = worldGameObject;
		_cur_floating = floatingWorldGameObject;
		worldGameObject.SetBuildingColor(new Color(1f, 1f, 1f, 0.7f));
		floatingWorldGameObject.RecalculateAvailability();
		floatingWorldGameObject.UpdateGridColor();
		WorldMap.ActivateGameObject(worldGameObject.gameObject);
		return floatingWorldGameObject;
	}

	public static void StopCurrentFloating(bool leave_on_scene = false)
	{
		if (_cur_floating == null)
		{
			return;
		}
		_cur_floating._wo.CancelSortOverEverything();
		InteractionBubbleGUI.RemoveBubble(_cur_floating.wobj.unique_id);
		Debug.Log("StopCurrentFloating, leave_on_scene = " + leave_on_scene);
		if (leave_on_scene)
		{
			_cur_floating.wobj.GetComponent<RoundAndSortComponent>().SetZ();
			_cur_floating.UpdateRoundAndSort();
			DestroyFlowGridCells();
			_cur_floating.EnableAllColliders();
			_cur_floating.DestroyComponent();
			_cur_floating.wobj.UpdatePathCell();
			_cur_floating.wobj.RecalculateZoneBelonging();
			can_be_built = true;
			_cur_floating.UpdateGridColor();
			WorldZone myWorldZone = _cur_floating.wobj.GetMyWorldZone();
			if (myWorldZone != null)
			{
				myWorldZone.Recalculate();
			}
		}
		else
		{
			Object.Destroy(_cur_floating.gameObject);
		}
		_cur_floating = null;
		LazyEngine.CancelCurrentItem();
	}

	private void EnableAllColliders(bool enable = true)
	{
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enable;
		}
	}

	private static void DestroyFlowGridCells()
	{
		foreach (FlowGridCell cell in _cells)
		{
			if (!(cell == null))
			{
				Object.Destroy(cell.gameObject);
			}
		}
		_cells.Clear();
		foreach (GameObject dock in _docks)
		{
			if (!(dock == null))
			{
				Object.Destroy(dock);
			}
		}
		_docks.Clear();
	}

	public static void MoveCurrentFloatingObject(Vector2 pos, bool is_global_pos = true, Vector2? direction_shift = null)
	{
		if (!(_cur_floating == null))
		{
			if (!is_global_pos)
			{
				pos -= _cur_floating.center_offsest;
			}
			_cur_floating.gameObject.SetActive(value: true);
			if (direction_shift.HasValue)
			{
				pos += new Vector2((direction_shift.Value.x > 0f) ? 1 : ((direction_shift.Value.x < 0f) ? (-1) : 0), (direction_shift.Value.y > 0f) ? ((int)GridUnderMovingObj.obj_size.y) : ((direction_shift.Value.y < 0f) ? (-1) : 0));
			}
			if (is_global_pos)
			{
				_cur_floating.wobj.MoveWhenPlacingGlobalPos(pos);
			}
			else
			{
				_cur_floating.wobj.MoveWhenPlacingLocalPos(pos);
			}
			_cur_floating.RecalculateAvailability();
			_cur_floating.UpdateGridColor();
		}
	}

	public static void RotateCurrentFloatingObject(bool rotate_right = true)
	{
		if (!(_cur_floating == null))
		{
			if (rotate_right)
			{
				_cur_floating._wo.NextVariationRadiobutton();
			}
			else
			{
				_cur_floating._wo.PrevVariationRadiobutton();
			}
			_cur_floating.RecalculateObjectMask();
			_cur_floating.RecalculateAvailability();
			_cur_floating.UpdateGridColor();
		}
	}

	public static void MoveCurrentByDir(Vector2 dir)
	{
		if (!(_cur_floating == null) && !dir.magnitude.EqualsTo(0f))
		{
			Vector3 position = _cur_floating.transform.position;
			position += (Vector3)dir.normalized * 32f;
			Vector2 vector = Camera.main.WorldToScreenPoint(position);
			if (!(vector.x < 0f) && !(vector.x > (float)Screen.width) && !(vector.y < 0f) && !(vector.y > (float)Screen.height))
			{
				_cur_floating.wobj.MoveWhenPlacingGlobalPos(position);
				_cur_floating.RecalculateAvailability();
				_cur_floating.UpdateGridColor();
			}
		}
	}

	public static bool IsFloating()
	{
		return _cur_floating != null;
	}

	public void UpdateObjSize()
	{
		if (!(_cur_floating == null))
		{
			Debug.Log("FloatingWorldGameObject.UpdateObjSize");
			_need_obj_mask_update = true;
		}
	}

	public void RecalculateObjectMask()
	{
		Debug.Log("FloatingWorldGameObject.RecalculateObjectMask - calculating obj size");
		_need_obj_mask_update = false;
		Vector2 vector = base.transform.position;
		Collider2D[] componentsInChildren = GetComponentsInChildren<Collider2D>(includeInactive: true);
		List<Collider2D> list = new List<Collider2D>();
		List<Collider2D> list2 = new List<Collider2D>();
		Collider2D[] array = componentsInChildren;
		foreach (Collider2D collider2D in array)
		{
			if (!BuildGrid.SkipCollider(collider2D))
			{
				list.Add(collider2D);
				if (!collider2D.enabled)
				{
					collider2D.enabled = true;
					list2.Add(collider2D);
				}
			}
		}
		vector -= (new Vector2(51f, 51f) / 2f - Vector2.one * 0.5f) * 32f;
		for (int j = 0; j < 51; j++)
		{
			for (int k = 0; k < 51; k++)
			{
				_mask[j, 51 - k - 1] = false;
				Vector2 pos = vector + new Vector2(j * 32, k * 32);
				_mask[j, 51 - k - 1] = BuildGrid.IsCellBusy(pos, list);
			}
		}
		DrawFlowGrid();
		wobj.RecalculateGridShape();
		foreach (Collider2D item in list2)
		{
			item.enabled = false;
		}
	}

	public static FloatingWorldGameObject CreateFloatingWorldObjectById(string obj_id)
	{
		FloatingWorldGameObject floatingWorldGameObject = CreateFloatingObject(Prefabs.wgo_prefab);
		if (!string.IsNullOrEmpty(obj_id))
		{
			floatingWorldGameObject.wobj.SetObject(obj_id);
			floatingWorldGameObject.wobj.ForceInitOptimizedColliders();
			floatingWorldGameObject.UpdateObjSize();
		}
		if (BuildGrid.current_sub_zone_configuration != null && BuildGrid.current_sub_zone_configuration.sort_floating_over_everything)
		{
			floatingWorldGameObject._wo.SetSortOverEverything();
		}
		MoveCurrentFloatingObject(MainGame.me.player.transform.localPosition, is_global_pos: false);
		_cur_floating = floatingWorldGameObject;
		ObjectDynamicShadow[] componentsInChildren = floatingWorldGameObject.GetComponentsInChildren<ObjectDynamicShadow>();
		foreach (ObjectDynamicShadow obj in componentsInChildren)
		{
			obj.Start();
			obj.Awake();
		}
		OptimizedCollider2D[] componentsInChildren2 = floatingWorldGameObject.GetComponentsInChildren<OptimizedCollider2D>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].Init();
		}
		floatingWorldGameObject.RecalculateObjectMask();
		floatingWorldGameObject.EnableAllColliders(enable: false);
		return floatingWorldGameObject;
	}

	public static WorldGameObject GetWGOUnderFloatingCursor()
	{
		Collider2D[] array = Physics2D.OverlapBoxAll(_cur_floating.gameObject.GetComponentInChildren<FlowGridCell>().transform.position, Vector2.one * 0.9f * 96f, 0f, 1);
		if (array.Length == 0)
		{
			return null;
		}
		return array[0].GetComponentInParent<WorldGameObject>();
	}

	private void UpdateRoundAndSort()
	{
		RoundAndSortComponent[] componentsInChildren = wobj.GetComponentsInChildren<RoundAndSortComponent>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].DoUpdateStuff(force: true);
		}
	}

	public void Update()
	{
		if (_need_obj_mask_update)
		{
			RecalculateObjectMask();
		}
	}

	public void DrawFlowGrid()
	{
		Debug.Log("FloatingWorldGameObject.DrawFlowGrid");
		DestroyFlowGridCells();
		if (wobj.obj_def == null)
		{
			Debug.LogError("Obj def is null, id = " + wobj.obj_id);
			return;
		}
		Transform parent_tr = base.transform;
		bool flag = wobj.obj_def.IsTotem();
		float totem_radius = wobj.obj_def.totem_radius;
		Debug.Log("Is totem = " + flag + ", totem_r = " + totem_radius);
		Vector2 vector = Vector2.one * float.MaxValue;
		Vector2 vector2 = Vector2.one * float.MinValue;
		float num = 25f;
		bool flag2 = center_offsest.magnitude.EqualsTo(0f);
		for (int i = 0; i < 51; i++)
		{
			float x = (float)i - num;
			for (int j = 0; j < 51; j++)
			{
				float y = (float)(-j) + num;
				FlowGridCell.CellType cellType = FlowGridCell.CellType.None;
				Vector2 pos = new Vector2(x, y);
				if (_mask[i, j])
				{
					cellType = FlowGridCell.CellType.UnderObject;
				}
				if (cellType == FlowGridCell.CellType.None)
				{
					continue;
				}
				pos /= 3f;
				FlowGridCell item = FlowGridCell.Create(parent_tr, pos, 3, cellType);
				_cells.Add(item);
				if (flag2 && cellType == FlowGridCell.CellType.UnderObject)
				{
					if (pos.x < vector.x)
					{
						vector.x = pos.x;
					}
					if (pos.x > vector2.x)
					{
						vector2.x = pos.x;
					}
					if (pos.y < vector.y)
					{
						vector.y = pos.y;
					}
					if (pos.y > vector2.y)
					{
						vector2.y = pos.y;
					}
				}
			}
		}
		DockPoint[] componentsInChildren = _wo.GetComponentsInChildren<DockPoint>();
		foreach (DockPoint dockPoint in componentsInChildren)
		{
			GameObject gameObject = Object.Instantiate(MainGame.me.dock_point_marker.GetMarker(dockPoint.GetActionDir()));
			gameObject.transform.SetParent(_wo.gameObject.transform, worldPositionStays: false);
			gameObject.transform.position = dockPoint.transform.position;
			_docks.Add(gameObject);
		}
		if (flag2)
		{
			Transform transform = ((wobj.wop != null) ? wobj.wop.center : null);
			if (transform == null)
			{
				Vector3 vector3 = vector2 - vector;
				center_offsest.x = vector.x + Mathf.Abs(vector3.x) / 2f;
				center_offsest.y = vector.y + Mathf.Abs(vector3.y) / 2f;
			}
			else
			{
				center_offsest = (transform.position - wobj.tf.position) / 96f;
			}
		}
	}

	public void RecalculateAvailability()
	{
		can_be_built = true;
		foreach (FlowGridCell cell in _cells)
		{
			if (!(cell == null) && !(cell.gameObject == null) && cell.gameObject.activeSelf && cell.cell_type != FlowGridCell.CellType.TotemArea)
			{
				bool flag = !BuildGrid.IsCellBusy(cell.transform.position) && cell.IsInsideWorldZone(MainGame.me.build_mode_logics.cur_build_zone_id, BuildGrid.GetCurrentSubZoneID());
				can_be_built &= flag;
			}
		}
	}

	public void UpdateGridColor()
	{
		if (wobj != null)
		{
			wobj.SetBuildingColor(can_be_built ? Color.white : Color.red);
		}
		foreach (FlowGridCell cell in _cells)
		{
			if (!(cell == null) && !(cell.gameObject == null) && cell.gameObject.activeSelf)
			{
				cell.SetRedColorState(!can_be_built);
			}
		}
	}

	public static bool IsObjectRotatable()
	{
		if (cur_floating == null || cur_floating.wobj == null)
		{
			return false;
		}
		return cur_floating.wobj.CanBeRotatedWhilePlacing();
	}
}
