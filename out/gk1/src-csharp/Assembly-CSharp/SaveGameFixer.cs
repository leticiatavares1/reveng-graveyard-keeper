using UnityEngine;

public static class SaveGameFixer
{
	private static bool _need_reset_donkey_flag;

	public static void UnstuckWGO(WorldGameObject wgo)
	{
		if (wgo.obj_def.IsNPC() && wgo.components.character.movement_state == MovementComponent.MovementState.None && wgo.GetParamInt("on_the_way_now") == 1)
		{
			wgo.SetParam("on_the_way_now", 0f);
			if (wgo.obj_id.Contains("donkey"))
			{
				_need_reset_donkey_flag = true;
				GDPoint gDPointByGDTag = WorldMap.GetGDPointByGDTag("default_destroy_point");
				wgo.transform.position = gDPointByGDTag.transform.position;
				wgo.RefreshPositionCache();
				wgo.OnCameToGDPoint(gDPointByGDTag);
			}
			Debug.Log("Fixing stuck " + wgo.name, wgo);
		}
	}

	public static void OnAfterAllInits()
	{
		if (_need_reset_donkey_flag)
		{
			_need_reset_donkey_flag = false;
			MainGame.me.player.SetParam("donkey_on_scene", 0f);
		}
	}
}
