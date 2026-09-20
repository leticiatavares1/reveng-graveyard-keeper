using UnityEngine;

public class SaveSlotGUI : MonoBehaviour
{
	public UILabel slot_name;

	public UILabel txt_realtime;

	public UILabel txt_descr;

	public UILabel txt_stats;

	public GameObject delete_button;

	public UI2DSprite back;

	public Color mouse_over_color;

	public UIWidget gamepad_frame;

	[SerializeField]
	[HideInInspector]
	private SaveSlotsMenuGUI _menu;

	[HideInInspector]
	[SerializeField]
	private GamepadNavigationItem _gamepad_item;

	private SaveSlotData _data;

	public void InitPrefab(SaveSlotsMenuGUI menu)
	{
		this.Deactivate();
		_menu = menu;
		_gamepad_item = GetComponent<GamepadNavigationItem>();
	}

	public void Show(SaveSlotData save)
	{
		_data = save;
		if (_data == null)
		{
			slot_name.text = GJL.L("new game");
			UILabel uILabel = txt_realtime;
			UILabel uILabel2 = txt_stats;
			string text2 = (txt_descr.text = "");
			string text4 = (uILabel2.text = text2);
			uILabel.text = text4;
		}
		else
		{
			slot_name.text = "";
			txt_realtime.text = save.real_time;
			txt_stats.text = save.stats;
			txt_descr.text = "[c][F0A33E]" + GJL.L("save_slot_descr", $"{Mathf.Max(0f, save.game_time - 1.5f):0.0}").Replace(":", ":[-][/c]");
		}
		delete_button.SetActive(!BaseGUI.for_gamepad && _data != null);
		if (BaseGUI.for_gamepad)
		{
			_gamepad_item.SetCallbacks(OnOver, OnOut, OnSlotSelect);
		}
		gamepad_frame.Deactivate();
	}

	public void OnMouseOvered()
	{
		if (!BaseGUI.for_gamepad)
		{
			OnOver();
		}
	}

	public void OnMouseOut()
	{
		if (!BaseGUI.for_gamepad)
		{
			OnOut();
		}
	}

	public void OnOver()
	{
		_menu.OnSlotGamepadOvered(_data, this);
		gamepad_frame.Activate();
		Sounds.OnGUIHover();
	}

	public void OnOut()
	{
		gamepad_frame.Deactivate();
	}

	public void OnSlotSelect()
	{
		Debug.Log("On select " + slot_name.text);
		_menu.OnSelectSlotPressed(_data);
	}

	public void OnDeletePressed()
	{
		OnOut();
		_menu.OnDeleteSlotPressed(_data);
	}
}
