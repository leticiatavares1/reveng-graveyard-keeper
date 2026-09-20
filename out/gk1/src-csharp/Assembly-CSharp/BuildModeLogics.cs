using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BuildModeLogics
{
	private enum Mode
	{
		None,
		Placing,
		Removing,
		PickUping,
		ScriptBuilding
	}

	private const float INNER_BORDERS = 96f;

	private const float OUTER_BORDERS = 96f;

	private const float CAMS_STEP = 32f;

	private Mode _mode;

	private Vector2 _mouse_pos = Vector2.zero;

	private ObjectCraftDefinition _cd;

	private Vector2 _mouse_dpos = Vector2.zero;

	private string _cur_build_zone_id;

	private WorldZone _cur_build_zone;

	private Bounds _cur_build_zone_bounds;

	private Bounds _outer_visible_bounds;

	private Vector3 _cam_half_size;

	private MultiInventory _multi_inventory;

	private Transform _zone_camera_tf;

	public static WorldGameObject last_build_desk;

	public string building_obj_bubble;

	public Dictionary<GameKey, Vector2> gamepad_directions = new Dictionary<GameKey, Vector2>
	{
		{
			GameKey.Left,
			Vector2.left
		},
		{
			GameKey.Right,
			Vector2.right
		},
		{
			GameKey.Up,
			Vector2.up
		},
		{
			GameKey.Down,
			Vector2.down
		}
	};

	private List<WorldGameObject> _wgos_with_marks = new List<WorldGameObject>();

	private GameObject _cur_remove_group;

	private GameObject _remove_grey_spr;

	private Vector2 _last_obj_pos = Vector2.zero;

	private bool _break_point;

	public WorldZone cur_build_zone => _cur_build_zone;

	public string cur_build_zone_id => _cur_build_zone_id;

	public MultiInventory multi_inventory => _multi_inventory;

	public static event Action on_cancel_while_script_building;

	public static event Action on_apply_while_script_building;

	public static event Action on_rotate_left_while_script_building;

	public static event Action on_rotate_right_while_script_building;

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			_break_point = !_break_point;
		}
		_mouse_dpos = _mouse_pos - (Vector2)Input.mousePosition;
		_mouse_pos = Input.mousePosition;
		switch (_mode)
		{
		case Mode.Placing:
			UpdateWhilePlacing();
			break;
		case Mode.Removing:
			UpdateWhileRemoving();
			break;
		case Mode.ScriptBuilding:
			UpdateWhileScriptBuilding();
			break;
		case Mode.PickUping:
			break;
		}
	}

	private void UpdateWhilePlacing()
	{
		if (LazyInput.GetKeyDown(GameKey.Back) || LazyInput.GetKeyDown(GameKey.RightClick))
		{
			CancelPlacing();
			LazyInput.ClearKeyDown(GameKey.Back);
			return;
		}
		ProcessMovement();
		Vector3 position = FloatingWorldGameObject.cur_floating.wobj.transform.position;
		if (!_last_obj_pos.x.EqualsTo(position.x) || !_last_obj_pos.y.EqualsTo(position.y))
		{
			BuildGrid.me.RedrawTotemRadius(FloatingWorldGameObject.cur_floating.wobj, FloatingWorldGameObject.cur_floating.center_offsest);
			_last_obj_pos = position;
			FloatingWorldGameObject.cur_floating.wobj.RefreshPositionCache();
			cur_build_zone.RecalculateTotems();
			cur_build_zone.RedrawQualities(true, separate_k: true);
		}
		if (LazyInput.GetKeyDown(GameKey.RotateLeft))
		{
			FloatingWorldGameObject.RotateCurrentFloatingObject(rotate_right: false);
			LazyInput.ClearKeyDown(GameKey.RotateLeft);
		}
		else if (LazyInput.GetKeyDown(GameKey.RotateRight))
		{
			FloatingWorldGameObject.RotateCurrentFloatingObject();
			LazyInput.ClearKeyDown(GameKey.RotateRight);
		}
		if ((LazyInput.gamepad_active && LazyInput.GetKeyDown(GameKey.Interaction)) || (!LazyInput.gamepad_active && LazyInput.GetKeyDown(GameKey.LeftClick)))
		{
			DoPlace();
		}
		GUIElements.me.build_mode_gui.RedrawPlacing(FloatingWorldGameObject.can_be_built && CanBuild(_cd), FloatingWorldGameObject.IsObjectRotatable());
	}

	private void UpdateWhileRemoving()
	{
		if (LazyInput.GetKeyDown(GameKey.Back) || LazyInput.GetKeyDown(GameKey.RightClick))
		{
			CancelRemoving();
			LazyInput.ClearKeyDown(GameKey.Back);
			return;
		}
		ProcessMovement();
		if ((LazyInput.gamepad_active && LazyInput.GetKeyDown(GameKey.Interaction)) || (!LazyInput.gamepad_active && LazyInput.GetKeyDown(GameKey.LeftClick)))
		{
			DoRemove();
		}
		WorldGameObject wGOUnderFloatingCursor = FloatingWorldGameObject.GetWGOUnderFloatingCursor();
		if (wGOUnderFloatingCursor == null)
		{
			GUIElements.me.build_mode_gui.RedrawRemoving(waiting_for_removing: false, can_be_removed: false);
		}
		else
		{
			GUIElements.me.build_mode_gui.RedrawRemoving(wGOUnderFloatingCursor.is_removing, wGOUnderFloatingCursor.has_removal_craft);
		}
	}

	private void UpdateWhileScriptBuilding()
	{
		if (LazyInput.GetKeyDown(GameKey.Back) || LazyInput.GetKeyDown(GameKey.RightClick))
		{
			MainGame.me.player.AddToInventory(_cd.needs);
			MainGame.me.build_mode_logics.SetCurrentBuildZone(string.Empty);
			BuildModeLogics.on_cancel_while_script_building?.Invoke();
			GUIElements.me.build_mode_gui.Hide();
			MainGame.me.ExitBuildMode();
			GUIElements.me.craft.Hide();
			_mode = Mode.None;
			RemoveMarksFromAllWGOs();
			MainGame.me.ExitBuildMode();
			MainGame.me.OpenBuildObjectGUI(last_build_desk);
			cur_build_zone.RedrawQualities(false);
			LazyInput.ClearKeyDown(GameKey.Back);
		}
		else if ((LazyInput.gamepad_active && LazyInput.GetKeyDown(GameKey.Interaction)) || (!LazyInput.gamepad_active && LazyInput.GetKeyDown(GameKey.LeftClick)))
		{
			MainGame.me.build_mode_logics.SetCurrentBuildZone(string.Empty);
			BuildModeLogics.on_apply_while_script_building?.Invoke();
			GUIElements.me.build_mode_gui.Hide();
			_mode = Mode.None;
			GUIElements.me.craft.Hide();
			MainGame.me.ExitBuildMode();
		}
		else
		{
			if (LazyInput.GetKeyDown(GameKey.RotateLeft) && _cd.has_variations)
			{
				BuildModeLogics.on_rotate_left_while_script_building?.Invoke();
				LazyInput.ClearKeyDown(GameKey.RotateLeft);
			}
			else if (LazyInput.GetKeyDown(GameKey.RotateRight) && _cd.has_variations)
			{
				BuildModeLogics.on_rotate_right_while_script_building?.Invoke();
				LazyInput.ClearKeyDown(GameKey.RotateRight);
			}
			GUIElements.me.build_mode_gui.RedrawScriptMode(_cd.has_variations);
		}
	}

	private void ProcessMovement()
	{
		if (LazyInput.gamepad_active)
		{
			foreach (GameKey key in gamepad_directions.Keys)
			{
				if (LazyInput.GetKeyDown(key))
				{
					FloatingWorldGameObject.MoveCurrentByDir(gamepad_directions[key]);
				}
			}
		}
		else
		{
			MoveObjectToMouse();
		}
		CheckCameraZone();
	}

	private void CheckCameraZone()
	{
		_ = _break_point;
		Camera world_cam = MainGame.me.world_cam;
		float num = 1f;
		Bounds bounds = default(Bounds);
		bounds.min = world_cam.ScreenToWorldPoint(Vector3.zero);
		bounds.max = world_cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height) * num);
		Bounds bounds2 = bounds;
		Bounds bounds3 = bounds2;
		bounds3.min += Vector3.one * 96f;
		bounds3.max -= Vector3.one * 96f;
		Vector2 vector = (LazyInput.gamepad_active ? FloatingWorldGameObject.cur_floating_pos : MainGame.me.world_cam.ScreenToWorldPoint(Input.mousePosition));
		Vector3 zero = Vector3.zero;
		if (vector.x < bounds3.min.x)
		{
			zero += Vector3.left;
		}
		else if (vector.x > bounds3.max.x)
		{
			zero += Vector3.right;
		}
		if (vector.y < bounds3.min.y)
		{
			zero += Vector3.down;
		}
		else if (vector.y > bounds3.max.y)
		{
			zero += Vector3.up;
		}
		if (!zero.magnitude.EqualsTo(0f))
		{
			_cam_half_size = bounds2.extents / 2f;
			_zone_camera_tf.position = GetFitCameraPos(_zone_camera_tf.position + zero * 32f);
		}
	}

	private Vector3 GetFitCameraPos(Vector3 source_pos)
	{
		Bounds outer_visible_bounds = _outer_visible_bounds;
		outer_visible_bounds.min += _cam_half_size;
		outer_visible_bounds.max -= _cam_half_size;
		if (outer_visible_bounds.min.x > outer_visible_bounds.max.x)
		{
			source_pos.x = outer_visible_bounds.center.x;
		}
		else
		{
			if (source_pos.x < outer_visible_bounds.min.x)
			{
				source_pos.x = outer_visible_bounds.min.x;
			}
			if (source_pos.x > outer_visible_bounds.max.x)
			{
				source_pos.x = outer_visible_bounds.max.x;
			}
		}
		if (outer_visible_bounds.min.y > outer_visible_bounds.max.y)
		{
			source_pos.y = outer_visible_bounds.center.y;
		}
		else
		{
			if (source_pos.y < outer_visible_bounds.min.y)
			{
				source_pos.y = outer_visible_bounds.min.y;
			}
			if (source_pos.y > outer_visible_bounds.max.y)
			{
				source_pos.y = outer_visible_bounds.max.y;
			}
		}
		return source_pos;
	}

	private void MoveObjectToMouse()
	{
		FloatingWorldGameObject.MoveCurrentFloatingObject(Camera.main.ScreenToWorldPoint(_mouse_pos) / 96f, is_global_pos: false);
	}

	private void DoPlace()
	{
		Debug.Log("DoPlace");
		if (!FloatingWorldGameObject.can_be_built)
		{
			Debug.Log("can't build - place is busy");
			return;
		}
		if (!CanBuild(_cd))
		{
			Debug.Log("Not enough");
			return;
		}
		_cur_build_zone.Recalculate();
		_multi_inventory.RemoveItems(_cd.needs);
		Stats.DesignEvent("Build:" + _cd.out_obj);
		Vector3 cur_floating_pos = FloatingWorldGameObject.cur_floating_pos;
		WorldGameObject wobj = FloatingWorldGameObject.cur_floating.wobj;
		FloatingWorldGameObject.StopCurrentFloating(leave_on_scene: true);
		string obj_id = wobj.obj_id;
		if (GameBalance.me.GetDataOrNull<ObjectDefinition>(obj_id + "_place") != null)
		{
			wobj.ReplaceWithObject(obj_id + "_place", show_puff: true);
			wobj.ForceInitOptimizedColliders();
			Collider2D[] componentsInChildren = wobj.GetComponentsInChildren<Collider2D>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
		if (!string.IsNullOrEmpty(_cd.end_script))
		{
			if (_cd.end_script.Contains(":"))
			{
				CustomFlowScript customFlowScript = GS.RunFlowScript(_cd.end_script.Split(':')[0]);
				customFlowScript.StartBehaviour();
				if (_cd.end_script.Split(':').Length > 2)
				{
					customFlowScript.FireEvent(_cd.end_script.Split(':')[1], _cd.end_script.Split(':')[2]);
				}
				else
				{
					customFlowScript.FireEvent(_cd.end_script.Split(':')[1]);
				}
			}
			GS.RunFlowScript(_cd.end_script);
		}
		if (_cd.one_time_craft && !MainGame.me.save.completed_one_time_crafts.Contains(_cd.id))
		{
			Debug.Log(_cd.id);
			MainGame.me.save.completed_one_time_crafts.Add(_cd.id);
		}
		wobj.just_built = true;
		wobj.Redraw();
		if (CanBuild(_cd) && MainGame.me.player.GetParamInt("waiting_for_first_bureal") != 1 && MainGame.me.save.IsCraftVisible(_cd))
		{
			CraftBuilding(_cd);
			if (LazyInput.gamepad_active)
			{
				FloatingWorldGameObject.MoveCurrentFloatingObject(cur_floating_pos);
			}
			else
			{
				MoveObjectToMouse();
			}
		}
		else
		{
			Debug.Log("Not enough res for the next building");
			CancelPlacing();
		}
		if (MainGame.me.player.GetParamInt("waiting_for_first_bureal") == 1)
		{
			GUIElements.me.craft.OnClosePressed();
			return;
		}
		if (!LazyInput.gamepad_active)
		{
			MoveObjectToMouse();
		}
		BuildGrid.ReshowBuildGrid();
	}

	public bool CanBuild(CraftDefinition cd)
	{
		if (_multi_inventory != null && cd != null)
		{
			return _multi_inventory.IsEnoughItems(cd.needs);
		}
		return false;
	}

	private void OnBuildPressed()
	{
	}

	public bool IsNowActive()
	{
		return _mode != Mode.None;
	}

	public void EnterScriptBuilding()
	{
		_mode = Mode.ScriptBuilding;
	}

	public void EnterRemoveMode()
	{
		_mode = Mode.Removing;
		FloatingWorldGameObject.CreateFloatingWorldObjectById("_cursor");
		_last_obj_pos = new Vector2(-99999f, 99999f);
		if (LazyInput.gamepad_active)
		{
			FloatingWorldGameObject.MoveCurrentFloatingObject(MainGame.me.world_cam.transform.position);
		}
		RemoveMarksFromAllWGOs();
		WorldZone zoneByID = WorldZone.GetZoneByID(cur_build_zone_id);
		if (zoneByID == null)
		{
			Debug.LogError("WorldZone is null");
			return;
		}
		if (zoneByID.can_be_removed_group == null)
		{
			zoneByID.can_be_removed_group = new GameObject("can be removed group");
			zoneByID.can_be_removed_group.transform.SetParent(MainGame.me.world_root, worldPositionStays: false);
			zoneByID.can_be_removed_group.AddComponent<SortingGroup>().sortingLayerName = "over everything";
		}
		_cur_remove_group = zoneByID.can_be_removed_group;
		_cur_remove_group.GetComponent<SortingGroup>().enabled = true;
		foreach (WorldGameObject zoneWGO in zoneByID.GetZoneWGOs())
		{
			if (zoneWGO.has_removal_craft)
			{
				zoneWGO.MarkObjectAsCanBeRemoved(zoneByID.can_be_removed_group);
				_wgos_with_marks.Add(zoneWGO);
			}
		}
		GUIElements.me.hud.Hide();
		GUIElements.me.build_mode_gui.Open();
		if (_remove_grey_spr == null)
		{
			_remove_grey_spr = Resources.Load<GameObject>("remove_grey_spr").Copy();
			_remove_grey_spr.transform.SetParent(MainGame.me.world_root, worldPositionStays: false);
		}
		_remove_grey_spr.transform.position = zoneByID.transform.position;
		_remove_grey_spr.SetActive(value: true);
		BuildGrid.ShowBuildGrid(show: true, ignore_repeating: true);
	}

	private void RemoveMarksFromAllWGOs()
	{
		foreach (WorldGameObject wgos_with_mark in _wgos_with_marks)
		{
			if (!wgos_with_mark.is_removing)
			{
				wgos_with_mark.RemoveMark();
			}
		}
		_wgos_with_marks.Clear();
	}

	private void OnPickupPressed()
	{
	}

	public void EnterBuildMode()
	{
		_mode = Mode.None;
		_last_obj_pos = new Vector2(-99999f, 99999f);
	}

	private void CancelPlacing()
	{
		Debug.Log("CancelPlacing, cur mode = " + _mode);
		if (_mode == Mode.Placing)
		{
			if (_cd != null && !string.IsNullOrEmpty(_cd.sub_zone_id))
			{
				BuildGrid.ShowBuildGrid(show: true);
			}
			CancelCurrentMode();
		}
	}

	private void CancelRemoving()
	{
		Debug.Log("CancelRemoving, cur mode = " + _mode);
		if (_mode == Mode.Removing)
		{
			CancelCurrentMode();
			BuildGrid.ShowBuildGrid(show: true);
			_cur_remove_group.GetComponent<SortingGroup>().enabled = false;
			if (_remove_grey_spr != null)
			{
				_remove_grey_spr.SetActive(value: false);
			}
			WorldGameObject[] componentsInChildren = _cur_remove_group.GetComponentsInChildren<WorldGameObject>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].CancelCanBeRemoved();
			}
		}
	}

	private void CancelCurrentMode()
	{
		_mode = Mode.None;
		FloatingWorldGameObject.StopCurrentFloating();
		RemoveMarksFromAllWGOs();
		MainGame.me.ExitBuildMode();
		GUIElements.me.build_mode_gui.Hide();
		MainGame.me.OpenBuildObjectGUI(last_build_desk);
		cur_build_zone.RedrawQualities(false);
		BuildGrid.me.ClearPreviousTotemRadius(apply_colors: true);
	}

	private void OnRemoveObjectPressed()
	{
	}

	public void OnBuildCraftSelected(ObjectCraftDefinition cd, Vector3? spawn_pos = null)
	{
		MainGame.me.EnterBuildMode();
		MainGame.me.gui_elements.craft.Hide();
		Debug.Log("OnBuildCraftSelected " + cd.id + ", obj = " + cd.out_obj);
		MainGame.me.save.quests.CheckKeyQuests("build_" + cd.out_obj);
		_mode = Mode.Placing;
		_cd = cd;
		string text = cd.out_obj;
		if (text.Contains("_place") && GameBalance.me.GetData<ObjectDefinition>(text.Replace("_place", "")) != null)
		{
			text = text.Replace("_place", "");
		}
		if (!string.IsNullOrEmpty(cd.sub_zone_id))
		{
			BuildGrid.ShowBuildGrid(show: true, ignore_repeating: true, cd.sub_zone_id);
		}
		FloatingWorldGameObject.CreateFloatingWorldObjectById(text);
		SmartDrawer component = FloatingWorldGameObject.cur_floating.wobj.wop.GetComponent<SmartDrawer>();
		if (component != null)
		{
			component.Redraw(force: true);
		}
		FloatingWorldGameObject.cur_floating.wobj.RedrawBubble();
		if (spawn_pos.HasValue && spawn_pos.Value.magnitude > 0f)
		{
			FloatingWorldGameObject.MoveCurrentFloatingObject(spawn_pos.Value);
		}
		else if (LazyInput.gamepad_active)
		{
			FloatingWorldGameObject.MoveCurrentFloatingObject(MainGame.me.world_cam.transform.position);
		}
		MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
	}

	public static ObjectCraftDefinition GetObjectRemoveCraftDefinition(string obj_id)
	{
		foreach (ObjectCraftDefinition craft_obj_datum in GameBalance.me.craft_obj_data)
		{
			if (craft_obj_datum.out_obj == obj_id && craft_obj_datum.build_type == ObjectCraftDefinition.BuildType.Remove && !craft_obj_datum.locked_builders_ids.Contains(last_build_desk.obj_id))
			{
				return craft_obj_datum;
			}
		}
		return null;
	}

	public static ObjectCraftDefinition GetObjectPutCraftDefinition(string obj_id)
	{
		foreach (ObjectCraftDefinition craft_obj_datum in GameBalance.me.craft_obj_data)
		{
			if (craft_obj_datum.out_obj == obj_id && craft_obj_datum.build_type == ObjectCraftDefinition.BuildType.Put && (last_build_desk == null || !craft_obj_datum.locked_builders_ids.Contains(last_build_desk.obj_id)))
			{
				return craft_obj_datum;
			}
		}
		return null;
	}

	public void ProcessRemovingCraft(WorldGameObject wgo, float delta_time)
	{
		CraftDefinition objectRemoveCraftDefinition = GetObjectRemoveCraftDefinition(wgo.obj_id);
		if (objectRemoveCraftDefinition == null)
		{
			Debug.Log("no remove craft found");
			return;
		}
		CraftComponent craft = wgo.components.craft;
		if (craft == null)
		{
			Debug.LogError("Can't remove object without craft component", wgo);
			return;
		}
		if (!craft.is_crafting)
		{
			if (craft.crafts.IndexOf(objectRemoveCraftDefinition) == 0)
			{
				craft.crafts.Add(objectRemoveCraftDefinition);
			}
			craft.Craft(objectRemoveCraftDefinition);
		}
		craft.DoAction(MainGame.me.player, delta_time);
		if (!craft.is_crafting)
		{
			wgo.ProcessRemove();
		}
	}

	private void DoRemove()
	{
		WorldGameObject wGOUnderFloatingCursor = FloatingWorldGameObject.GetWGOUnderFloatingCursor();
		if (wGOUnderFloatingCursor == null)
		{
			Debug.Log("No object under cursor");
		}
		else
		{
			wGOUnderFloatingCursor.MarkForRemoval();
		}
	}

	protected bool MouseWasMoved()
	{
		return (double)_mouse_dpos.sqrMagnitude > 0.01;
	}

	public void SetCurrentBuildZone(string zone_id, string custom_sub_zone = "")
	{
		Debug.Log("SetCurrentBuildZone " + zone_id + ", cur = " + _cur_build_zone_id);
		if (_cur_build_zone_id == zone_id)
		{
			return;
		}
		if (string.IsNullOrEmpty(zone_id))
		{
			WorldZone.MarkZoneAsDirty(_cur_build_zone_id);
			WorldZone zoneByID = WorldZone.GetZoneByID(_cur_build_zone_id);
			if (zoneByID != null)
			{
				zoneByID.Recalculate();
			}
		}
		_cur_build_zone_id = zone_id;
		_multi_inventory = (string.IsNullOrEmpty(_cur_build_zone_id) ? null : MainGame.me.player.GetMultiInventory(null, cur_build_zone_id));
		FocusCameraOnBuildZone(zone_id);
		BuildGrid.ShowBuildGrid(!string.IsNullOrEmpty(zone_id), ignore_repeating: false, custom_sub_zone);
	}

	private void FocusCameraOnBuildZone(string zone_id)
	{
		if (string.IsNullOrEmpty(zone_id))
		{
			CameraTools.RestoreCameraTargets();
			return;
		}
		CameraTools.StoreCameraTargets();
		_cur_build_zone = WorldZone.GetZoneByID(zone_id);
		if (!(_cur_build_zone == null))
		{
			_cur_build_zone_bounds = _cur_build_zone.GetBounds();
			float num = 1f;
			Bounds bounds = default(Bounds);
			bounds.min = MainGame.me.world_cam.ScreenToWorldPoint(Vector3.zero);
			bounds.max = MainGame.me.world_cam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height) * num);
			Bounds bounds2 = bounds;
			_cam_half_size = bounds2.extents / 2f;
			_outer_visible_bounds = _cur_build_zone_bounds;
			_outer_visible_bounds.min += bounds2.extents / 2f - Vector3.one * 96f;
			_outer_visible_bounds.max += Vector3.one * 96f - bounds2.extents / 2f;
			string[] obj = new string[6] { "_cur_build_zone_bounds: ", null, null, null, null, null };
			bounds = _cur_build_zone_bounds;
			obj[1] = bounds.ToString();
			obj[2] = ", cam_bounds: ";
			bounds = bounds2;
			obj[3] = bounds.ToString();
			obj[4] = ", _outer_visible_bounds: ";
			bounds = _outer_visible_bounds;
			obj[5] = bounds.ToString();
			Debug.Log(string.Concat(obj));
			if (_zone_camera_tf == null)
			{
				_zone_camera_tf = new GameObject("~build zone camera target").transform;
				_zone_camera_tf.SetParent(MainGame.me.world_root);
				_zone_camera_tf.localScale = Vector3.one;
				_zone_camera_tf.gameObject.SetActive(value: false);
			}
			_zone_camera_tf.position = GetFitCameraPos(_cur_build_zone.center_tf.position);
			CameraTools.AddToCameraTargets(_zone_camera_tf);
			BuildGrid.me.MoveBuildGridTo(_cur_build_zone.center_tf.position);
		}
	}

	public bool IsBuilding()
	{
		return !string.IsNullOrEmpty(_cur_build_zone_id);
	}

	public void CraftBuilding(CraftDefinition craft)
	{
		ObjectCraftDefinition objectCraftDefinition = craft as ObjectCraftDefinition;
		Debug.Log("CraftBuilding craft_id = " + craft.id + ", type = " + objectCraftDefinition.build_type);
		if (objectCraftDefinition.build_type == ObjectCraftDefinition.BuildType.Put)
		{
			OnBuildCraftSelected(objectCraftDefinition);
			GUIElements.me.game_gui.OnClosePressed();
			GUIElements.me.hud.Hide();
			GUIElements.me.build_mode_gui.Open();
			MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
		}
		else if (objectCraftDefinition.wait_script_callback)
		{
			Debug.Log("OnBuildCraftSelected " + objectCraftDefinition.id + ", obj = " + objectCraftDefinition.out_obj);
			MainGame.me.save.quests.CheckKeyQuests("build_" + objectCraftDefinition.out_obj);
			_mode = Mode.ScriptBuilding;
			_cd = objectCraftDefinition;
			MainGame.me.EnterScriptBuilding();
			MainGame.me.gui_elements.craft.Hide();
			GUIElements.me.game_gui.OnClosePressed();
			GUIElements.me.hud.Hide();
			GUIElements.me.build_mode_gui.Open();
			MainGame.me.player.components.interaction.RedrawCurrentInteractiveHint();
			last_build_desk.components.craft.crafts.Clear();
			last_build_desk.components.craft.crafts.Add(objectCraftDefinition);
			last_build_desk.components.craft.CraftAsPlayer(objectCraftDefinition);
		}
		else
		{
			GUIElements.me.build_mode_gui.Hide();
			GUIElements.me.craft.Hide();
			MainGame.me.ExitBuildMode();
			last_build_desk.components.craft.crafts.Clear();
			last_build_desk.components.craft.crafts.Add(objectCraftDefinition);
			last_build_desk.components.craft.CraftAsPlayer(objectCraftDefinition);
			MainGame.me.build_mode_logics.SetCurrentBuildZone(string.Empty);
		}
	}
}
