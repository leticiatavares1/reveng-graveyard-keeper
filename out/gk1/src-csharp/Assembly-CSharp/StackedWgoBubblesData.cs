using UnityEngine;

public class StackedWgoBubblesData
{
	public long id;

	public WorldGameObject wgo;

	public GameRes res;

	public int frames_delay;

	public float period_delay;

	public Vector3? custom_pos;

	public StackedWgoBubblesData(long id, WorldGameObject wgo, GameRes res, Vector3? custom_pos = null)
	{
		this.id = id;
		this.wgo = wgo;
		this.res = res;
		if (custom_pos.HasValue)
		{
			this.custom_pos = custom_pos;
		}
	}

	public void TryToShowBubble()
	{
		if (!res.IsEmpty())
		{
			EffectBubblesManager.ShowImmediately(wgo, res);
			res.Clear();
			res.durability = 0f;
			period_delay = 0.5f * Time.timeScale;
		}
	}

	public void AddRes(GameRes res)
	{
		if (!res.IsEmpty())
		{
			this.res += res;
			if (frames_delay == 0 && period_delay < 0f)
			{
				frames_delay = 3;
				period_delay = 0f;
			}
		}
	}
}
