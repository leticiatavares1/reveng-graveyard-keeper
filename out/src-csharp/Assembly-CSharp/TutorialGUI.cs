using System.Collections.Generic;
using UnityEngine;

public class TutorialGUI : BaseGUI
{
	private GJCommons.VoidDelegate _on_closed;

	private List<GameObject> _children = new List<GameObject>();

	[Space(20f)]
	public string test_tutorial_id = "";

	private void TestTutorialID()
	{
		Open(test_tutorial_id);
	}

	public override void Init()
	{
		this.Deactivate();
		base.Init();
		_children.Clear();
		foreach (object item in base.transform)
		{
			GameObject gameObject = (item as Transform).gameObject;
			if (!gameObject.name.StartsWith("("))
			{
				_children.Add(gameObject);
			}
		}
	}

	public void Open(string id, GJCommons.VoidDelegate on_closed = null)
	{
		if (!BaseGUI.for_gamepad && id == "controls")
		{
			id = "controls_pc";
		}
		Debug.Log("Open tutorial window id = " + id);
		base.Open();
		_on_closed = on_closed;
		MainGame.me.player.SetParam("tut_shown_" + id, 1f);
		foreach (GameObject child in _children)
		{
			child.SetActive(child.name == id);
			if (!(child.name == id))
			{
				continue;
			}
			ControlsGUI component = child.GetComponent<ControlsGUI>();
			if (component != null)
			{
				component.just_opened = true;
			}
			if (BaseGUI.for_gamepad)
			{
				ButtonTipsStr componentInChildren = GetComponentInChildren<ButtonTipsStr>();
				if (componentInChildren != null)
				{
					componentInChildren.Print(GameKeyTip.Select("ok"));
				}
			}
		}
	}

	protected override bool OnPressedBack()
	{
		OnClosePressed();
		return true;
	}

	protected override bool OnPressedSelect()
	{
		OnClosePressed();
		return true;
	}

	public override void Hide(bool play_hide_sound = true)
	{
		base.Hide(play_hide_sound);
		if (_on_closed != null)
		{
			GJCommons.VoidDelegate on_closed = _on_closed;
			_on_closed = null;
			on_closed();
		}
	}
}
