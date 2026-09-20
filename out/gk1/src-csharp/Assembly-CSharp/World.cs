using UnityEngine;

[ExecuteInEditMode]
public class World : MonoBehaviour
{
	public GameObject zones;

	private static Vector3 _player_default_pos = Vector3.zero;

	private static bool _player_default_pos_set = false;

	public static Vector3 player_default_pos
	{
		get
		{
			if (!_player_default_pos_set)
			{
				Debug.LogError("Player default pos is not set");
				return Vector3.zero;
			}
			return _player_default_pos;
		}
	}

	public void FindAndRemovePlayerPrefab()
	{
		PlayerComponent[] componentsInChildren = GetComponentsInChildren<PlayerComponent>(includeInactive: true);
		foreach (PlayerComponent playerComponent in componentsInChildren)
		{
			if (!_player_default_pos_set)
			{
				_player_default_pos_set = true;
				_player_default_pos = playerComponent.gameObject.transform.localPosition;
				Vector3 vector = _player_default_pos;
				Debug.Log("Player default pos: " + vector.ToString());
				if (!MainGame.loaded_from_scene_main)
				{
					MainGame.me.save.player_position = _player_default_pos;
				}
			}
			Debug.Log("Removing player prefab");
			Object.Destroy(playerComponent.gameObject);
		}
	}

	public static void InitWorldOnApplicationStart()
	{
		Debug.Log("InitWorldOnApplicationStart");
		MainGame.me.world = Object.FindObjectOfType<World>();
		if (MainGame.me.world == null)
		{
			Debug.LogError("Couldn't find the world");
		}
		else
		{
			MainGame.me.world_root = MainGame.me.world.transform;
		}
	}

	public void LateUpdate()
	{
	}
}
