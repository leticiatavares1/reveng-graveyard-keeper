using UnityEngine;

namespace Rewired.Glyphs.UnityUI;

[AddComponentMenu("Rewired/Glyphs/Unity UI/Unity UI Default Controller Element Glyph Settings")]
public class UnityUIDefaultControllerElementGlyphSettings : DefaultControllerElementGlyphSettingsBase
{
	protected override void SetDefaultGlyphOrTextPrefab()
	{
		UnityUIControllerElementGlyphBase.defaultGlyphOrTextPrefab = base.glyphOrTextPrefab;
	}
}
