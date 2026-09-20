using UnityEngine;

public class RatBuffItemGUI : MonoBehaviour
{
	public UILabel txt_header;

	public UILabel txt_descr;

	public UILabel txt_timer;

	public UI2DSprite buff_icon;

	public Item buff;

	public bool buff_is_null;

	public string buff_id;

	public void Init()
	{
		buff = null;
		buff_is_null = true;
	}

	public void Draw(Item rat_buff)
	{
		buff = rat_buff;
		buff_is_null = buff == null;
		buff_id = (buff_is_null ? string.Empty : buff.id);
		txt_header.text = (buff_is_null ? "" : buff.definition.GetItemName());
		txt_descr.text = (buff_is_null ? "" : buff.definition.GetItemDescription(buff));
		buff_icon.sprite2D = (buff_is_null ? null : EasySpritesCollection.GetSprite(buff.definition.GetIcon()));
	}

	public void Redraw()
	{
		if (buff_is_null)
		{
			return;
		}
		if (buff == null)
		{
			buff_is_null = true;
			if (!string.IsNullOrEmpty(buff_id))
			{
				Draw(null);
				GUIElements.me.rat_cell_gui.Redraw();
			}
			return;
		}
		if (buff.id != buff_id)
		{
			Draw(buff);
		}
		if (buff.definition.has_durability)
		{
			int num = (int)(buff.durability / buff.definition.durability_decrease);
			int num2 = num % 60;
			txt_timer.text = num / 60 + ":" + ((num2 < 10) ? "0" : "") + num2;
		}
		else
		{
			txt_timer.text = string.Empty;
		}
	}

	public void Update()
	{
		Redraw();
	}
}
