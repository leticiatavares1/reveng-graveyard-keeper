using System;
using UnityEngine;

public class BubbleWidgetOptionsItem : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	protected UIWidget ui_widget;

	[SerializeField]
	[HideInInspector]
	protected UIButton button;

	[SerializeField]
	[HideInInspector]
	protected UILabel label;

	public Color disable_outline_color;

	private Action callback;

	public void Init()
	{
		ui_widget = GetComponent<UIWidget>();
		label = GetComponent<UILabel>();
		button = GetComponent<UIButton>();
	}

	public void Draw(string name, Action callback, bool enabled = true)
	{
		base.name = name;
		label.text = GJL.L(name);
		this.callback = callback;
		if (!enabled)
		{
			label.color = button.disabledColor;
		}
		button.isEnabled = enabled;
		label.effectColor = (enabled ? Color.black : disable_outline_color);
	}

	public void OnItemSelect()
	{
		callback.TryInvoke();
	}

	public void OnItemOver()
	{
		if (button.isEnabled)
		{
			Sounds.OnGUIHover(Sounds.ElementType.ItemCell);
		}
	}

	public Vector2 GetSize()
	{
		if (ui_widget == null || label == null)
		{
			Init();
		}
		return ui_widget.localSize;
	}
}
