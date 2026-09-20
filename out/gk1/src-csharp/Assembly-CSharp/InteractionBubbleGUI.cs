using System.Collections.Generic;
using UnityEngine;

public class InteractionBubbleGUI : WidgetsBubbleGUI
{
	public const float WIDGET_ALPHA_WHEN_NOT_HIGHLIGHTED = 1f;

	[SerializeField]
	private Color _quality_k_color = Color.cyan;

	private static InteractionBubbleGUI _prefab;

	private static Dictionary<long, InteractionBubbleGUI> _bubbles = new Dictionary<long, InteractionBubbleGUI>();

	private static bool _available = true;

	private long _instance_id;

	private string _additional_line = "";

	public static Color quality_k_color => _prefab._quality_k_color;

	public override void Init()
	{
		_prefab = this;
		BaseGUI.on_window_opened += delegate
		{
			if (MainGame.game_started)
			{
				ChangeBubblesVisibility();
			}
		};
		BaseGUI.on_window_closed += delegate
		{
			if (MainGame.game_started)
			{
				ChangeBubblesVisibility();
			}
		};
		base.Init();
	}

	public static InteractionBubbleGUI Show(WorldGameObject wgo, BubbleWidgetDataContainer data)
	{
		if (_prefab == null)
		{
			return null;
		}
		InteractionBubbleGUI interactionBubbleGUI = GetBubble(wgo.unique_id, create_if_null: true);
		interactionBubbleGUI.linked_tf = wgo.bubble_pos_tf;
		if (wgo.obj_def != null && wgo.obj_def.hint_pos == ObjectDefinition.HintPos.AbovePlayer)
		{
			interactionBubbleGUI.linked_tf = MainGame.me.player.bubble_pos_tf;
		}
		interactionBubbleGUI.Show(data);
		interactionBubbleGUI.RefreshAlign(wgo);
		return interactionBubbleGUI;
	}

	public void RefreshAlign(WorldGameObject obj)
	{
		BubbleCornerPoint bubbleCornerPoint = obj.GetBubbleCornerPoint();
		SimpleUITable componentInChildren = GetComponentInChildren<SimpleUITable>();
		componentInChildren.alignment = ((bubbleCornerPoint == null || bubbleCornerPoint.bubble_custom_align == SimpleUITable.Alignment.NotSet) ? SimpleUITable.Alignment.Bottom : bubbleCornerPoint.bubble_custom_align);
		componentInChildren.Reposition();
	}

	public static void ShowAllRemoveBubbles()
	{
		foreach (InteractionBubbleGUI value in _bubbles.Values)
		{
			if (value.HasWidgetOfType(typeof(BubbleWidgetProgressData)))
			{
				value.SetActive(active: true);
			}
		}
	}

	public override void Update()
	{
		if (!MainGame.game_starting)
		{
			if (SpeechBubbleGUI.all.ContainsKey(_instance_id))
			{
				widget.alpha = 0f;
			}
			else if (base.data?.linked_wgo != null && MainGame.me.player.components.interaction.nearest != null && MainGame.me.player.components.interaction.nearest == base.data?.linked_wgo)
			{
				widget.alpha = 1f;
			}
			else
			{
				widget.alpha = 1f;
			}
			base.Update();
		}
	}

	public static InteractionBubbleGUI GetBubble(long instance_id, bool create_if_null = false)
	{
		if (_prefab == null)
		{
			return null;
		}
		InteractionBubbleGUI interactionBubbleGUI = null;
		if (_bubbles.ContainsKey(instance_id))
		{
			interactionBubbleGUI = _bubbles[instance_id];
		}
		else if (create_if_null)
		{
			interactionBubbleGUI = _prefab.Copy();
			interactionBubbleGUI._instance_id = instance_id;
			_bubbles.Add(instance_id, interactionBubbleGUI);
			if (_available)
			{
				interactionBubbleGUI.Activate();
			}
			else
			{
				interactionBubbleGUI.Deactivate();
			}
		}
		return interactionBubbleGUI;
	}

	public static void RemoveBubble(WorldGameObject wgo, bool immediate = false)
	{
		RemoveBubble(wgo.unique_id, immediate);
	}

	public static void RemoveBubble(long instance_id, bool immediate = false)
	{
		if (!(_prefab == null) && _bubbles.ContainsKey(instance_id))
		{
			_bubbles[instance_id].DestroyMe();
		}
	}

	public void ChangeBubblesVisibility()
	{
		bool should_be_active = MainGame.me.player.components.character.control_enabled;
		if (MainGame.me.gui_elements.build_mode_gui.is_shown)
		{
			should_be_active = true;
		}
		ChangeBubblesVisibility(should_be_active);
	}

	public static void ChangeBubblesVisibility(bool should_be_active)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Debug.Log("ChangeBubblesVisibility " + should_be_active);
		_available = should_be_active;
		if (should_be_active)
		{
			MainGame.me.player_char.player.RedrawQualities();
		}
		foreach (InteractionBubbleGUI value in _bubbles.Values)
		{
			value.SetActive(should_be_active);
		}
	}

	public static void DestroyAll()
	{
		List<InteractionBubbleGUI> list = new List<InteractionBubbleGUI>(_bubbles.Values);
		Debug.Log("Destroy All bubbles, count = " + list.Count);
		foreach (InteractionBubbleGUI item in list)
		{
			item.DestroyMe();
		}
		_bubbles.Clear();
		InteractionBubbleGUI[] componentsInChildren = GUIElements.me.interaction_bubble.transform.parent.GetComponentsInChildren<InteractionBubbleGUI>(includeInactive: true);
		foreach (InteractionBubbleGUI interactionBubbleGUI in componentsInChildren)
		{
			if (!(interactionBubbleGUI == GUIElements.me.interaction_bubble))
			{
				interactionBubbleGUI.DestroyMe();
			}
		}
	}

	public void DestroyMe()
	{
		widget.alpha = 0f;
		base.gameObject.Destroy();
		if (_bubbles.ContainsKey(_instance_id))
		{
			_bubbles.Remove(_instance_id);
		}
	}

	protected override void SortWidgets()
	{
		base.data.data_list.Sort(delegate(BubbleWidgetData a, BubbleWidgetData b)
		{
			int widget_id = (int)a.widget_id;
			int widget_id2 = (int)b.widget_id;
			return (widget_id != widget_id2) ? ((widget_id > widget_id2) ? 1 : (-1)) : 0;
		});
	}
}
