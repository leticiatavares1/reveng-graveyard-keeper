using System;
using UnityEngine;

public class BuffIcon : MonoBehaviour
{
	public UI2DSprite icon;

	public UILabel txt_timer;

	public bool show_timer;

	[NonSerialized]
	public PlayerBuff linked_buff;

	public void Draw(PlayerBuff buff)
	{
		linked_buff = buff;
		icon.sprite2D = EasySpritesCollection.GetSprite(buff.definition.GetIconName());
		show_timer = !linked_buff.definition.do_not_show_timer;
		Redraw();
	}

	public void Redraw()
	{
		if (show_timer)
		{
			string timerText = linked_buff.GetTimerText();
			if (timerText != txt_timer.text)
			{
				txt_timer.text = timerText;
			}
		}
		else
		{
			txt_timer.text = string.Empty;
		}
	}
}
