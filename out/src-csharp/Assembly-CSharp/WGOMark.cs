using System;
using UnityEngine;

public class WGOMark : MonoBehaviour
{
	public enum MarkType
	{
		None,
		CanBeRemoved,
		IsRemoving
	}

	public SpriteRenderer spr;

	public Sprite s_can_be_removed;

	public Sprite s_is_removing;

	public void Draw(MarkType type)
	{
		switch (type)
		{
		case MarkType.None:
		case MarkType.CanBeRemoved:
			spr.sprite = null;
			break;
		case MarkType.IsRemoving:
			spr.sprite = s_is_removing;
			break;
		default:
			throw new ArgumentOutOfRangeException("type", type, null);
		}
	}
}
