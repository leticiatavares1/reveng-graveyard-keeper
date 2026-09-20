using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class TextStyleData
{
	public bool containsFontColor;

	public Color fontColor = Color.white;

	public bool containsOutline;

	public bool eightSide;

	public Color outlineColor = Color.white;

	public bool containsSecondOutline;

	public Color secondOutlineColor = Color.white;

	public bool containsShadow;

	public Color shadowOutlineColor = Color.white;

	public bool containsOverlayTexture;

	public Texture overlayTexture;

	public bool containsLineSpacing = true;

	public float lineSpacing;

	[Space(10f)]
	[Tooltip("If true, this style wouldn't be affected by any locale font changes (for Asian fonts).")]
	public bool isAlwaysStaticFont;
}
