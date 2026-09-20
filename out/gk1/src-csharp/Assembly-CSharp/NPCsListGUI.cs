public class NPCsListGUI : BaseGameGUI
{
	public NPCItemGUI prefab;

	public UIScrollView scroll;

	public float gamepad_scroll_speed = 5f;

	public override void Init()
	{
		base.Init();
		prefab.gameObject.SetActive(value: false);
	}

	public override void Open()
	{
		base.Open();
		NPCItemGUI[] componentsInChildren = GetComponentsInChildren<NPCItemGUI>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			NGUITools.Destroy(componentsInChildren[i].gameObject);
		}
		scroll.ResetPosition();
		MainGame.me.save.known_npcs.Sort();
		foreach (KnownNPC npc in MainGame.me.save.known_npcs.npcs)
		{
			if (npc.npc_id != "player")
			{
				ObjectDefinition dataOrNull = GameBalance.me.GetDataOrNull<ObjectDefinition>(npc.npc_id);
				if (dataOrNull == null || !dataOrNull.IsRelationVisible() || npc.npc_id.StartsWith("worker_zombie"))
				{
					continue;
				}
			}
			prefab.Copy().Draw(npc);
		}
		GetComponentInChildren<UIGrid>().Reposition();
		GetComponentInChildren<UIGrid>().repositionNow = true;
		scroll.RestrictWithinBounds(instant: true);
		scroll.ResetPosition();
	}

	protected override bool OnPressedBack()
	{
		GUIElements.me.game_gui.Hide();
		return true;
	}

	protected override bool OnPressedDown()
	{
		scroll.Scroll(gamepad_scroll_speed);
		return true;
	}

	protected override bool OnPressedUp()
	{
		scroll.Scroll(0f - gamepad_scroll_speed);
		return true;
	}
}
