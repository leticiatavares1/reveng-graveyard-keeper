using UnityEngine;

[CreateAssetMenu(fileName = "NewImageColors", menuName = "UI/Image Colors", order = 1)]
public class ImageColors : ScriptableObject
{
	[SerializeField]
	private Color normalColor;

	[SerializeField]
	private Color highlightedColorMouse;

	public Color NormalColor => normalColor;

	public Color HighlightedColorMouse => highlightedColorMouse;
}
