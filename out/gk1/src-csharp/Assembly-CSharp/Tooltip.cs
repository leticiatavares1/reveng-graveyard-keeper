using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
	public bool available = true;

	public TooltipBubbleGUI linked_tooltip;

	private UIWidget _widget;

	private bool _shown;

	private UIScrollView _scroll_view;

	private UIPanel _scroll_panel;

	private Transform _scroll_view_tf;

	private bool _is_scroll_table_item;

	private bool _initialized;

	public WidgetsBubbleGUI.Alignment alignment = WidgetsBubbleGUI.Alignment.Left;

	private BubbleWidgetDataContainer data = new BubbleWidgetDataContainer(WidgetsBubbleGUI.Alignment.Center);

	public string init_text = "";

	public bool has_info => data.has_data;

	public void Init()
	{
		if (!_initialized)
		{
			_widget = GetComponent<UIWidget>();
			_scroll_view = GetComponentInParent<UIScrollView>();
			_is_scroll_table_item = _scroll_view != null;
			_scroll_panel = (_is_scroll_table_item ? _scroll_view.GetComponent<UIPanel>() : null);
			_scroll_view_tf = (_is_scroll_table_item ? _scroll_view.transform : null);
			_initialized = true;
		}
	}

	public void OnEnable()
	{
		if (!string.IsNullOrEmpty(init_text))
		{
			Init();
			SetText(GJL.L(init_text));
		}
	}

	public bool IsScrolling()
	{
		if (!_is_scroll_table_item)
		{
			return false;
		}
		return DOTween.IsTweening(_scroll_view_tf);
	}

	public bool IsClippedByScrollView()
	{
		if (!_is_scroll_table_item || _widget == null)
		{
			return false;
		}
		Bounds bounds = _widget.CalculateBounds(_scroll_view_tf);
		Vector2 vector = _scroll_panel.CalculateConstrainOffset(bounds.min, bounds.max);
		if (!(Mathf.Abs(vector.x) > (float)_widget.width / 3f))
		{
			return Mathf.Abs(vector.y) > (float)_widget.height / 3f;
		}
		return true;
	}

	public void Show(bool for_gamepad)
	{
		if (!_shown && available && has_info && data.has_data)
		{
			data.alignment = alignment;
			linked_tooltip = TooltipBubbleGUI.Show(data, for_gamepad, GetComponent<Collider2D>());
			if (!(linked_tooltip == null))
			{
				_shown = true;
			}
		}
	}

	public void Hide()
	{
		if (_shown)
		{
			_shown = false;
			if (linked_tooltip != null)
			{
				linked_tooltip.DestroyBubble();
				linked_tooltip = null;
			}
		}
	}

	public void SetText(string text, UITextStyles.TextStyle style = UITextStyles.TextStyle.Usual)
	{
		Init();
		available = true;
		data.ClearData();
		data.SetData(new BubbleWidgetTextData(text, style));
	}

	public void SetData(params BubbleWidgetData[] data_params)
	{
		Init();
		available = true;
		data.SetData(data_params);
	}

	public void SetData(List<BubbleWidgetData> tooltip_datas)
	{
		Init();
		available = true;
		data.SetData(tooltip_datas);
	}

	public void AddData(BubbleWidgetData tooltip_data)
	{
		Init();
		available = true;
		data.AddData(tooltip_data);
	}

	public void ClearData()
	{
		data.ClearData();
	}

	public void SetCraftDefinition(CraftDefinition definition)
	{
		available = true;
		data.ClearData();
	}

	private void OnDestroy()
	{
		Hide();
	}
}
