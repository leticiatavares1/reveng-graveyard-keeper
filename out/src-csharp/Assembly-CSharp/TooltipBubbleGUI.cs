using System.Collections.Generic;
using UnityEngine;

public class TooltipBubbleGUI : WidgetsBubbleGUI
{
	private static List<TooltipBubbleGUI> _all = new List<TooltipBubbleGUI>();

	private static bool _available = true;

	public override void DestroyBubble()
	{
		_all.Remove(this);
		widget.alpha = 0f;
		base.DestroyBubble();
	}

	public static TooltipBubbleGUI Show(BubbleWidgetDataContainer data, bool for_gamepad, Collider2D tooltip_collider)
	{
		if (data == null || !data.has_data)
		{
			Debug.LogError("no tooltip data");
			return null;
		}
		TooltipBubbleGUI tooltipBubbleGUI = GUIElements.me.tooltip_bubble.Copy();
		if (tooltipBubbleGUI.simple_table != null)
		{
			tooltipBubbleGUI.simple_table.use_hash = false;
			tooltipBubbleGUI.simple_table.ClearHashes();
		}
		tooltipBubbleGUI.LinkColliderForGamepad(for_gamepad, tooltip_collider);
		tooltipBubbleGUI.Show(data, force_redraw: true);
		_all.Add(tooltipBubbleGUI);
		GJL.EnsureChildLabelsHasCorrectFont(tooltipBubbleGUI.gameObject, do_cache: false);
		GJL.ApplyCustomFontSettings(tooltipBubbleGUI.gameObject);
		return tooltipBubbleGUI;
	}

	public static void ChangeAvaibility(bool available)
	{
		if (!available)
		{
			while (_all.Count > 0)
			{
				_all[0].DestroyBubble();
			}
		}
		_available = available;
	}
}
