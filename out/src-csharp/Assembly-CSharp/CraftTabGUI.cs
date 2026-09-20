using System;
using UnityEngine;

public class CraftTabGUI : MonoBehaviour
{
	public string tab_id;

	private Action<string> _on_clicked_delegate;

	public UIChangingSprite tab_back;

	public UIButton button;

	public Color clr_normal;

	public Color clr_selected;

	public UI2DSprite spr_icon;

	public void OnTabClicked()
	{
		if (_on_clicked_delegate != null)
		{
			_on_clicked_delegate(tab_id);
		}
	}

	public void Draw(WorldGameObject craftery_wgo, string tab_id, Action<string> on_clicked_delegate)
	{
		this.tab_id = tab_id;
		if (string.IsNullOrEmpty(tab_id))
		{
			tab_id = "other";
		}
		string sprite_name = "tab_" + tab_id;
		if (this.tab_id.StartsWith("?"))
		{
			sprite_name = "tab_custom_" + tab_id.Substring(1);
		}
		spr_icon.sprite2D = EasySpritesCollection.GetSprite(sprite_name);
		if (spr_icon.sprite2D == null)
		{
			spr_icon.sprite2D = EasySpritesCollection.GetSprite("tab_other");
		}
		_on_clicked_delegate = on_clicked_delegate;
	}

	public void SetSelectedState(bool is_selected)
	{
		spr_icon.color = (is_selected ? clr_selected : clr_normal);
		tab_back.ChangeSprite((!is_selected) ? 1 : 0);
		button.normalSprite2D = tab_back.GetComponent<UI2DSprite>().sprite2D;
	}
}
