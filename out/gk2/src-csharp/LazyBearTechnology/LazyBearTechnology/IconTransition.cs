using System;
using UnityEngine;
using UnityEngine.UI;

namespace LazyBearTechnology;

[Serializable]
public class IconTransition
{
	public Image targetImage;

	public Sprite defaultSprite;

	public Sprite highlightedSprite;

	public Sprite pressedSprite;

	public Sprite selectedSprite;

	public Sprite disabledSprite;
}
