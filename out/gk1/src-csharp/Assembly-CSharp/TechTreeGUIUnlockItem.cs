using System;
using UnityEngine;

public class TechTreeGUIUnlockItem : MonoBehaviour
{
	public UI2DSprite spr;

	public UI2DSprite item_quality;

	public UILabel label_name;

	public UILabel label_description;

	public UIWidget background;

	public void Draw(TechUnlock unlock, bool init_tooltip)
	{
		base.gameObject.SetActive(unlock != null);
		if (unlock != null)
		{
			TechUnlock.TechUnlockData data = unlock.GetData();
			Draw(data, init_tooltip ? unlock : null);
		}
	}

	public void Draw(TechUnlock.TechUnlockData unlock_data, TechUnlock tooltip_data = null)
	{
		base.gameObject.SetActive(unlock_data != null);
		if (unlock_data == null)
		{
			return;
		}
		spr.sprite2D = unlock_data.sprite;
		if (label_name != null)
		{
			label_name.text = unlock_data.name;
		}
		if (label_description != null)
		{
			label_description.text = unlock_data.description;
			label_description.gameObject.SetActive(!string.IsNullOrEmpty(label_description.text));
			label_description.ProcessText();
			if (background != null)
			{
				background.height = Math.Max(46, 28 + (int)label_description.localSize.y);
			}
		}
		if (item_quality != null)
		{
			item_quality.sprite2D = unlock_data.quality_icon;
			item_quality.SetActive(item_quality.sprite2D != null);
		}
		if (tooltip_data != null)
		{
			Tooltip component = GetComponent<Tooltip>();
			if (component != null)
			{
				component.ClearData();
				tooltip_data.GetTooltip(component);
			}
		}
		else
		{
			BoxCollider2D component2 = GetComponent<BoxCollider2D>();
			if (component2 != null)
			{
				component2.enabled = false;
			}
		}
		SimpleUITable componentInChildren = GetComponentInChildren<SimpleUITable>();
		if (componentInChildren != null)
		{
			componentInChildren.Reposition();
		}
	}
}
