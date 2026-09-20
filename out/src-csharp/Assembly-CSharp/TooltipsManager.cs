using UnityEngine;

public class TooltipsManager : MonoBehaviour
{
	private static TooltipsManager _instance;

	public Tooltip current_tooltip;

	private bool _skip_update;

	public static TooltipsManager me => _instance ?? (_instance = Object.FindObjectOfType<TooltipsManager>());

	private void Awake()
	{
		_instance = this;
	}

	private void Update()
	{
		Tooltip tooltip = FindOveredTooltip();
		if (!(current_tooltip == tooltip))
		{
			if (current_tooltip != null)
			{
				current_tooltip.Hide();
			}
			current_tooltip = tooltip;
			if (!(current_tooltip == null))
			{
				current_tooltip.Show(for_gamepad: true);
			}
		}
	}

	private Tooltip FindOveredTooltip()
	{
		if (BaseGUI.active_gui == null || GUIElements.me.context_menu_bubble.is_shown)
		{
			return null;
		}
		Vector2 point;
		if (BaseGUI.for_gamepad)
		{
			if (!(GamepadNavigationController.current != null) || !(GamepadNavigationController.current.focused_item != null))
			{
				return null;
			}
			point = GamepadNavigationController.current.focused_item.pos;
		}
		else
		{
			point = MainGame.me.gui_cam.ScreenToWorldPoint(Input.mousePosition);
		}
		Collider2D[] array = Physics2D.OverlapPointAll(point, 8192);
		int num = 0;
		Collider2D[] array2 = array;
		foreach (Collider2D collider2D in array2)
		{
			UIWidget component = collider2D.GetComponent<UIWidget>();
			int depth;
			if (component != null && component.panel != null)
			{
				depth = component.panel.depth;
			}
			else
			{
				UIPanel component2 = collider2D.GetComponent<UIPanel>();
				if (component2 == null)
				{
					continue;
				}
				depth = component2.depth;
			}
			if (depth > num)
			{
				num = depth;
			}
		}
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].GetComponent<TooltipBlocker>() != null)
			{
				return null;
			}
		}
		array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Tooltip component3 = array2[i].GetComponent<Tooltip>();
			if (!(component3 == null) && component3.gameObject.activeInHierarchy && component3.available && component3.has_info && !component3.IsScrolling() && !component3.IsClippedByScrollView())
			{
				UIWidget component4 = component3.GetComponent<UIWidget>();
				if (!(component4 == null) && !(component4.panel == null) && component4.panel.depth >= num)
				{
					return component3;
				}
			}
		}
		return null;
	}

	public static void Redraw()
	{
		if (!_instance._skip_update)
		{
			Tooltip tooltip = _instance.current_tooltip;
			if (tooltip != null)
			{
				tooltip.Hide();
			}
			_instance.current_tooltip = null;
			_instance.Update();
		}
	}
}
