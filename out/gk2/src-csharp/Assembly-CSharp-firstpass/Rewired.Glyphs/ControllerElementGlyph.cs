using System;

namespace Rewired.Glyphs;

public abstract class ControllerElementGlyph : ControllerElementGlyphBase
{
	[NonSerialized]
	private ActionElementMap _actionElementMap;

	[NonSerialized]
	private ControllerElementIdentifier _controllerElementIdentifier;

	[NonSerialized]
	private AxisRange _axisRange;

	public ActionElementMap actionElementMap
	{
		get
		{
			return _actionElementMap;
		}
		set
		{
			_actionElementMap = value;
		}
	}

	public ControllerElementIdentifier controllerElementIdentifier
	{
		get
		{
			return _controllerElementIdentifier;
		}
		set
		{
			_controllerElementIdentifier = value;
		}
	}

	public AxisRange axisRange
	{
		get
		{
			return _axisRange;
		}
		set
		{
			_axisRange = value;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!ReInput.isReady)
		{
			return;
		}
		if (_actionElementMap == null && controllerElementIdentifier == null)
		{
			Hide();
			return;
		}
		if (actionElementMap != null)
		{
			ShowGlyphsOrText(_actionElementMap);
		}
		else if (controllerElementIdentifier != null)
		{
			ShowGlyphsOrText(_controllerElementIdentifier, axisRange);
		}
		EvaluateObjectVisibility();
	}
}
