using System;
using UnityEngine;

namespace Rewired.Glyphs;

[Serializable]
public class ControllerElementGlyphSelectorOptions
{
	[Tooltip("Determines if the Player's last active controller is used for glyph selection.")]
	[SerializeField]
	private bool _useLastActiveController = true;

	[Tooltip("List of controller type priority. First in list corresponds to highest priority. This determines which controller types take precedence when displaying glyphs. If use last active controller is enabled, the active controller will always take priority, however, if there is no last active controller, selection will fall back based on this priority. In addition, keyboard and mouse are treated as a single controller for the purposes of glyph handling, so to prioritze keyboard over mouse or vice versa, the one that is lower in the list will take precedence.")]
	[SerializeField]
	private ControllerType[] _controllerTypeOrder = new ControllerType[4]
	{
		ControllerType.Joystick,
		ControllerType.Custom,
		ControllerType.Mouse,
		ControllerType.Keyboard
	};

	private static ControllerElementGlyphSelectorOptions s_defaultOptions;

	public bool useLastActiveController
	{
		get
		{
			return _useLastActiveController;
		}
		set
		{
			_useLastActiveController = value;
		}
	}

	public ControllerType[] controllerTypeOrder
	{
		get
		{
			return _controllerTypeOrder;
		}
		set
		{
			_controllerTypeOrder = value;
		}
	}

	public static ControllerElementGlyphSelectorOptions defaultOptions
	{
		get
		{
			if (s_defaultOptions == null)
			{
				return s_defaultOptions = new ControllerElementGlyphSelectorOptions();
			}
			return s_defaultOptions;
		}
		set
		{
			s_defaultOptions = value;
		}
	}

	public virtual bool TryGetControllerTypeOrder(int index, out ControllerType controllerType)
	{
		if ((uint)index >= (uint)_controllerTypeOrder.Length)
		{
			controllerType = ControllerType.Keyboard;
			return false;
		}
		controllerType = _controllerTypeOrder[index];
		return true;
	}
}
