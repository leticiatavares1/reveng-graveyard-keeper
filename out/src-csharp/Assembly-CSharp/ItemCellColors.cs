using System;
using UnityEngine;

[Serializable]
public class ItemCellColors
{
	private const float BROKEN_ALPHA = 0.75f;

	public Color normal;

	public Color mouse_overed;

	public Color gamepad_overed;

	public Color wrong;

	public Color inactive;

	public Color enough_res;

	public Color not_enough_res;

	public Color money;

	public Color broken => new Color(wrong.r, wrong.g, wrong.b, 0.75f);
}
