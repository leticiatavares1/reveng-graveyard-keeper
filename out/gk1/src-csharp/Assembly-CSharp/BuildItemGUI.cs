using UnityEngine;

public class BuildItemGUI : MonoBehaviour
{
	[HideInInspector]
	public Tooltip tooltip;

	[HideInInspector]
	public UIButton button;

	[HideInInspector]
	public UIWidget widget;

	[HideInInspector]
	public GamepadNavigationItem gamepad_item;

	[HideInInspector]
	public BoxCollider2D box_collider;

	public UI2DSprite icon;

	public UI2DSprite gamepad_selection;

	public UI2DSprite mouse_selection;

	private CraftDefinition _craft_definition;

	private bool _for_gamepad;

	private bool _can_craft;

	public CraftDefinition definition => _craft_definition;

	public void InitPrefab()
	{
		box_collider = GetComponentInChildren<BoxCollider2D>();
		tooltip = GetComponentInChildren<Tooltip>();
		button = GetComponentInChildren<UIButton>();
		widget = GetComponentInChildren<UIWidget>();
		gamepad_item = GetComponentInChildren<GamepadNavigationItem>();
		gamepad_selection.Deactivate();
		mouse_selection.Deactivate();
		NGUIExtensionMethods.InitEventTriggers(this, OnOver, OnOut, OnItemSelect);
		this.Deactivate();
	}

	public void Init(ObjectCraftDefinition definition, bool can_craft, bool for_gamepad)
	{
		_craft_definition = definition;
		_for_gamepad = for_gamepad;
		_can_craft = can_craft;
		InitIcon();
		tooltip.SetCraftDefinition(definition);
		button.enabled = can_craft;
		icon.color = (can_craft ? button.defaultColor : button.disabledColor);
		gamepad_selection.Deactivate();
		mouse_selection.Deactivate();
		gamepad_item.SetCallbacks(OnOver, OnOut, OnItemSelect);
	}

	public void OnOver()
	{
		if (!_for_gamepad)
		{
			mouse_selection.Activate();
			return;
		}
		gamepad_selection.Activate();
		GUIElements.me.builds.UpdateTip(_can_craft);
	}

	public void OnOut()
	{
		(_for_gamepad ? gamepad_selection : mouse_selection).Deactivate();
	}

	public void OnItemSelect()
	{
		LazyInput.ClearKeyDown((!_for_gamepad) ? GameKey.LeftClick : GameKey.Select);
		Select();
	}

	public void Select()
	{
		if (MainGame.me.build_mode_logics.CanBuild(_craft_definition))
		{
			MainGame.me.build_mode_logics.CraftBuilding(_craft_definition);
		}
	}

	private void InitIcon()
	{
		Sprite sprite = null;
		if (!string.IsNullOrEmpty(_craft_definition.icon))
		{
			sprite = EasySpritesCollection.GetSprite(_craft_definition.icon);
		}
		if (!(sprite == null))
		{
			icon.DrawAndResize(sprite);
			gamepad_selection.Update();
			box_collider.size = icon.localSize;
			widget.width = gamepad_selection.width;
			widget.height = gamepad_selection.height;
		}
	}
}
