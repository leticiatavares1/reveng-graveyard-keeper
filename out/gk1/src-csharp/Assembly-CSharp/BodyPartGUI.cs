using UnityEngine;

public class BodyPartGUI : MonoBehaviour
{
	public enum State
	{
		Empty,
		Exists
	}

	public Color mouse_frame_color = Color.magenta;

	public Color gamepad_frame_color = Color.magenta;

	public ItemDefinition.ItemType type;

	[SerializeField]
	private UI2DSprite _sprite_back;

	[SerializeField]
	private UI2DSprite _sprite_selected;

	[SerializeField]
	private UI2DSprite _sprite_frame;

	private AutopsyGUI _autopsy_gui;

	private GamepadNavigationItem _gamepad_item;

	private bool _for_gamepad;

	private State _state;

	public bool exists => _state == State.Exists;

	public GamepadNavigationItem gamepad_item
	{
		get
		{
			if (_gamepad_item == null)
			{
				_gamepad_item = GetComponent<GamepadNavigationItem>();
			}
			return _gamepad_item;
		}
	}

	public void Init()
	{
		gamepad_item.SetCallbacks(OnOver, OnOut, OnPartSelect);
		NGUIExtensionMethods.InitEventTriggers(this, OnOver, OnOut, OnPartSelect);
	}

	public void Open(AutopsyGUI autopsy_gui, bool for_gamepad, bool exists)
	{
		this.Activate();
		_autopsy_gui = autopsy_gui;
		_for_gamepad = for_gamepad;
		_sprite_frame.color = (for_gamepad ? gamepad_frame_color : mouse_frame_color);
		_sprite_frame.Deactivate();
		_sprite_selected.Deactivate();
		_state = (exists ? State.Exists : State.Empty);
		gamepad_item.active = exists;
		base.gameObject.SetActive(exists);
		_sprite_back.SetActive(exists);
		_sprite_selected.SetActive(exists);
	}

	public void OnOver()
	{
		if (exists)
		{
			_sprite_frame.SetActive(active: true);
		}
	}

	public void OnOut()
	{
		_sprite_frame.SetActive(active: false);
	}

	public void OnPartSelect()
	{
		_ = exists;
	}

	public void ChangeSelectionState(bool active)
	{
		if (exists)
		{
			_sprite_selected.SetActive(active);
		}
	}

	public void Reinit()
	{
		for (int i = 0; i < base.transform.childCount; i++)
		{
			UI2DSprite component = base.transform.GetChild(i).GetComponent<UI2DSprite>();
			string text = component.name;
			if (text.Contains("back"))
			{
				_sprite_back = component;
			}
			if (text.Contains("selected"))
			{
				_sprite_selected = component;
			}
			if (text.Contains("frame"))
			{
				_sprite_frame = component;
			}
		}
		_sprite_back.Activate();
		_sprite_selected.Activate();
		_sprite_frame.Activate();
	}
}
