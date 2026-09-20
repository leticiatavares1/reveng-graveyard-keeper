using UnityEngine;

public class DropCollectorComponent : WorldGameObjectComponent
{
	public override void StartComponent()
	{
		_update_every_frame = 4;
		base.StartComponent();
	}

	public override bool HasUpdate()
	{
		return true;
	}

	public override void UpdateComponent(float delta_time)
	{
		if (!(DropsList.me == null) && !DelayedUpdate(delta_time))
		{
			DropsList.me.CheckDrops(base.wgo);
		}
	}

	public void OnTriggerStay2D(Collider2D col)
	{
		if (Application.isPlaying)
		{
			DropResGameObject component = col.GetComponent<DropResGameObject>();
			if ((bool)component)
			{
				CheckDrop(component);
			}
		}
	}

	public void OnTriggerEnter2D(Collider2D col)
	{
		if (Application.isPlaying)
		{
			DropResGameObject component = col.GetComponent<DropResGameObject>();
			if ((bool)component)
			{
				CheckDrop(component);
			}
		}
	}

	public void CheckDrop(DropResGameObject drop)
	{
		if (!drop.is_collected)
		{
			int num = base.wgo.CanCollectDrop(drop);
			if (num == 0)
			{
				drop.UnsuccessfullPickup(base.wgo);
				return;
			}
			if (drop.res.value == num)
			{
				drop.CollectDrop(base.wgo);
				return;
			}
			drop.res.value -= num;
			drop.RedrawStackCounter();
			base.wgo.data.AddItem(drop.res.id, num);
		}
	}
}
