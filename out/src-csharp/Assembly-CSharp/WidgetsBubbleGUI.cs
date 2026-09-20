using System;
using System.Collections.Generic;
using UnityEngine;

public class WidgetsBubbleGUI : BaseBubbleGUI
{
	public enum Alignment
	{
		Center,
		Left,
		Right
	}

	private const int MIN_WIDTH = 16;

	private const int MIN_HEIGHT = 6;

	public static Dictionary<Type, BubbleWidgetBase> widget_prefabs = new Dictionary<Type, BubbleWidgetBase>();

	protected bool for_gamepad;

	protected Vector3 pos;

	protected Vector3 alternative_pos = Vector3.zero;

	protected Collider2D linked_collider;

	[HideInInspector]
	[SerializeField]
	public UIWidget widget;

	[HideInInspector]
	[SerializeField]
	protected UITable ui_table;

	[HideInInspector]
	[SerializeField]
	protected SimpleUITable simple_table;

	public UITableOrGrid table;

	protected List<BubbleWidgetBase> bubble_widgets = new List<BubbleWidgetBase>();

	protected bool waiting_for_reposition;

	private bool _recalc_on_late_update;

	private BubbleWidgetDataContainer _last_drawn_data;

	private Dictionary<int, UILabel> _labels_hash = new Dictionary<int, UILabel>();

	private UILabel _label;

	public BubbleWidgetDataContainer data { get; protected set; }

	protected UILabel label
	{
		get
		{
			if (_label == null || (simple_table != null && !simple_table.use_hash))
			{
				_label = GetComponent<UILabel>();
			}
			return _label;
		}
	}

	private static WidgetsBubbleGUI bubble => UnityEngine.Object.FindObjectOfType<GUIElements>().GetComponentInChildren<InteractionBubbleGUI>(includeInactive: true);

	private UILabel GetLabelOfObject(MonoBehaviour o)
	{
		int instanceID = o.GetInstanceID();
		if (!_labels_hash.TryGetValue(instanceID, out var value))
		{
			value = o.GetComponent<UILabel>();
			_labels_hash.Add(instanceID, value);
		}
		return value;
	}

	public override void Init()
	{
		ui_table = GetComponentInChildren<UITable>(includeInactive: true);
		simple_table = GetComponentInChildren<SimpleUITable>(includeInactive: true);
		if (simple_table != null)
		{
			simple_table.use_hash = true;
		}
		widget = GetComponent<UIWidget>();
		bubble_widgets.AddRange(GetComponentsInChildren<BubbleWidgetBase>(includeInactive: true));
		Clear();
		base.Init();
	}

	private void OnEnable()
	{
		if (simple_table != null)
		{
			simple_table.Reposition();
			simple_table.use_hash = true;
		}
		else if (ui_table != null)
		{
			ui_table.Reposition();
		}
	}

	public void InitWidgetsContainer()
	{
		widget_prefabs.Clear();
		BubbleWidgetBase[] componentsInChildren = GetComponentsInChildren<BubbleWidgetBase>();
		foreach (BubbleWidgetBase bubbleWidgetBase in componentsInChildren)
		{
			widget_prefabs.Add(bubbleWidgetBase.GetWidgetType(), bubbleWidgetBase);
		}
		if (Application.isPlaying)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public bool HasWidgetOfType(Type type)
	{
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			if (bubble_widget.GetWidgetType() == type)
			{
				return true;
			}
		}
		return false;
	}

	public BubbleWidget<T> GetWidget<T>() where T : BubbleWidgetData
	{
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			if (bubble_widget.GetWidgetType() == typeof(T))
			{
				return bubble_widget as BubbleWidget<T>;
			}
		}
		return null;
	}

	public void Show(BubbleWidgetDataContainer data_container, bool force_redraw = false)
	{
		data = data_container;
		if (force_redraw)
		{
			_last_drawn_data = null;
		}
		Redraw();
	}

	public void Redraw(bool is_hint = false)
	{
		bool flag = false;
		if (!initialized || (ui_table == null && simple_table == null) || widget == null)
		{
			Init();
			flag = true;
		}
		if (!flag)
		{
			flag = true;
			if (_last_drawn_data?.data_list != null && data?.data_list != null && data.data_list.Count == _last_drawn_data.data_list.Count && !(data.linked_wgo != _last_drawn_data.linked_wgo) && data.alignment == _last_drawn_data.alignment)
			{
				bool flag2 = false;
				for (int i = 0; i < data.data_list.Count; i++)
				{
					BubbleWidgetData bubbleWidgetData = data.data_list[i];
					BubbleWidgetData bubbleWidgetData2 = _last_drawn_data.data_list[i];
					if (bubbleWidgetData.widget_id != bubbleWidgetData2.widget_id)
					{
						flag2 = true;
						break;
					}
					if (bubbleWidgetData.GetType() != bubbleWidgetData2.GetType())
					{
						flag2 = true;
						break;
					}
					if (bubbleWidgetData.IsEmpty() != bubbleWidgetData2.IsEmpty())
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					flag = false;
				}
			}
		}
		if (flag)
		{
			Clear();
			Transform parent = ((simple_table != null) ? simple_table.transform : ui_table.transform);
			SortWidgets();
			foreach (BubbleWidgetData item in data.data_list)
			{
				if (!item.IsEmpty())
				{
					Type type = item.GetType();
					if (widget_prefabs.ContainsKey(type))
					{
						BubbleWidgetBase bubbleWidgetBase = widget_prefabs[type].Copy(parent);
						GJL.EnsureChildLabelsHasCorrectFont(bubbleWidgetBase.gameObject, do_cache: false);
						GJL.ApplyCustomFontSettings(bubbleWidgetBase.gameObject);
						bubbleWidgetBase.BaseDraw(item);
						bubble_widgets.Add(bubbleWidgetBase);
					}
				}
			}
			if (simple_table == null && ui_table == null)
			{
				Debug.LogError(base.name + " widget hasn't any table", this);
			}
			if (!(this is InteractionBubbleGUI))
			{
				switch (data.alignment)
				{
				case Alignment.Center:
					if (simple_table != null)
					{
						simple_table.alignment = SimpleUITable.Alignment.Top;
					}
					if (ui_table != null)
					{
						ui_table.cellAlignment = UIWidget.Pivot.Top;
					}
					break;
				case Alignment.Left:
					if (simple_table != null)
					{
						simple_table.alignment = SimpleUITable.Alignment.TopLeft;
					}
					if (ui_table != null)
					{
						ui_table.cellAlignment = UIWidget.Pivot.TopLeft;
					}
					break;
				case Alignment.Right:
					if (simple_table != null)
					{
						simple_table.alignment = SimpleUITable.Alignment.TopRight;
					}
					if (ui_table != null)
					{
						ui_table.cellAlignment = UIWidget.Pivot.TopRight;
					}
					break;
				}
			}
			UpdateSizeAndWidgetsPositions();
			all_widgets = GetComponentsInChildren<UIWidget>();
			OnContentChanged();
			if (simple_table != null)
			{
				simple_table.Reposition();
			}
			Update();
			_recalc_on_late_update = true;
			UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].UpdateAnchors();
			}
			_last_drawn_data = data.GetCopy();
			return;
		}
		int num = 0;
		foreach (BubbleWidgetData item2 in data.data_list)
		{
			bubble_widgets[num].BaseDraw(item2);
			_last_drawn_data.data_list[num] = item2;
			num++;
		}
		Update();
	}

	public void MakeBottomAligned()
	{
		switch (data.alignment)
		{
		case Alignment.Center:
			if (simple_table != null)
			{
				simple_table.alignment = SimpleUITable.Alignment.Bottom;
			}
			if (ui_table != null)
			{
				ui_table.cellAlignment = UIWidget.Pivot.Bottom;
			}
			break;
		case Alignment.Left:
			if (simple_table != null)
			{
				simple_table.alignment = SimpleUITable.Alignment.BottomLeft;
			}
			if (ui_table != null)
			{
				ui_table.cellAlignment = UIWidget.Pivot.BottomLeft;
			}
			break;
		case Alignment.Right:
			if (simple_table != null)
			{
				simple_table.alignment = SimpleUITable.Alignment.BottomRight;
			}
			if (ui_table != null)
			{
				ui_table.cellAlignment = UIWidget.Pivot.BottomRight;
			}
			break;
		}
		UpdateSizeAndWidgetsPositions();
	}

	public void UpdateSizeAndWidgetsPositions()
	{
		if (!initialized || (ui_table == null && simple_table == null) || widget == null)
		{
			Init();
		}
		UpdateSize();
		Reposition();
		waiting_for_reposition = true;
		widget.UpdateAnchors();
	}

	private void UpdateSize()
	{
		Vector2 zero = Vector2.zero;
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			if (bubble_widget.gameObject.activeSelf)
			{
				Vector2 size = bubble_widget.GetSize();
				if (size.x > zero.x)
				{
					zero.x = size.x;
				}
				zero.y += size.y;
			}
		}
		if (zero.x < 16f)
		{
			zero.x = 16f;
		}
		if (zero.y < 6f)
		{
			zero.y = 6f;
		}
		widget.width = Mathf.CeilToInt(zero.x);
		widget.height = Mathf.CeilToInt(zero.y / 2f) * 2;
	}

	protected void Reposition()
	{
		if (simple_table != null)
		{
			simple_table.Reposition();
		}
		else
		{
			ui_table.Reposition();
		}
		waiting_for_reposition = true;
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			if (!(bubble_widget == null) && data != null)
			{
				UILabel labelOfObject = GetLabelOfObject(bubble_widget);
				if (data.alignment == Alignment.Left && labelOfObject != null)
				{
					labelOfObject.GetComponent<UIWidget>().pivot = UIWidget.Pivot.Left;
				}
			}
		}
	}

	public virtual void Update()
	{
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			bubble_widget.UpdateWidget();
		}
		if (linked_tf != null)
		{
			return;
		}
		if (for_gamepad)
		{
			if (linked_collider == null)
			{
				DestroyBubble();
				return;
			}
			if (widget.alpha.EqualsTo(0f))
			{
				widget.alpha = 1f;
			}
			Bounds bounds = linked_collider.bounds;
			pos = new Vector2(bounds.center.x, bounds.max.y);
			alternative_pos = new Vector2(bounds.center.x, bounds.min.y);
		}
		else
		{
			pos = MainGame.me.gui_cam.ScreenToWorldPoint(Input.mousePosition);
		}
		UpdateBubble(pos, use_world_cam: false, alternative_pos, ignore_halfres_magic: true);
	}

	public override void LateUpdate()
	{
		if (waiting_for_reposition)
		{
			waiting_for_reposition = false;
			Reposition();
		}
		base.LateUpdate();
		if (_recalc_on_late_update)
		{
			_recalc_on_late_update = false;
			OnContentChanged();
		}
	}

	protected void LinkColliderForGamepad(bool for_gamepad, Collider2D tooltip_collider)
	{
		if (for_gamepad && !(tooltip_collider == null))
		{
			this.for_gamepad = true;
			linked_collider = tooltip_collider;
		}
	}

	public void LinkTransform(Transform target)
	{
		linked_tf = target;
		LateUpdate();
	}

	protected void Clear()
	{
		if (bubble_widgets == null)
		{
			bubble_widgets = new List<BubbleWidgetBase>();
			return;
		}
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			bubble_widget.gameObject.SetActive(value: false);
			bubble_widget.DestroyGO();
		}
		bubble_widgets.Clear();
		simple_table?.ClearHashes();
	}

	protected virtual void SortWidgets()
	{
	}
}
