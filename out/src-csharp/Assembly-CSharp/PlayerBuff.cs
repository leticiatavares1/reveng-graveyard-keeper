using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerBuff
{
	public string buff_id;

	public float end_time;

	[SerializeField]
	private float _tick_time;

	public BuffDefinition definition => GameBalance.me.GetData<BuffDefinition>(buff_id);

	public string GetTimerText()
	{
		float num = end_time - MainGame.game_time;
		if (num < 0f)
		{
			return "0:00";
		}
		num *= 450f;
		int num2 = Mathf.FloorToInt(num / 60f);
		num -= (float)(num2 * 60);
		int num3 = Mathf.FloorToInt(num);
		int num4 = Mathf.FloorToInt((float)num2 / 60f);
		num2 -= num4 * 60;
		return string.Format((num4 == 0) ? "{0:0}:{1:00}" : "{2:0}:{0:00}:{1:00}", num2, num3, num4);
	}

	public void CustomUpdate(float delta_time)
	{
		if (EnvironmentEngine.me.IsTimeStopped())
		{
			return;
		}
		_tick_time += delta_time;
		BuffDefinition buffDefinition = definition;
		if (!buffDefinition.tick_period.EqualsTo(0f) && _tick_time > buffDefinition.tick_period)
		{
			_tick_time -= buffDefinition.tick_period;
			GameRes gameRes = MainGame.me.player.data.GetParams().Clone();
			buffDefinition.se_tick.Evaluate();
			GameRes gameRes2 = MainGame.me.player.data.GetParams() - gameRes;
			gameRes2.RemoveAllBut(new List<string> { "hp", "energy", "money" });
			if (!gameRes2.IsEmpty() && !MainGame.me.player.is_dead)
			{
				EffectBubblesManager.ShowImmediately(MainGame.me.player.bubble_pos, gameRes2);
			}
		}
	}
}
