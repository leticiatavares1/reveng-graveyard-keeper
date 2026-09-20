using System;
using TMPro;

namespace LazyBearTechnology;

[Serializable]
public class TextTransition
{
	public TextMeshProUGUI targetLabel;

	public TextStyle defaultStyle;

	public TextStyle highlightedStyle;

	public TextStyle pressedStyle;

	public TextStyle selectedStyle;

	public TextStyle disabledStyle;
}
