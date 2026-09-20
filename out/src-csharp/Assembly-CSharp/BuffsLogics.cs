using UnityEngine;

public static class BuffsLogics
{
	private static float _time;

	public static void UpdateEveryFrame()
	{
		_time += Time.deltaTime;
		if (_time > 1f)
		{
			_time = 0f;
			CheckBuffsGiveConditions();
		}
	}

	public static void CheckBuffsGiveConditions()
	{
		if (!MainGame.game_started)
		{
			return;
		}
		foreach (BuffDefinition buffs_datum in GameBalance.me.buffs_data)
		{
			if (buffs_datum.condition_player_res.IsEmpty())
			{
				continue;
			}
			bool flag = true;
			foreach (GameResAtom item in buffs_datum.condition_player_res.ToAtomList())
			{
				flag = MainGame.me.player.GetParam(item.type) >= item.value;
				if (!flag)
				{
					break;
				}
			}
			if (flag)
			{
				GiveBuffIfNotExists(buffs_datum);
			}
		}
	}

	private static void GiveBuffIfNotExists(BuffDefinition buff)
	{
		if (FindBuffByID(buff.id) == null)
		{
			AddBuff(buff.id);
		}
	}

	public static void AddBuff(string buff_id, float? length = null)
	{
		Debug.Log("AddBuff: " + buff_id);
		BuffDefinition data = GameBalance.me.GetData<BuffDefinition>(buff_id);
		if (data == null)
		{
			Debug.LogError("BuffDefinition not found, id = " + buff_id);
			return;
		}
		PlayerBuff playerBuff = FindBuffByID(buff_id);
		float num = length ?? data.length.EvaluateFloat();
		num = num / 450f * 60f;
		if (playerBuff == null)
		{
			MainGame.me.save.buffs.Add(new PlayerBuff
			{
				buff_id = buff_id,
				end_time = MainGame.game_time + num
			});
			MainGame.me.player.data.AddToParams(data.res);
			data.se_start.Evaluate();
		}
		else
		{
			switch (data.overlay_type)
			{
			case BuffDefinition.BuffOverlayType.Set:
				playerBuff.end_time = MainGame.game_time + num;
				break;
			case BuffDefinition.BuffOverlayType.Add:
				playerBuff.end_time += num;
				break;
			}
		}
		GUIElements.me.buffs.Redraw();
	}

	public static void RemoveBuff(string buff_id)
	{
		Debug.Log("RemoveBuff: " + buff_id);
		PlayerBuff playerBuff = FindBuffByID(buff_id);
		if (playerBuff != null)
		{
			MainGame.me.save.buffs.Remove(playerBuff);
			BuffDefinition data = GameBalance.me.GetData<BuffDefinition>(buff_id);
			MainGame.me.player.data.SubFromParams(data.res);
			data.se_finish.Evaluate();
			GUIElements.me.buffs.Redraw();
		}
	}

	public static void RecalculateBuffs()
	{
		float deltaTime = Time.deltaTime;
		bool flag = false;
		for (int i = 0; i < MainGame.me.save.buffs.Count; i++)
		{
			PlayerBuff playerBuff = MainGame.me.save.buffs[i];
			playerBuff.CustomUpdate(deltaTime);
			if (!(playerBuff.end_time > MainGame.game_time))
			{
				flag = true;
				RemoveBuff(playerBuff.buff_id);
				i--;
			}
		}
		if (flag)
		{
			GUIElements.me.buffs.Redraw();
		}
	}

	public static PlayerBuff FindBuffByID(string buff_id)
	{
		foreach (PlayerBuff buff in MainGame.me.save.buffs)
		{
			if (buff.buff_id == buff_id)
			{
				return buff;
			}
		}
		return null;
	}
}
