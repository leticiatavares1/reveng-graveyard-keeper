using System.Collections.Generic;
using UnityEngine;

public class DropsList : MonoBehaviour
{
	private static DropsList _instance;

	public List<DropResGameObject> drops = new List<DropResGameObject>();

	private bool _initialized;

	public static DropsList me
	{
		get
		{
			object obj = _instance;
			if (obj == null)
			{
				obj = Object.FindObjectOfType<DropsList>() ?? new GameObject("~DropsList").AddComponent<DropsList>();
				_instance = (DropsList)obj;
			}
			return (DropsList)obj;
		}
	}

	public bool Add(DropResGameObject drop)
	{
		if (drops.Contains(drop))
		{
			return false;
		}
		drops.Add(drop);
		return true;
	}

	private void Update()
	{
		for (int i = 0; i < drops.Count; i++)
		{
			drops[i].UpdateMe();
			if (drops[i].is_collected)
			{
				Object.Destroy(drops[i].gameObject);
				drops.RemoveAt(i);
				i--;
			}
		}
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		foreach (DropResGameObject drop in drops)
		{
			drop.FixedUpdateMe(fixedDeltaTime);
		}
	}

	public void CheckDrops(WorldGameObject target_obj)
	{
		Vector3 position = target_obj.transform.position;
		foreach (DropResGameObject drop in drops)
		{
			if (!drop.has_target)
			{
				drop.ProcessDropCollectorRangeCheck(target_obj, position);
			}
		}
	}

	public void SetHighlighted(DropResGameObject drop)
	{
		if (drop == null)
		{
			foreach (DropResGameObject drop2 in drops)
			{
				drop2.SetInteractionHilight(interaction: false);
			}
			return;
		}
		foreach (DropResGameObject drop3 in drops)
		{
			drop3.SetInteractionHilight(drop3 == drop);
		}
	}

	public void RemoveAllDropsFromTheScene()
	{
		for (int i = 0; i < drops.Count; i++)
		{
			Object.Destroy(drops[i].gameObject);
		}
		drops.Clear();
	}

	public void ToGameSave(GameSave save)
	{
		save.drops = new List<GameSave.SavedDropItem>();
		foreach (DropResGameObject drop in drops)
		{
			save.drops.Add(new GameSave.SavedDropItem
			{
				pos = drop.transform.position,
				res = drop.res,
				zone_id = drop.zone_id
			});
		}
	}

	public void FromGameSave(GameSave save)
	{
		if (save?.drops == null)
		{
			return;
		}
		foreach (GameSave.SavedDropItem drop in save.drops)
		{
			DropResGameObject dropResGameObject = DropResGameObject.Drop(drop.pos, drop.res, MainGame.me.world_root, Direction.IgnoreDirection, 1f, -1, check_walls: true, force_stacked_drop: true);
			if (dropResGameObject == null)
			{
				Debug.LogError("Couldn't drop: " + drop.res);
			}
			else
			{
				dropResGameObject.zone_id = drop.zone_id;
			}
		}
	}
}
