using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SimpleUITable : MonoBehaviour
{
	public enum Alignment
	{
		NotSet = -1,
		Center = 0,
		Left = 1,
		Right = 2,
		Top = 10,
		TopLeft = 11,
		TopRight = 12,
		Bottom = 20,
		BottomLeft = 21,
		BottomRight = 22
	}

	public int offset;

	public Alignment alignment;

	public bool dont_change_x;

	public bool resize_widget_height;

	private readonly List<UIWidget> _widgets = new List<UIWidget>();

	private Dictionary<int, UIWidget> _widgets_hash = new Dictionary<int, UIWidget>();

	private Dictionary<int, UILabel> _labels_hash = new Dictionary<int, UILabel>();

	[NonSerialized]
	public bool use_hash;

	private UIWidget _widget;

	protected UIWidget widget
	{
		get
		{
			if (!use_hash || _widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void OnValidate()
	{
		if (base.enabled)
		{
			Reposition();
		}
	}

	public void ClearHashes()
	{
		_widgets_hash.Clear();
		_labels_hash.Clear();
	}

	private UIWidget GetWidgetByTransform(Transform t)
	{
		int instanceID = t.GetInstanceID();
		if (!use_hash || !_widgets_hash.TryGetValue(instanceID, out var value))
		{
			value = t.GetComponent<UIWidget>();
			if (use_hash)
			{
				_widgets_hash.Add(instanceID, value);
			}
		}
		return value;
	}

	private UILabel GetLabelOfObject(MonoBehaviour o)
	{
		int instanceID = o.GetInstanceID();
		if (!use_hash || !_labels_hash.TryGetValue(instanceID, out var value))
		{
			value = o.GetComponent<UILabel>();
			if (use_hash)
			{
				_labels_hash.Add(instanceID, value);
			}
		}
		return value;
	}

	public void Reposition()
	{
		Transform transform = base.transform;
		_widgets.Clear();
		int childCount = transform.childCount;
		int num = 0;
		int num2 = 0;
		int offset_y;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			UIWidget widgetByTransform = GetWidgetByTransform(child);
			if (!(widgetByTransform == null) && widgetByTransform.gameObject.activeInHierarchy)
			{
				if (widgetByTransform.width > num)
				{
					num = widgetByTransform.width;
				}
				num2 += CalculateWidgetHeight(widgetByTransform, out offset_y);
				if (i < childCount - 1)
				{
					num2 += offset;
				}
				_widgets.Add(widgetByTransform);
			}
		}
		if (_widgets.Count == 0)
		{
			return;
		}
		Alignment alignment = (Alignment)((int)this.alignment % 10);
		Alignment alignment2 = (Alignment)(Mathf.FloorToInt((float)this.alignment / 10f) * 10);
		int num3 = 0;
		int num4 = 0;
		if (alignment2 != Alignment.Top)
		{
			num3 += ((alignment2 == Alignment.Center) ? Mathf.RoundToInt((float)num2 / 2f) : num2);
		}
		for (int j = 0; j < _widgets.Count; j++)
		{
			UIWidget uIWidget = _widgets[j];
			int num5 = CalculateWidgetHeight(uIWidget, out offset_y);
			int num6 = Mathf.CeilToInt((float)num5 / 2f);
			num4 += num5;
			int num7 = Mathf.CeilToInt((float)uIWidget.height / 2f);
			int num8 = 0;
			if (alignment != 0)
			{
				num8 = (Mathf.CeilToInt((float)num / 2f) - num6) * ((alignment != Alignment.Left) ? 1 : (-1));
			}
			int num9 = num3 - num7;
			num3 -= num5 + offset;
			if (j > 0)
			{
				num4 += offset;
			}
			switch (uIWidget.pivot)
			{
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Top:
			case UIWidget.Pivot.TopRight:
				num9 += num7;
				break;
			case UIWidget.Pivot.BottomLeft:
			case UIWidget.Pivot.Bottom:
			case UIWidget.Pivot.BottomRight:
				num9 -= num7;
				break;
			}
			switch (uIWidget.pivot)
			{
			case UIWidget.Pivot.TopLeft:
			case UIWidget.Pivot.Left:
			case UIWidget.Pivot.BottomLeft:
				num8 -= num6;
				break;
			case UIWidget.Pivot.TopRight:
			case UIWidget.Pivot.Right:
			case UIWidget.Pivot.BottomRight:
				num8 += num6;
				break;
			}
			Transform transform2 = uIWidget.transform;
			if (dont_change_x)
			{
				num8 = Mathf.RoundToInt(transform2.localPosition.x);
			}
			transform2.localPosition = new Vector3(num8, num9 + offset_y);
		}
		if (resize_widget_height)
		{
			widget.height = num4;
		}
	}

	private int CalculateWidgetHeight(UIWidget w, out int offset_y)
	{
		int height = w.height;
		offset_y = 0;
		UILabel labelOfObject = GetLabelOfObject(w);
		if (labelOfObject != null)
		{
			offset_y -= Mathf.FloorToInt((float)labelOfObject.spacingY / 2f);
		}
		return height;
	}
}
