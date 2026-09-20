using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rewired.Glyphs.UnityUI;

public class UnityUIGlyphOrTextTMPro : GlyphOrTextBase<Image, Sprite, TMP_Text>
{
	protected override string textString
	{
		get
		{
			if (!(base.textComponent != null))
			{
				return string.Empty;
			}
			return base.textComponent.text;
		}
		set
		{
			if (!(base.textComponent == null))
			{
				base.textComponent.text = value;
			}
		}
	}

	protected override Sprite glyphGraphic
	{
		get
		{
			if (!(base.glyphComponent != null))
			{
				return null;
			}
			return base.glyphComponent.sprite;
		}
		set
		{
			if (!(base.glyphComponent == null))
			{
				base.glyphComponent.sprite = value;
			}
		}
	}
}
