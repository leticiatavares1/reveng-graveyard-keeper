using System;
using UnityEngine;

[Serializable]
public class BaseItemCellElements
{
	public GameObject container;

	public UI2DSprite icon;

	public UI2DSprite back;

	public UI2DSprite selection;

	public UI2DSprite gamepad_frame;

	public UILabel counter;

	public Tooltip tooltip;

	public UI2DSprite gratitude_craft_label;

	public GameObject empty_item_gfx;

	public UI2DSprite icon_cap_limit;

	[SerializeField]
	[HideInInspector]
	private Collider2D _collider;

	public Collider2D collider
	{
		get
		{
			if (_collider == null)
			{
				_collider = container.GetComponent<Collider2D>();
			}
			if (_collider == null)
			{
				_collider = container.GetComponentInChildren<Collider2D>();
			}
			_ = _collider == null;
			return _collider;
		}
	}
}
