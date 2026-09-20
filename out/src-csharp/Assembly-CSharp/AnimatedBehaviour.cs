public class AnimatedBehaviour : WorldGameObjectComponent
{
	public delegate void OnItemAnimationEvent(ItemDefinition.ItemType item_type, bool flag);

	public delegate void OnAnimUpdate(float normalized_time);

	public event GJCommons.VoidDelegate on_loop;

	public event GJCommons.VoidDelegate on_first_loop;

	public event GJCommons.VoidDelegate on_enter;

	public event GJCommons.VoidDelegate on_exit;

	public event OnItemAnimationEvent on_item_loop;

	public event OnItemAnimationEvent on_item_first_loop;

	public event OnAnimUpdate on_update;

	public void SetCallbacks(OnAnimUpdate update, GJCommons.VoidDelegate loop)
	{
		on_update += update;
		on_loop += loop;
	}

	public void RemoveCallbacks(OnAnimUpdate update, GJCommons.VoidDelegate loop)
	{
		on_update -= update;
		on_loop -= loop;
	}

	public void OnItemStart(ItemDefinition.ItemType item_type)
	{
	}

	public void OnItemStop(ItemDefinition.ItemType item_type)
	{
	}

	public void OnItemLoop(ItemDefinition.ItemType item_type, bool flag)
	{
		if (this.on_item_loop != null)
		{
			this.on_item_loop(item_type, flag);
		}
	}

	public void OnItemFirstLoop(ItemDefinition.ItemType item_type, bool flag)
	{
		if (this.on_item_first_loop != null)
		{
			this.on_item_first_loop(item_type, flag);
		}
	}

	public void OnLoop()
	{
		this.on_loop.TryInvoke();
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
		this.on_exit.TryInvoke();
	}

	public void OnFirstLoop()
	{
		this.on_first_loop.TryInvoke();
	}

	public void OnUpdate(float normalized_time)
	{
		if (this.on_update != null)
		{
			this.on_update(normalized_time);
		}
	}

	public override void UpdateEnableState(ObjectDefinition.ObjType obj_type)
	{
		base.enabled = obj_type == ObjectDefinition.ObjType.Mob || obj_type == ObjectDefinition.ObjType.NPC || base.wgo.is_player;
	}
}
