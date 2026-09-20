using LazyBearTechnology;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class UIMapMilestoneVirtualCursorEventHandler : BaseVirtualCursorEventHandler
{
	public UIMapMilestone mapMilestone;

	private void Awake()
	{
		if (mapMilestone == null)
		{
			mapMilestone = base.transform.parent.GetComponent<UIMapMilestone>();
		}
	}

	protected override void OnSelect(BaseVirtualCursor cursor)
	{
		if (mapMilestone.IsInteractable)
		{
			if (cursor != null && cursor is MapVirtualCursor mapVirtualCursor)
			{
				mapVirtualCursor.DoAnimationTo(mapMilestone.NavigationRect);
			}
			LazyUI.GetWindow<UIMapWindow>().OnEnterMapMilestone(mapMilestone);
		}
	}

	protected override void OnDeselect(BaseVirtualCursor cursor)
	{
		if (mapMilestone.IsInteractable)
		{
			if (cursor != null && cursor is MapVirtualCursor mapVirtualCursor)
			{
				mapVirtualCursor.DoAnimationTo(null);
			}
			LazyUI.GetWindow<UIMapWindow>().OnExitMapMilestone(mapMilestone);
		}
	}
}
