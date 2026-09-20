using UnityEngine;

public class HUDRelationBubble : AnimatedGUIPanel
{
	public int hidden_x = 30;

	public int shown_x = -1;

	public UILabel label;

	public void Init()
	{
		Init(hidden_x, shown_x, AnimationType.AnimateX);
	}

	public static string GetRelationChangeString(int delta)
	{
		if (delta > 0)
		{
			return "+(positive)" + Mathf.Abs(delta);
		}
		return "-(negative)" + Mathf.Abs(delta);
	}

	public void OnChangedRelation(int delta)
	{
		label.text = GJL.L("relation") + GJL.L(":") + " " + GetRelationChangeString(delta);
		Show();
	}
}
