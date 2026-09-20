using UnityEngine;

public class GameDebugConfiguration : MonoBehaviour
{
	public string scene_to_load = "";

	public bool multiplayer;

	private string[] _scenes_values = new string[2] { "scene_graveyard", "scene_prefabs_list" };
}
