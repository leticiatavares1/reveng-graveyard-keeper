using UnityEngine;

public class BubbleWidgetItem : BubbleWidget<BubbleWidgetItemData>
{
	[HideInInspector]
	[SerializeField]
	private BaseItemCellGUI _item_gui;

	[HideInInspector]
	[SerializeField]
	private GameObject _back;

	public override void Init()
	{
		_item_gui = GetComponent<BaseItemCellGUI>();
		_back = _item_gui.x1.back.gameObject;
		base.Init();
	}

	public override void Draw(BubbleWidgetItemData data)
	{
		if (!initialized)
		{
			Init();
		}
		base.data = data;
		if (data.IsEmpty())
		{
			this.Deactivate();
			return;
		}
		_item_gui.DrawItem(data.item_id, data.counter, init_tooltip: false, try_optimize_redraw: true, data.infinity_counter);
		if (!string.IsNullOrEmpty(data.icon_id))
		{
			ItemDefinition itemDefinition = GameBalance.me.GetData<ItemDefinition>(data.item_id);
			if (itemDefinition == null || !itemDefinition.is_big)
			{
				_item_gui.DrawIcon(data.icon_id, draw_back: true, hide_quality_icon: false);
				if (_item_gui.container.counter != null)
				{
					if (data.infinity_counter)
					{
						_item_gui.container.counter.text = "∞";
					}
					else
					{
						_item_gui.container.counter.text = ((data.counter == 1) ? "" : data.counter.ToString());
					}
				}
			}
		}
		if (_item_gui.container.counter != null)
		{
			_item_gui.container.counter.gameObject.SetActive(value: true);
		}
		if (_item_gui.quality_icon != null)
		{
			_item_gui.quality_icon.SetActive(data.show_quality);
		}
		_back.SetActive(data.show_back);
		_item_gui.DrawCapIcon(data.cap_limit);
		_item_gui.DrawGratitudeIcon(data.is_gratitude, data.is_enough_gratitude);
		GetSize();
	}

	public override Vector2 GetSize()
	{
		if (!initialized)
		{
			Init();
		}
		if (data != null && data.IsEmpty())
		{
			return Vector2.zero;
		}
		bool activeSelf = _back.activeSelf;
		ui_widget.width = (activeSelf ? 48 : 34);
		ui_widget.height = (activeSelf ? 48 : 34);
		return base.GetSize();
	}
}
