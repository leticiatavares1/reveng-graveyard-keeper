using UnityEngine;

public class DialogGUI : BaseGUI
{
	private DialogButtonsGUI _dialog_buttons;

	public UILabel label_1;

	public UILabel label_2;

	public UILabel label_3;

	public UILabel stars_1;

	public UILabel stars_2;

	private bool _run_second_option_by_pressed_back;

	public UITable table;

	public GameObject item_container;

	public GameObject button_separator;

	public GameObject ingredients_go;

	private BaseItemCellGUI[] _ingredients;

	public override void Init()
	{
		_dialog_buttons = GetComponentInChildren<DialogButtonsGUI>(includeInactive: true);
		if (_dialog_buttons != null)
		{
			_dialog_buttons.Init();
		}
		if (ingredients_go != null)
		{
			_ingredients = ingredients_go.GetComponentsInChildren<BaseItemCellGUI>(includeInactive: true);
		}
		this.Deactivate();
		base.Init();
	}

	public void OpenYesNo(string text, GJCommons.VoidDelegate delegate_1, GJCommons.VoidDelegate delegate_2 = null, GJCommons.VoidDelegate on_hide = null)
	{
		Open(text, GJL.L("yes"), delegate_1, GJL.L("no"), delegate_2, on_hide);
	}

	public void OpenOK(string text, GJCommons.VoidDelegate on_hide = null, string text2 = "", bool separate_with_stars = false, string text3 = "")
	{
		Open(text, GJL.L("ok"), null, null, null, on_hide, GameKey.Select, GameKey.Back, text2, separate_with_stars, text3);
	}

	public void Open(string text, string option_1, GJCommons.VoidDelegate delegate_1, string option_2 = null, GJCommons.VoidDelegate delegate_2 = null, GJCommons.VoidDelegate on_hide = null, GameKey key_1 = GameKey.Select, GameKey key_2 = GameKey.Back, string text2 = "", bool separate_with_stars = false, string text3 = "")
	{
		OpenDialog(text, option_1, delegate_1, option_2, delegate_2, on_hide, key_1, key_2, text2, null, separate_with_stars, text3);
	}

	public void OpenDialog(string text, string option_1, GJCommons.VoidDelegate delegate_1, string option_2 = null, GJCommons.VoidDelegate delegate_2 = null, GJCommons.VoidDelegate on_hide = null, GameKey key_1 = GameKey.Select, GameKey key_2 = GameKey.Back, string text2 = "", Item item = null, bool separate_with_stars = false, string text3 = "")
	{
		base.Open();
		if (label_1 != null)
		{
			label_1.text = LocalizedLabel.ColorizeTags(GJL.L(text), LocalizedLabel.TextColor.Tutorial);
		}
		if (label_2 != null)
		{
			label_2.text = GJL.L(text2);
			label_2.gameObject.SetActive(!string.IsNullOrEmpty(text2));
		}
		if (label_3 != null)
		{
			label_3.text = GJL.L(text3);
			label_3.gameObject.SetActive(!string.IsNullOrEmpty(text3));
		}
		if (stars_1 != null)
		{
			stars_1.gameObject.SetActive(separate_with_stars && !string.IsNullOrEmpty(text2));
		}
		if (stars_2 != null)
		{
			stars_2.gameObject.SetActive(separate_with_stars && !string.IsNullOrEmpty(text3));
		}
		if (button_separator != null)
		{
			button_separator.SetActive(!string.IsNullOrEmpty(text2));
		}
		if (ingredients_go != null)
		{
			ingredients_go.SetActive(value: false);
		}
		if (item_container != null)
		{
			item_container.SetActive(item != null);
			if (item != null)
			{
				if (item.inventory != null && item.inventory.Count > 0)
				{
					ingredients_go.SetActive(value: true);
					item_container.SetActive(value: false);
					BaseItemCellGUI.DrawIngredients(_ingredients, item.inventory, MainGame.me.player.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true));
					ingredients_go.GetComponentInChildren<UITableOrGrid>().Reposition();
				}
				else
				{
					item_container.GetComponent<BaseItemCellGUI>().DrawIngredient(item, MainGame.me.player.GetMultiInventory(null, "", MultiInventory.PlayerMultiInventory.DontChange, include_toolbelt: false, sortWGOS: true));
				}
			}
		}
		if (_dialog_buttons != null)
		{
			_dialog_buttons.Set(option_1, delegate
			{
				Hide();
				on_hide.TryInvoke();
				delegate_1.TryInvoke();
			}, option_2, delegate
			{
				if (base.is_shown)
				{
					Hide();
					on_hide.TryInvoke();
					delegate_2.TryInvoke();
				}
			}, null, null, key_1, key_2);
		}
		if (table != null)
		{
			table.Reposition();
		}
		UpdateAllAnchors();
		DialogButtonGUI[] componentsInChildren = base.gameObject.GetComponentsInChildren<DialogButtonGUI>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].RestoreHeight();
		}
	}
}
