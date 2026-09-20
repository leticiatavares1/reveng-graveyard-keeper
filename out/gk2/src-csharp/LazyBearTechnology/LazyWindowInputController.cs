using System;
using System.Collections.Generic;
using LazyBearTechnology;

public class LazyWindowInputController
{
	private readonly GamepadNavigationController gamepadNavigationController;

	private List<GamepadNavigationItem> customNavigationItems;

	private Dictionary<GameKey, Func<bool>> gameKeyDelegates;

	private bool isActive;

	private int rememberedGroup;

	private Func<GamepadNavigationItem> getFocusedAction;

	public List<GamepadNavigationItem> CustomNavigationItems
	{
		get
		{
			return customNavigationItems;
		}
		set
		{
			customNavigationItems = value;
		}
	}

	public LazyWindowInputController(GamepadNavigationController gamepadNavigationController, Dictionary<GameKey, Func<bool>> gameKeyDelegates, Func<GamepadNavigationItem> getFocusedAction)
	{
		this.gamepadNavigationController = gamepadNavigationController;
		this.getFocusedAction = getFocusedAction;
		InitKeys(gameKeyDelegates);
	}

	public void Enable(bool restoreFocused)
	{
		isActive = true;
		if (LazyInput.IsGamepadActive)
		{
			gamepadNavigationController.Enable();
			if (restoreFocused && gamepadNavigationController.HaveSavedFocusForGroup(rememberedGroup))
			{
				gamepadNavigationController.ReinitItems(focusOnFirstActive: false);
				gamepadNavigationController.RestoreFocus(rememberedGroup);
			}
			else
			{
				gamepadNavigationController.ReinitItems(focusOnFirstActive: true);
			}
		}
	}

	public void Disable(bool rememberFocused)
	{
		isActive = false;
		if (rememberFocused)
		{
			GamepadNavigationItem focusedItem = gamepadNavigationController.FocusedItem;
			if ((object)focusedItem != null)
			{
				rememberedGroup = focusedItem.group;
				gamepadNavigationController.RememberFocused(focusedItem);
			}
		}
		gamepadNavigationController.Disable();
	}

	public void Update()
	{
		if (isActive)
		{
			UpdatePressed();
		}
	}

	public void UpdateGamepadDependentStuff()
	{
		if (LazyInput.IsGamepadActive)
		{
			gamepadNavigationController.Enable();
			GamepadNavigationItem gamepadNavigationItem = getFocusedAction?.Invoke();
			if (gamepadNavigationItem != null)
			{
				gamepadNavigationController.ReinitItems(focusOnFirstActive: false, CustomNavigationItems);
				gamepadNavigationController.SetFocusedItem(gamepadNavigationItem);
			}
			else
			{
				gamepadNavigationController.ReinitItems(focusOnFirstActive: true, CustomNavigationItems);
			}
		}
		else
		{
			gamepadNavigationController.Disable();
		}
	}

	public bool OnPressedLeft()
	{
		return HandlePressedAndNavigate(GameKey.Left, GUIDirection.Left);
	}

	public bool OnPressedRight()
	{
		return HandlePressedAndNavigate(GameKey.Right, GUIDirection.Right);
	}

	public bool OnPressedUp()
	{
		return HandlePressedAndNavigate(GameKey.Up, GUIDirection.Up);
	}

	public bool OnPressedDown()
	{
		return HandlePressedAndNavigate(GameKey.Down, GUIDirection.Down);
	}

	public bool OnPressedLeftDpad()
	{
		return HandlePressedAndNavigate(GameKey.DpadLeft, GUIDirection.Left);
	}

	public bool OnPressedRightDpad()
	{
		return HandlePressedAndNavigate(GameKey.DpadRight, GUIDirection.Right);
	}

	public bool OnPressedUpDpad()
	{
		return HandlePressedAndNavigate(GameKey.DpadUp, GUIDirection.Up);
	}

	public bool OnPressedDownDpad()
	{
		return HandlePressedAndNavigate(GameKey.DpadDown, GUIDirection.Down);
	}

	public bool OnPressedSelect()
	{
		gamepadNavigationController.SelectFocusedItem();
		return true;
	}

	private bool HandlePressedAndNavigate(GameKey gameKey, GUIDirection direction)
	{
		if (gamepadNavigationController.ignoreHoldedKeys)
		{
			LazyInput.WaitForRelease(gameKey);
		}
		gamepadNavigationController.Navigate(direction);
		return true;
	}

	private void UpdatePressed()
	{
		foreach (KeyValuePair<GameKey, Func<bool>> gameKeyDelegate in gameKeyDelegates)
		{
			if (!isActive)
			{
				break;
			}
			if (LazyInput.GetKeyDown(gameKeyDelegate.Key) && gameKeyDelegate.Value())
			{
				LazyInput.ClearKeyDown(gameKeyDelegate.Key);
			}
		}
	}

	private void InitKeys(Dictionary<GameKey, Func<bool>> gameKeyDelegates)
	{
		this.gameKeyDelegates = gameKeyDelegates;
		this.gameKeyDelegates.TryAdd(GameKey.Select, OnPressedSelect);
		this.gameKeyDelegates.TryAdd(GameKey.Left, OnPressedLeft);
		this.gameKeyDelegates.TryAdd(GameKey.Right, OnPressedRight);
		this.gameKeyDelegates.TryAdd(GameKey.Up, OnPressedUp);
		this.gameKeyDelegates.TryAdd(GameKey.Down, OnPressedDown);
		this.gameKeyDelegates.TryAdd(GameKey.DpadLeft, OnPressedLeftDpad);
		this.gameKeyDelegates.TryAdd(GameKey.DpadRight, OnPressedRightDpad);
		this.gameKeyDelegates.TryAdd(GameKey.DpadUp, OnPressedUpDpad);
		this.gameKeyDelegates.TryAdd(GameKey.DpadDown, OnPressedDownDpad);
	}
}
