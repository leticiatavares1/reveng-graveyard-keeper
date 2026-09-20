using System.Collections.Generic;

namespace Rewired.Glyphs;

public static class GlyphTools
{
	public static bool TryGetActionElementMaps(int playerId, int actionId, AxisRange actionRange, ControllerElementGlyphSelectorOptions options, List<ActionElementMap> workingActionElementMaps, out ActionElementMap aemResult1, out ActionElementMap aemResult2)
	{
		aemResult1 = null;
		aemResult2 = null;
		if (!ReInput.isReady)
		{
			return false;
		}
		if (options == null)
		{
			return false;
		}
		if (workingActionElementMaps == null)
		{
			return false;
		}
		InputAction action = ReInput.mapping.GetAction(actionId);
		if (action == null)
		{
			return false;
		}
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return false;
		}
		Controller controller = player.controllers.GetLastActiveController();
		workingActionElementMaps.Clear();
		if (options.useLastActiveController && controller != null)
		{
			Controller controller2 = null;
			if (controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse)
			{
				if (IsMousePrioritizedOverKeyboard(options))
				{
					if (ReInput.controllers.Mouse.enabled && player.controllers.hasMouse)
					{
						controller = ReInput.controllers.Mouse;
						controller2 = ReInput.controllers.Keyboard;
					}
				}
				else if (ReInput.controllers.Keyboard.enabled && player.controllers.hasKeyboard)
				{
					controller = ReInput.controllers.Keyboard;
					controller2 = ReInput.controllers.Mouse;
				}
			}
			if (GetElementMapsWithAction(player, controller.type, controller.id, actionId, skipDisabledMaps: true, workingActionElementMaps) > 0 && TryGetActionElementMaps(action, actionRange, workingActionElementMaps, out aemResult1, out aemResult2))
			{
				return true;
			}
			if (controller2 != null && GetElementMapsWithAction(player, controller2.type, controller2.id, actionId, skipDisabledMaps: true, workingActionElementMaps) > 0 && TryGetActionElementMaps(action, actionRange, workingActionElementMaps, out aemResult1, out aemResult2))
			{
				return true;
			}
			if (GetElementMapsWithAction(player, controller.type, actionId, skipDisabledMaps: true, workingActionElementMaps) > 0 && TryGetActionElementMaps(action, actionRange, workingActionElementMaps, out aemResult1, out aemResult2))
			{
				return true;
			}
		}
		ControllerType controllerType;
		for (int i = 0; options.TryGetControllerTypeOrder(i, out controllerType); i++)
		{
			if (GetElementMapsWithAction(player, controllerType, actionId, skipDisabledMaps: true, workingActionElementMaps) > 0 && TryGetActionElementMaps(action, actionRange, workingActionElementMaps, out aemResult1, out aemResult2))
			{
				return true;
			}
		}
		if (GetElementMapsWithAction(player, actionId, skipDisabledMaps: true, workingActionElementMaps) > 0 && TryGetActionElementMaps(action, actionRange, workingActionElementMaps, out aemResult1, out aemResult2))
		{
			return true;
		}
		return false;
	}

	public static bool TryGetActionElementMaps(InputAction action, AxisRange actionRange, List<ActionElementMap> tempAems, out ActionElementMap aemResult1, out ActionElementMap aemResult2)
	{
		aemResult1 = null;
		aemResult2 = null;
		bool flag = action.type == InputActionType.Axis;
		int count = tempAems.Count;
		for (int i = 0; i < count; i++)
		{
			if (flag)
			{
				if (actionRange == AxisRange.Full)
				{
					ActionElementMap negativeAem = FindFirstFullAxisBinding(tempAems);
					if (negativeAem != null)
					{
						aemResult1 = negativeAem;
						return true;
					}
					if (FindFirstSplitAxisBindingPair(tempAems, out negativeAem, out var positiveAem))
					{
						aemResult1 = negativeAem;
						aemResult2 = positiveAem;
						return true;
					}
				}
				else
				{
					ActionElementMap negativeAem = FindFirstBinding(tempAems, actionRange);
					if (negativeAem != null)
					{
						aemResult1 = negativeAem;
						return true;
					}
				}
			}
			else
			{
				ActionElementMap negativeAem = FindFirstBinding(tempAems, actionRange);
				if (negativeAem != null)
				{
					aemResult1 = negativeAem;
					return true;
				}
			}
		}
		return false;
	}

	public static ActionElementMap FindFirstFullAxisBinding(List<ActionElementMap> actionElementMaps)
	{
		int count = actionElementMaps.Count;
		for (int i = 0; i < count; i++)
		{
			ActionElementMap actionElementMap = actionElementMaps[i];
			if (actionElementMap.elementType == ControllerElementType.Axis && actionElementMap.axisType == AxisType.Normal)
			{
				return actionElementMap;
			}
		}
		return null;
	}

	public static ActionElementMap FindFirstBinding(List<ActionElementMap> actionElementMaps, AxisRange actionRange)
	{
		if (actionElementMaps.Count == 0)
		{
			return null;
		}
		int count = actionElementMaps.Count;
		for (int i = 0; i < count; i++)
		{
			ActionElementMap actionElementMap = actionElementMaps[i];
			switch (actionRange)
			{
			case AxisRange.Full:
				if (actionElementMap.axisRange == AxisRange.Full)
				{
					return actionElementMap;
				}
				break;
			case AxisRange.Positive:
				if ((actionElementMap.axisType == AxisType.Split || actionElementMap.axisType == AxisType.None) && actionElementMap.axisContribution == Pole.Positive)
				{
					return actionElementMap;
				}
				break;
			case AxisRange.Negative:
				if ((actionElementMap.axisType == AxisType.Split || actionElementMap.axisType == AxisType.None) && actionElementMap.axisContribution == Pole.Negative)
				{
					return actionElementMap;
				}
				break;
			}
		}
		if (actionRange == AxisRange.Full)
		{
			for (int j = 0; j < count; j++)
			{
				ActionElementMap actionElementMap = actionElementMaps[j];
				if ((actionElementMap.axisType == AxisType.Split || actionElementMap.axisType == AxisType.None) && actionElementMap.axisContribution == Pole.Positive)
				{
					return actionElementMap;
				}
			}
		}
		return null;
	}

	public static bool FindFirstSplitAxisBindingPair(List<ActionElementMap> actionElementMaps, out ActionElementMap negativeAem, out ActionElementMap positiveAem)
	{
		negativeAem = null;
		positiveAem = null;
		int count = actionElementMaps.Count;
		for (int i = 0; i < count; i++)
		{
			ActionElementMap actionElementMap = actionElementMaps[i];
			if (actionElementMap.elementType == ControllerElementType.Axis)
			{
				if (actionElementMap.axisType == AxisType.Normal || actionElementMap.axisType == AxisType.None)
				{
					continue;
				}
			}
			else if (actionElementMap.elementType != ControllerElementType.Button)
			{
				continue;
			}
			if (actionElementMap.axisContribution == Pole.Positive)
			{
				if (positiveAem == null)
				{
					positiveAem = actionElementMap;
				}
			}
			else if (negativeAem == null)
			{
				negativeAem = actionElementMap;
			}
		}
		if (negativeAem == null)
		{
			return positiveAem != null;
		}
		return true;
	}

	public static bool IsMousePrioritizedOverKeyboard(ControllerElementGlyphSelectorOptions options)
	{
		if (options == null)
		{
			return false;
		}
		ControllerType controllerType;
		for (int i = 0; options.TryGetControllerTypeOrder(i, out controllerType); i++)
		{
			switch (controllerType)
			{
			case ControllerType.Mouse:
				return true;
			case ControllerType.Keyboard:
				return false;
			}
		}
		return false;
	}

	private static int GetElementMapsWithAction(Player player, ControllerType controllerType, int controllerId, int actionId, bool skipDisabledMaps, List<ActionElementMap> results)
	{
		int count = results.Count;
		player.controllers.maps.GetElementMapsWithAction(controllerType, controllerId, actionId, skipDisabledMaps, results);
		RemoveInvalidElementMaps(player, results, count);
		return results.Count - count;
	}

	private static int GetElementMapsWithAction(Player player, ControllerType controllerType, int actionId, bool skipDisabledMaps, List<ActionElementMap> results)
	{
		int count = results.Count;
		player.controllers.maps.GetElementMapsWithAction(controllerType, actionId, skipDisabledMaps, results);
		RemoveInvalidElementMaps(player, results, count);
		return results.Count - count;
	}

	private static int GetElementMapsWithAction(Player player, int actionId, bool skipDisabledMaps, List<ActionElementMap> results)
	{
		int count = results.Count;
		player.controllers.maps.GetElementMapsWithAction(actionId, skipDisabledMaps, results);
		RemoveInvalidElementMaps(player, results, count);
		return results.Count - count;
	}

	private static int RemoveInvalidElementMaps(Player player, List<ActionElementMap> results, int startIndex)
	{
		int count = results.Count;
		for (int num = count - 1; num >= startIndex; num--)
		{
			if (!player.controllers.ContainsController(results[num].controllerMap.controller) || !results[num].controllerMap.controller.enabled)
			{
				results.RemoveAt(num);
			}
		}
		return count - results.Count;
	}
}
