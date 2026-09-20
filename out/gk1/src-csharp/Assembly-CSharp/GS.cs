using NodeCanvas.Framework;
using UnityEngine;

public class GS
{
	private static GameObject _globals_scripts;

	protected static GameObject global_scripts
	{
		get
		{
			if (_globals_scripts != null)
			{
				return _globals_scripts;
			}
			_globals_scripts = GameObject.Find("* Global Scripts");
			if (_globals_scripts != null)
			{
				return _globals_scripts;
			}
			_globals_scripts = new GameObject("* Global Scripts");
			return _globals_scripts;
		}
	}

	public static WorldGameObject Spawn(string obj_id, Transform pos = null, string custom_tag = "")
	{
		WorldGameObject worldGameObject = WorldMap.SpawnWGO(MainGame.me.world_root, obj_id, (pos == null) ? Vector3.zero : pos.position);
		worldGameObject.custom_tag = custom_tag;
		worldGameObject.OnJustSpawned();
		GraphOwner[] componentsInChildren = worldGameObject.GetComponentsInChildren<GraphOwner>();
		foreach (GraphOwner graphOwner in componentsInChildren)
		{
			graphOwner.StartBehaviour();
			Debug.Log(graphOwner.name, graphOwner);
		}
		Debug.Log("Spawned " + obj_id + " (" + worldGameObject.obj_id + ") at: " + (worldGameObject.transform.position / 96f).ToString(), worldGameObject);
		return worldGameObject;
	}

	public void GoTo(Vector2 dest)
	{
	}

	public static CustomFlowScript RunFlowScript(string uscript_name, CustomFlowScript.OnFinishedDelegate on_finished = null)
	{
		return CustomFlowScript.Create(global_scripts, uscript_name, is_global: true, on_finished);
	}

	public static void SetPlayerEnable(bool player_enabled, bool affect_cinematic)
	{
		Debug.Log("Set player " + (player_enabled ? "enabled" : "disabled") + ", affect_cinematic = " + affect_cinematic);
		MainGame.me.player_char.control_enabled = player_enabled;
		GUIElements.ChangeHUDAlpha(player_enabled, affect_cinematic);
		GUIElements.ChangeBubblesVisibility(player_enabled);
		GUIElements.me.overhead_panel.gameObject.SetActive(player_enabled);
		MainGame.me.player.components.character.body.bodyType = ((!player_enabled) ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic);
		if (affect_cinematic)
		{
			CameraTools.TweenLetterbox(!player_enabled);
		}
		if (affect_cinematic || player_enabled)
		{
			GUIElements.me.relation.ChangeHUDAlpha(player_enabled, animated: false);
		}
		GUIElements.me.relation.Update();
	}

	public static void AffectCinematic(bool show)
	{
		bool control_enabled = MainGame.me.player_char.control_enabled;
		GUIElements.ChangeHUDAlpha(control_enabled, show);
		GUIElements.ChangeBubblesVisibility(control_enabled);
		GUIElements.me.overhead_panel.gameObject.SetActive(control_enabled);
		CameraTools.TweenLetterbox(show);
		if (show || control_enabled)
		{
			GUIElements.me.relation.ChangeHUDAlpha(control_enabled, animated: false);
		}
		GUIElements.me.relation.Update();
	}

	public static bool IsPlayerEnable()
	{
		return MainGame.me.player_char.control_enabled;
	}

	public static void AddCameraTarget(Transform transform)
	{
		CameraTools.AddToCameraTargets(transform);
	}

	public static void RemoveCameraTarget(Transform transform)
	{
		CameraTools.RemoveFromCameraTargets(transform);
	}
}
