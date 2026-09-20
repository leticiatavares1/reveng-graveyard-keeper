using UnityEngine;

public class TechBranchGUIItem : MonoBehaviour
{
	private int _branch_id;

	public UILabel txt_name;

	public UI2DSprite icon;

	public UI2DSprite icon_2;

	public GameObject go_selected;

	public GameObject go_gamepad_over_full;

	public GameObject go_gamepad_over_folded;

	public GameObject go_not_selected;

	public int width_not_selected;

	public int width_selected;

	private bool _is_selected;

	private TechTreeGUI _tech_tree;

	public void Draw(int branch_id)
	{
		_branch_id = branch_id;
		UI2DSprite uI2DSprite = icon_2;
		Sprite sprite2D = (icon.sprite2D = EasySpritesCollection.GetSprite("i_tbranch_" + branch_id));
		uI2DSprite.sprite2D = sprite2D;
		txt_name.text = GJL.L("tbranch_" + branch_id);
		GetComponent<Tooltip>().SetText(txt_name.text);
		Redraw();
		_tech_tree = GUIElements.me.tech_tree;
		OnGamepadOut();
	}

	public void Redraw()
	{
		_is_selected = MainGame.me.gui_elements.tech_tree.current_branch == _branch_id;
		GetComponent<Tooltip>().available = !_is_selected;
		go_selected.SetActive(_is_selected);
		go_not_selected.SetActive(!_is_selected);
		GetComponent<UIWidget>().width = (_is_selected ? width_selected : width_not_selected);
	}

	public void OnGamepadSelectedTechBranch()
	{
		SelectCurrentTechBranch();
		OnGamepadOver();
	}

	public void SelectCurrentTechBranch()
	{
		if (!BaseGUI.IsLastClickRightButton())
		{
			_tech_tree.SelectTechBranch(_branch_id);
			if (!Sounds.WasAnySoundPlayedThisFrame())
			{
				Sounds.OnGUIClick();
			}
		}
	}

	private void OnGamepadOver()
	{
		_tech_tree.gamepad_controller.restore_last_in_group = false;
		if (_tech_tree.current_branch == _branch_id)
		{
			go_gamepad_over_full.SetActive(value: true);
			go_gamepad_over_folded.SetActive(value: false);
			_tech_tree.button_tips.Print(GameKeyTip.Close());
		}
		else
		{
			go_gamepad_over_full.SetActive(value: false);
			go_gamepad_over_folded.SetActive(value: true);
			_tech_tree.button_tips.Print(GameKeyTip.Select("open"), GameKeyTip.Close());
		}
		if (!Sounds.WasAnySoundPlayedThisFrame())
		{
			Sounds.OnGUIHover();
		}
	}

	private void OnGamepadOut()
	{
		go_gamepad_over_full.SetActive(value: false);
		go_gamepad_over_folded.SetActive(value: false);
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad && !Sounds.WasAnySoundPlayedThisFrame())
		{
			Sounds.OnGUIHover();
		}
	}

	public void OnMouseOuted()
	{
		_ = BaseGUI.for_gamepad;
	}
}
