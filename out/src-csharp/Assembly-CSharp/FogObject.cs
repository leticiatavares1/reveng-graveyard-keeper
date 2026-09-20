using UnityEngine;

public class FogObject : MonoBehaviour
{
	public Vector2 scr_pos;

	public Vector2 tile_pos;

	private const int SIZE_X = 300;

	private const int SIZE_Y = 300;

	private const float OBJECT_WIDTH_IN_TILES = 6f;

	private const float OBJECT_HEIGHT_IN_TILES = 0.3f;

	private const int FOGFIELD_WIDTH_IN_OBJECTS = 6;

	private const int FOGFIELD_HEIGHT_IN_OBJECTS = 63;

	private readonly Vector3 TILES_X_VECTOR = new Vector2(36f, 0f);

	private readonly Vector3 TILES_Y_VECTOR = new Vector2(0f, 18.900002f);

	private static Transform _fog_parent = null;

	protected RoundAndSortComponent round_and_sort;

	protected bool round_and_sort_set;

	private Material _mat;

	public static float BORDER_X = 1f;

	public static float BORDER_Y = 6f;

	public void Update()
	{
		scr_pos = MainGame.me.world_cam.WorldToScreenPoint(base.transform.position);
		tile_pos = new Vector2(scr_pos.x / 6f, scr_pos.y / 0.3f) / 96f;
		bool flag = false;
		if (tile_pos.x < 0f - BORDER_X)
		{
			base.transform.localPosition += TILES_X_VECTOR;
			flag = true;
		}
		else if (tile_pos.x > 6f - BORDER_X)
		{
			base.transform.localPosition -= TILES_X_VECTOR;
			flag = true;
		}
		if (tile_pos.y < 0f - BORDER_Y)
		{
			base.transform.localPosition += TILES_Y_VECTOR;
			flag = true;
		}
		else if (tile_pos.y > 63f - BORDER_Y)
		{
			base.transform.localPosition -= TILES_Y_VECTOR;
			flag = true;
		}
		if (flag && round_and_sort_set)
		{
			round_and_sort.DoUpdateStuff();
		}
	}

	public static void InitFog(FogObject prefab)
	{
		if (prefab == null)
		{
			Debug.LogError("Fog object not found on a scene");
			return;
		}
		_fog_parent = Fog.SpawnNewFog();
		ChunkedGameObject chunkedGameObject = prefab.GetComponent<ChunkedGameObject>();
		if (chunkedGameObject == null)
		{
			chunkedGameObject = prefab.gameObject.AddComponent<ChunkedGameObject>();
		}
		chunkedGameObject.always_active = true;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 63; j++)
			{
				FogObject fogObject = Object.Instantiate(prefab);
				fogObject.transform.SetParent(_fog_parent, worldPositionStays: false);
				fogObject.round_and_sort = fogObject.GetComponent<RoundAndSortComponent>();
				fogObject.round_and_sort_set = fogObject.round_and_sort != null;
				Fog.me.OnNewFogObjectCreated(fogObject);
				fogObject.transform.localPosition = new Vector3(6f * (float)i, 0.3f * (float)j);
			}
		}
	}
}
