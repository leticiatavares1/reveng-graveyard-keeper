using SmartPools;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
	private static Prefabs _instance;

	public DropResGameObject drop_res_game_object;

	public WorldGameObject test_place_obj_prefab;

	public GameObject movement_and_seeker_setup;

	public GameObject dock_point;

	public GameObject debug_teleport;

	public GameObject player_prefab;

	private static WorldGameObject _wgo_prefab;

	private static ProjectileObject _projectile_prefab;

	private static WGOMark _mark_prefab;

	private static BaseItemCellGUI _item_cell;

	private TechPointsDrop _tech_points_drop;

	public static Prefabs me => _instance ?? (_instance = Object.FindObjectOfType<Prefabs>());

	public static WorldGameObject wgo_prefab
	{
		get
		{
			if (_wgo_prefab == null)
			{
				GameObject gameObject = Resources.Load<GameObject>("wgo prefab");
				if (gameObject == null)
				{
					Debug.LogError("cannot load wgo prefab");
					return null;
				}
				gameObject.SetActive(value: false);
				_wgo_prefab = gameObject.GetComponent<WorldGameObject>();
			}
			return _wgo_prefab;
		}
	}

	public static ProjectileObject projectile_prefab
	{
		get
		{
			if (_projectile_prefab == null)
			{
				GameObject gameObject = Resources.Load<GameObject>("projectile prefab");
				if (gameObject == null)
				{
					Debug.LogError("cannot load wgo prefab");
					return null;
				}
				gameObject.SetActive(value: false);
				_projectile_prefab = gameObject.GetComponent<ProjectileObject>();
			}
			return _projectile_prefab;
		}
	}

	public static WGOMark mark_prefab
	{
		get
		{
			if (_mark_prefab == null)
			{
				_mark_prefab = Resources.Load<WGOMark>("wgo mark");
				if (_mark_prefab == null)
				{
					Debug.LogError("cannot load wgo mark prefab");
					return null;
				}
			}
			return _mark_prefab;
		}
	}

	public static BaseItemCellGUI item_cell
	{
		get
		{
			if (_item_cell == null)
			{
				GameObject gameObject = Resources.Load<GameObject>("Base Item Cell");
				if (gameObject == null)
				{
					Debug.LogError("cannot load item cell prefab");
					return null;
				}
				gameObject.SetActive(value: false);
				_item_cell = gameObject.GetComponent<BaseItemCellGUI>();
			}
			return _item_cell;
		}
	}

	public TechPointsDrop tech_points_drop
	{
		get
		{
			if (_tech_points_drop == null)
			{
				_tech_points_drop = Resources.Load<TechPointsDrop>("tech points drop");
				if (_tech_points_drop == null)
				{
					Debug.LogError("cannot load tech_points_drop prefab");
					return null;
				}
			}
			return _tech_points_drop;
		}
	}

	private void Start()
	{
		_instance = this;
		if (drop_res_game_object == null)
		{
			drop_res_game_object = GetComponentInChildren<DropResGameObject>(includeInactive: true);
		}
		drop_res_game_object.Deactivate();
		test_place_obj_prefab.Deactivate();
		movement_and_seeker_setup.SetActive(value: false);
		if ((bool)dock_point)
		{
			dock_point.Deactivate();
		}
		if ((bool)debug_teleport)
		{
			debug_teleport.Deactivate();
		}
		SmartPooler.CreatePool(wgo_prefab, 10000).ConfigurePool(40, activate_on_creation: false);
	}
}
