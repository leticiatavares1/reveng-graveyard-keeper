using UnityEngine;

namespace Rewired.Glyphs.UnityUI;

[AddComponentMenu("Rewired/Glyphs/Unity UI/Unity UI Controller Element Glyph")]
public class UnityUIControllerElementGlyph : ControllerElementGlyph
{
	protected override GameObject GetDefaultGlyphOrTextPrefab()
	{
		return UnityUIControllerElementGlyphBase.defaultGlyphOrTextPrefab;
	}
}
