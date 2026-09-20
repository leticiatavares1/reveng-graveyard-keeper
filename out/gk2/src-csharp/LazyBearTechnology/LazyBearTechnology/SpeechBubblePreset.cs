using System;
using UnityEngine;

namespace LazyBearTechnology;

[Serializable]
public class SpeechBubblePreset
{
	public Color backgroundColor = Color.white;

	public TextStyle textStyle;

	public TextStyle highlightedTextColorStyle;

	public Sprite backgroundSprite;

	public Sprite cornerSprite;

	public bool centered;

	public Vector2[] cornerOffset;

	public Vector2[] spriteOffset;
}
