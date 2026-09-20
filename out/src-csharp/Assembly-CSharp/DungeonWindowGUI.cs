public class DungeonWindowGUI : BaseGUI
{
	public const int MAX_DUNGEON_LEVELS = 15;

	public const string DUNGEON_UNLOCKED = "dungeon_unlocked_";

	public static readonly int[] NEED_TO_BE_UNLOCKED = new int[1] { 11 };

	public DungeonLevelGUIItem prefab;

	public UIScrollView scroll;

	public override void Open()
	{
		base.Open();
		RemoveGUIItems();
		for (int i = 1; i <= 15; i++)
		{
			prefab.Copy(prefab.transform.parent).SetDungeonLevel(i);
		}
		scroll.GetComponentInChildren<UIGrid>().Reposition();
		scroll.GetComponentInChildren<UIGrid>().repositionNow = true;
		scroll.RestrictWithinBounds(instant: true);
		scroll.ResetPosition();
		if (BaseGUI.for_gamepad)
		{
			base.gamepad_controller.ReinitItems(focus_on_first_active: true);
		}
	}

	public override void OnClosePressed()
	{
		base.OnClosePressed();
		RemoveGUIItems();
	}

	private void RemoveGUIItems()
	{
		prefab.gameObject.SetActive(value: false);
		DungeonLevelGUIItem[] componentsInChildren = scroll.GetComponentsInChildren<DungeonLevelGUIItem>(includeInactive: false);
		if (componentsInChildren.Length != 0)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				NGUITools.Destroy(componentsInChildren[i].gameObject);
			}
		}
		scroll.ResetPosition();
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}
}
