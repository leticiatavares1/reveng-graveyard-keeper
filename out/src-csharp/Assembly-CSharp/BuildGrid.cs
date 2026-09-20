using System.Collections.Generic;
using UnityEngine;

public class BuildGrid : MonoBehaviour
{
	private static Dictionary<long, bool> _has_build_colliders = new Dictionary<long, bool>();

	private Texture2D _tx;

	private Material[] _mats;

	private const int TEXTURE_SIZE = 1024;

	public const int BUILD_GRID_DIVIDER = 3;

	private const int SCAN_AREA_SIZE = 100;

	private const int HALF_TEXTURE_SIZE = 512;

	public const int BUILD_GRID_SIZE = 32;

	public const int HALF_BUILD_GRID_SIZE = 16;

	public static readonly Vector2 BUILD_GRID_VECTOR2 = new Vector2(32f, 32f);

	public static readonly Vector2 GRID_CHECK_BOX_SIZE = new Vector2(32f, 32f) * 0.9f;

	private static readonly Color[] _colors = new Color[1048576];

	private static Collider2D[] _colliders = new Collider2D[100];

	private static BuildGrid _me = null;

	private static bool _build_grid_shown = false;

	private const float C_TOTEM_MOD = -0.01f;

	private string _custom_sub_zone = "";

	private List<int> _drawn_totem_radius_cells = new List<int>();

	private BuildingSubZoneConfiguration _current_sub_zone_configuration;

	public static BuildingSubZoneConfiguration current_sub_zone_configuration => me._current_sub_zone_configuration;

	public static BuildGrid me => MainGame.me.build_grid;

	[ContextMenu("Refresh grid")]
	private void RefreshGrid()
	{
		Debug.Log("RefreshGrid");
		_current_sub_zone_configuration = null;
		_drawn_totem_radius_cells.Clear();
		CacheMaterials();
		if (_tx == null)
		{
			_tx = new Texture2D(1024, 1024, TextureFormat.Alpha8, mipChain: false)
			{
				filterMode = FilterMode.Point
			};
		}
		Color color = new Color(0f, 0f, 0f, 0f);
		string cur_build_zone_id = MainGame.me.build_mode_logics.cur_build_zone_id;
		Vector2 vector = base.transform.position;
		bool flag = !string.IsNullOrEmpty(_custom_sub_zone);
		for (int i = -100; i < 100; i++)
		{
			int num = i * 32;
			for (int j = -100; j < 100; j++)
			{
				int num2 = j * 32;
				int num3 = 1024 - (i + 512);
				int num4 = 1024 - (j + 512);
				Vector2 vector2 = new Vector2(num - 16, num2 - 16) + vector;
				Collider2D[] array = Physics2D.OverlapBoxAll(vector2, GRID_CHECK_BOX_SIZE, 0f, 524288);
				bool flag2 = false;
				Collider2D[] array2 = array;
				foreach (Collider2D collider2D in array2)
				{
					if (!(collider2D != null))
					{
						continue;
					}
					WorldZone component = collider2D.GetComponent<WorldZone>();
					if (flag)
					{
						if (!(component == null))
						{
							continue;
						}
						WorldSubZone component2 = collider2D.GetComponent<WorldSubZone>();
						if (component2 == null || component2.sub_zone_id != _custom_sub_zone)
						{
							continue;
						}
						flag2 = true;
						_current_sub_zone_configuration = component2.buildingSubZoneConfiguration;
					}
					else
					{
						if (component == null)
						{
							continue;
						}
						flag2 = component.id == cur_build_zone_id && component.IsAvailableForBuild();
					}
					if (flag2)
					{
						break;
					}
				}
				Vector3 position = me.transform.position;
				position.z = ((_current_sub_zone_configuration == null) ? 1900f : _current_sub_zone_configuration.z_build_grid_sorting);
				me.transform.position = position;
				bool flag3 = IsCellBusy(vector2);
				color.a = (flag2 ? (flag3 ? 1f : 0.5f) : 0f);
				_colors[num3 + num4 * 1024] = color;
			}
		}
		DrawExistingTotemRadiuses();
		ApplyColors();
		Update();
	}

	private void DrawExistingTotemRadiuses()
	{
		WorldZone zoneByID = WorldZone.GetZoneByID(MainGame.me.build_mode_logics.cur_build_zone_id);
		if (zoneByID == null)
		{
			return;
		}
		foreach (WorldGameObject zoneWGO in zoneByID.GetZoneWGOs())
		{
			if (zoneWGO.obj_def.IsTotem())
			{
				RedrawTotemRadius(zoneWGO, default(Vector2), is_floating_object: false);
			}
		}
	}

	public void RedrawTotemRadius(WorldGameObject wobj, Vector2 center_offset = default(Vector2), bool is_floating_object = true)
	{
		if (wobj == null || !wobj.obj_def.IsTotem())
		{
			return;
		}
		if (is_floating_object)
		{
			ClearPreviousTotemRadius(apply_colors: false);
		}
		Vector2 vector = (Vector2)wobj.transform.position + center_offset * 96f;
		int num = 1024 - Mathf.RoundToInt((vector.x + 16f - base.transform.position.x) / 32f + 512f);
		int num2 = 1024 - Mathf.RoundToInt((vector.y + 16f - base.transform.position.y) / 32f + 512f);
		float num3 = wobj.obj_def.totem_radius * wobj.obj_def.totem_radius;
		for (int i = -100; i < 100; i++)
		{
			for (int j = -100; j < 100; j++)
			{
				int num4 = 1024 - (i + 512);
				int num5 = 1024 - (j + 512);
				if ((float)((num4 - num) * (num4 - num) + (num5 - num2) * (num5 - num2)) <= num3)
				{
					ColorizeTotemCell(num4 + num5 * 1024, apply_colors: false, is_floating_object);
				}
			}
		}
		if (is_floating_object)
		{
			ApplyColors();
		}
	}

	private void ColorizeTotemCell(int pos, bool apply_colors, bool is_floating_object = true)
	{
		if (is_floating_object && !_drawn_totem_radius_cells.Contains(pos))
		{
			_drawn_totem_radius_cells.Add(pos);
		}
		_colors[pos].a += -0.01f;
		if (is_floating_object && apply_colors)
		{
			ApplyColors();
		}
	}

	public void ClearPreviousTotemRadius(bool apply_colors)
	{
		foreach (int drawn_totem_radius_cell in _drawn_totem_radius_cells)
		{
			_colors[drawn_totem_radius_cell].a -= -0.01f;
		}
		_drawn_totem_radius_cells.Clear();
		if (apply_colors)
		{
			ApplyColors();
		}
	}

	private void ApplyColors()
	{
		_tx.SetPixels(_colors);
		_tx.Apply();
		Material[] mats = _mats;
		foreach (Material obj in mats)
		{
			obj.SetTexture("_MainTex3", _tx);
			obj.SetFloat("_Scale", 300f);
			obj.SetFloat("_Shift", 362f);
		}
	}

	private void CacheMaterials()
	{
		if (_mats == null)
		{
			MeshRenderer[] componentsInChildren = GetComponentsInChildren<MeshRenderer>();
			_mats = new Material[componentsInChildren.Length];
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				_mats[i] = new Material(componentsInChildren[i].sharedMaterial);
				componentsInChildren[i].material = _mats[i];
			}
		}
	}

	private void Show(bool show = true)
	{
		if (show)
		{
			_has_build_colliders.Clear();
			RefreshGrid();
		}
		base.gameObject.SetActive(show);
	}

	public static void ReshowBuildGrid()
	{
		ShowBuildGrid(show: true, ignore_repeating: true, me._custom_sub_zone);
		GJTimer.AddTimer(0.01f, delegate
		{
			ShowBuildGrid(show: true, ignore_repeating: true, me._custom_sub_zone);
		});
	}

	public static void ShowBuildGrid(bool show, bool ignore_repeating = false, string custom_sub_zone = "")
	{
		if (ignore_repeating || _build_grid_shown != show || !(custom_sub_zone == me._custom_sub_zone))
		{
			Debug.Log("ShowBuildGrid " + show + ", current state = " + _build_grid_shown);
			_build_grid_shown = show;
			me._custom_sub_zone = (show ? custom_sub_zone : "");
			me.Show(show);
		}
	}

	public void MoveBuildGridTo(Vector2 pos)
	{
		Vector3 position = me.transform.position;
		position.x = Mathf.Round(pos.x / 96f) * 96f + 48f;
		position.y = Mathf.Round(pos.y / 96f) * 96f + 48f;
		position.z = 1900f;
		me.transform.position = position;
	}

	public static bool IsCellBusy(Vector2 pos, List<Collider2D> target_colliders = null)
	{
		Collider2D[] array = Physics2D.OverlapBoxAll(pos, GRID_CHECK_BOX_SIZE, 0f, 8389121);
		foreach (Collider2D collider2D in array)
		{
			if (SkipCollider(collider2D))
			{
				continue;
			}
			if (target_colliders != null)
			{
				if (target_colliders.Contains(collider2D))
				{
					return true;
				}
			}
			else if (!(collider2D.GetComponentInParent<FloatingWorldGameObject>() != null))
			{
				return true;
			}
		}
		return false;
	}

	public static bool SkipCollider(Collider2D collider, WorldGameObject owner = null)
	{
		if (owner == null)
		{
			owner = collider.GetComponentInParent<WorldGameObject>();
			if (owner == null)
			{
				return false;
			}
		}
		if (WGOHasBuildCollider(owner))
		{
			return collider.gameObject.layer != 23;
		}
		if (collider.gameObject.layer != 0)
		{
			return collider.gameObject.layer != 9;
		}
		return false;
	}

	private static bool WGOHasBuildCollider(WorldGameObject wgo)
	{
		if (wgo == null)
		{
			return false;
		}
		long unique_id = wgo.unique_id;
		if (_has_build_colliders.ContainsKey(unique_id))
		{
			return _has_build_colliders[unique_id];
		}
		_has_build_colliders.Add(unique_id, value: false);
		Collider2D[] componentsInChildren = wgo.GetComponentsInChildren<Collider2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].gameObject.layer == 23)
			{
				_has_build_colliders[unique_id] = true;
				break;
			}
		}
		return _has_build_colliders[unique_id];
	}

	public void Update()
	{
		if (base.gameObject.activeInHierarchy && MainGame.game_started)
		{
			CacheMaterials();
			Color ambientLight = RenderSettings.ambientLight;
			ambientLight.a = RenderSettings.ambientIntensity;
			Material[] mats = _mats;
			for (int i = 0; i < mats.Length; i++)
			{
				mats[i].SetColor("_AmbientColor", ambientLight);
			}
		}
	}

	public static string GetCurrentSubZoneID()
	{
		return me._custom_sub_zone;
	}
}
