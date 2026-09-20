using System;
using UnityEngine;

namespace LazyBearTechnology;

public class PredefinedGUIStyle
{
	private Func<GUIStyle> baseStyleGetter;

	private Action<GUIStyle> initializer;

	private GUIStyle style;

	public GUIStyle Style
	{
		get
		{
			if (style == null)
			{
				style = new GUIStyle(baseStyleGetter());
				initializer(style);
			}
			return style;
		}
	}

	public PredefinedGUIStyle(Func<GUIStyle> baseStyleGetter, Action<GUIStyle> initializer)
	{
		this.baseStyleGetter = baseStyleGetter;
		this.initializer = initializer;
	}

	public static explicit operator GUIStyle(PredefinedGUIStyle s)
	{
		return s.Style;
	}
}
