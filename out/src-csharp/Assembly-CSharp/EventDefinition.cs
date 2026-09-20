using System;
using UnityEngine;

[Serializable]
public class EventDefinition
{
	public string spawn_wgo;

	public string drop_item;

	public string anim_trigger;

	public GameRes set_game_res = new GameRes();

	public string event_to_fire;

	public string sound;

	public bool anim_driven;

	public Action Invoke(Vector2 pos, Animator animator, Action dlg, WorldGameObject target_wgo, out bool need_to_wait_animation_finish)
	{
		need_to_wait_animation_finish = false;
		if (animator != null && animator.isInitialized)
		{
			animator.SetTrigger(anim_trigger);
			need_to_wait_animation_finish = true;
		}
		Action action = delegate
		{
			if (dlg != null)
			{
				dlg();
			}
			if (!string.IsNullOrEmpty(spawn_wgo))
			{
				Transform parent = (MainGame.me.dungeon_root.dungeon_is_loaded_now ? MainGame.me.dungeon_root.transform : MainGame.me.world_root);
				if (spawn_wgo.StartsWith("*"))
				{
					WorldMap.SpawnWSO(parent, spawn_wgo.Substring(1), pos);
					string text = spawn_wgo;
					Vector2 vector = pos;
					Debug.Log("Spawned WSO \"" + text + "\" at pos " + vector.ToString());
				}
				else
				{
					WorldMap.SpawnWGO(parent, spawn_wgo, pos);
					string text2 = spawn_wgo;
					Vector2 vector = pos;
					Debug.Log("Spawned WGO \"" + text2 + "\" at pos " + vector.ToString());
				}
			}
			if (!string.IsNullOrEmpty(drop_item))
			{
				DropResGameObject.Drop(pos, new Item(drop_item, 1), MainGame.me.world_root);
			}
			if (target_wgo != null)
			{
				target_wgo.SetParam(set_game_res);
				if (!string.IsNullOrEmpty(event_to_fire))
				{
					target_wgo.FireEvent(event_to_fire);
				}
			}
		};
		if (!anim_driven)
		{
			action();
			return null;
		}
		return action;
	}
}
