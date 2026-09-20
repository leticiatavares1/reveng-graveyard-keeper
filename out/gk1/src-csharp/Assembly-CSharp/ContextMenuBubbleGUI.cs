using System;
using UnityEngine;

public class ContextMenuBubbleGUI : WidgetsBubbleGUI
{
	public GJCommons.VoidDelegate _on_hide;

	[NonSerialized]
	public bool is_shown;

	public static ContextMenuBubbleGUI Show(string[] options, Action[] opt_delegates, Vector2 mouse_pos, GJCommons.VoidDelegate on_hide = null)
	{
		BubbleWidgetDataContainer bubbleWidgetDataContainer = new BubbleWidgetDataContainer(Alignment.Left);
		bubbleWidgetDataContainer.SetData(new BubbleWidgetDataOptions(options, opt_delegates));
		return Show(bubbleWidgetDataContainer, mouse_pos, on_hide);
	}

	public static ContextMenuBubbleGUI Show(BubbleWidgetDataOptions options_data, Vector2 mouse_pos, GJCommons.VoidDelegate on_hide = null)
	{
		return Show(new BubbleWidgetDataContainer(Alignment.Left, options_data), mouse_pos, on_hide);
	}

	public static ContextMenuBubbleGUI Show(BubbleWidgetDataContainer data, Vector2 mouse_pos, GJCommons.VoidDelegate on_hide = null)
	{
		if (data == null || !data.has_data)
		{
			Debug.LogError("no tooltip data");
			return null;
		}
		LazyInput.ClearAllKeysDown();
		ContextMenuBubbleGUI context_menu_bubble = GUIElements.me.context_menu_bubble;
		if (context_menu_bubble.is_shown)
		{
			context_menu_bubble.OnHide();
		}
		BubbleWidgetDataOptions bubbleWidgetDataOptions = data.GetData<BubbleWidgetDataOptions>();
		if (bubbleWidgetDataOptions != null)
		{
			bubbleWidgetDataOptions.on_hide = context_menu_bubble.OnHide;
		}
		BaseGUI.on_window_opened += context_menu_bubble.OnAnyWindowStateChanged;
		BaseGUI.on_window_closed += context_menu_bubble.OnAnyWindowStateChanged;
		context_menu_bubble.is_shown = true;
		context_menu_bubble.try_show_down = true;
		context_menu_bubble.table.DestroyChildren();
		context_menu_bubble.gameObject.SetActive(value: true);
		context_menu_bubble.Show(data, force_redraw: true);
		context_menu_bubble._on_hide = on_hide;
		Vector3 world_pos = MainGame.me.gui_cam.ScreenToWorldPoint(Input.mousePosition);
		context_menu_bubble.UpdateBubble(world_pos, use_world_cam: false);
		return context_menu_bubble;
	}

	public void OnAnyWindowStateChanged(BaseGUI window_obj)
	{
		OnHide();
	}

	public void OnBackClicked()
	{
		OnHide();
		if (!LazyInput.GetKeyDown(GameKey.RightClick))
		{
			return;
		}
		Collider2D[] collidersUnderMouse = NGUIExtensionMethods.GetCollidersUnderMouse(MainGame.me.gui_cam);
		foreach (Collider2D obj in collidersUnderMouse)
		{
			UIEventTrigger component = obj.GetComponent<UIEventTrigger>();
			if (component != null)
			{
				EventDelegate.Execute(component.onHoverOver);
				EventDelegate.Execute(component.onPress);
			}
			UIButton component2 = obj.GetComponent<UIButton>();
			if (component2 != null && component2.isEnabled)
			{
				EventDelegate.Execute(component2.onClick);
			}
		}
	}

	public void OnHide()
	{
		base.gameObject.SetActive(value: false);
		BaseGUI.on_window_opened -= OnAnyWindowStateChanged;
		BaseGUI.on_window_closed -= OnAnyWindowStateChanged;
		is_shown = false;
		_on_hide.TryInvoke();
	}

	public override void Update()
	{
		foreach (BubbleWidgetBase bubble_widget in bubble_widgets)
		{
			bubble_widget.UpdateWidget();
		}
		if (LazyInput.AnyKeyDown() && !LazyInput.AnyClick())
		{
			OnHide();
		}
	}
}
