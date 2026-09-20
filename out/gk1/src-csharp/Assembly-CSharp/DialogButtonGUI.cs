using UnityEngine;

public class DialogButtonGUI : MonoBehaviour
{
	private UILabel _label;

	private UIButton _button;

	private DialogButtonsGUI _gui;

	private int _overflow_width;

	private UILabel.Overflow _overflow;

	private UIWidget[] _widgets;

	private int _anchor_top;

	private int _anchor_bottom;

	public void Init(DialogButtonsGUI gui)
	{
		_gui = gui;
		_label = GetComponentInChildren<UILabel>();
		_button = GetComponentInChildren<UIButton>();
		_widgets = GetComponentsInChildren<UIWidget>();
		_anchor_top = _label.topAnchor.absolute;
		_anchor_bottom = _label.bottomAnchor.absolute;
		_overflow = _label.overflowMethod;
		_overflow_width = _label.overflowWidth;
	}

	public void SetText(string text, bool translate = true)
	{
		_label.overflowMethod = _overflow;
		_label.overflowWidth = _overflow_width;
		bool flag = string.IsNullOrEmpty(text);
		SetActive(!flag);
		SetEnabled(!flag);
		_label.text = ((translate && !flag) ? GJL.L(text) : text);
	}

	public int GetWidth()
	{
		if (!base.gameObject.activeSelf)
		{
			return 0;
		}
		return _label.width;
	}

	public void SetWidth(int width)
	{
		_label.overflowMethod = UILabel.Overflow.ResizeHeight;
		_label.width = width;
		UIWidget[] widgets = _widgets;
		for (int i = 0; i < widgets.Length; i++)
		{
			widgets[i].UpdateAnchors();
		}
	}

	public void SetEnabled(bool active)
	{
		_button.isEnabled = active;
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public void OnClick()
	{
		_gui.OnBtnClicked(this);
	}

	public void RestoreHeight()
	{
		if (!(_label == null) && _label.topAnchor != null && _label.bottomAnchor != null)
		{
			_label.topAnchor.absolute = _anchor_top;
			_label.bottomAnchor.absolute = _anchor_bottom;
		}
	}
}
