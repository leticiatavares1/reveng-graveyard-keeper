using System;
using UnityEngine;

public class PerkBuffItemGUI : MonoBehaviour
{
	public UIWidget widget;

	public GameObject go_perk;

	public GameObject go_buff;

	public UILabel txt_header;

	public UILabel txt_descr;

	public UILabel txt_timer;

	public UI2DSprite icon_perk;

	public UI2DSprite icon_buff;

	private bool _is_buff;

	[NonSerialized]
	public PlayerBuff linked_buff;

	private int _default_hgt;

	public void Draw(PlayerBuff buff)
	{
		go_buff.SetActive(value: true);
		go_perk.SetActive(value: false);
		_is_buff = true;
		linked_buff = buff;
		txt_header.text = buff.definition.GetLocalizedName();
		txt_descr.text = buff.definition.GetDescriptionIfExists();
		icon_buff.sprite2D = EasySpritesCollection.GetSprite(buff.definition.GetIconName());
		RecalculateDescriptionHeight();
		GetComponentInChildren<SimpleUITable>().Reposition();
		Redraw();
	}

	public void Draw(PerkDefinition perk)
	{
		go_buff.SetActive(value: false);
		go_perk.SetActive(value: true);
		_is_buff = false;
		linked_buff = null;
		txt_header.text = GJL.L(perk.id);
		txt_descr.text = perk.GetDescriptionIfExists();
		icon_perk.sprite2D = EasySpritesCollection.GetSprite(perk.GetIcon());
		RecalculateDescriptionHeight();
		GetComponentInChildren<SimpleUITable>().Reposition();
		Redraw();
	}

	private void RecalculateDescriptionHeight()
	{
		txt_descr.ProcessText();
		int num = Mathf.RoundToInt(txt_descr.localSize.y - 20f);
		if (num > 0)
		{
			if (_default_hgt == 0)
			{
				_default_hgt = widget.height;
			}
			widget.height = _default_hgt + num;
			icon_perk.transform.localPosition = new Vector3(icon_perk.transform.localPosition.x, num / 2);
		}
	}

	public void Redraw()
	{
		string text = txt_timer.text;
		if (_is_buff)
		{
			text = ((!linked_buff.definition.do_not_show_timer) ? linked_buff.GetTimerText() : string.Empty);
		}
		if (text != txt_timer.text)
		{
			txt_timer.text = text;
		}
	}

	public void Update()
	{
		Redraw();
	}
}
